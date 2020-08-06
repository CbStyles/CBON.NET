using System;
using System.Collections.Generic;
using System.Text;
using CbStyles.Cbon.Parser;
using CbStyles.Cbon.Errors;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace CbStyles.Cbon
{
    namespace Errors
    {
        public class DeserializeError : Exception
        {
            public DeserializeError(string message) : base(message) { }
        }
        public class DeserializeTypeError : Exception
        {
            public DeserializeTypeError(string typename, string tname) : base($"Cannot deserialize <{typename}> to => {tname}") { }
        }
    }

    public static partial class SeDe
    {
        internal static class De
        {
            public static void CheckDeType(Type t)
            {
                if (!t.IsSerializable) throw new DeserializeError("This type cannot be deserialized");
                if (t.IsAbstract) throw new DeserializeError("Cannot deserialize abstract class");
                if (t.IsInterface) throw new DeserializeError("Cannot deserialize interface");
                if (t.IsCOMObject) throw new DeserializeError("Cannot deserialize COM object");
                if (t.IsPointer) throw new DeserializeError("Cannot deserialize pointer");
            }

            public static List<T> ArrDe<T>(Type t, List<CbAst> ast) => ast.Count == 0 ? new List<T>() : (from v in ast select ItemDe<T>(t, v)).ToList();
            public static T ItemDe<T>(Type t, CbAst ast) => (T)ItemDe(t, ast);
            public static object ItemDe(Type t, CbAst ast) => t.IsPrimitive ? DePrimitive(t, ast) : typeof(string).IsAssignableFrom(t) ? DeStr(t, ast) : ast switch
            {
                CbAst.Obj { Item: var v } => DeObj(t, v),
                CbAst.Arr { Item: var v } => throw new NotImplementedException("todo"),
                var a when a.IsNull => null,
                var a when a.IsStr => throw new DeserializeTypeError("string", t.FullName),
                var a when a.IsNum => throw new DeserializeTypeError("number", t.FullName),
                var a when a.IsHex => throw new DeserializeTypeError("integer", t.FullName),
                var a when a.IsBool => throw new DeserializeTypeError("bool", t.FullName),
                _ => throw new NotImplementedException("never"),
            };

            public static object DeStr(Type t, CbAst ast) => ast switch
            {
                CbAst.Str { Item: var v } => v,
                CbAst.Bool { Item: var v } => v ? "true" : "false",
                CbAst.Num { Item: var v } => v.raw,
                CbAst.Hex { Item: var v } => v.raw,
                var a when a.IsNull => null,
                var a when a.IsArr => throw new DeserializeTypeError("array", t.FullName),
                var a when a.IsObj => throw new DeserializeTypeError("array", t.FullName),
                _ => throw new NotImplementedException("never"),
            };

            public static object DePrimitive(Type t, CbAst ast) => ast switch
            {
                CbAst.Bool { Item: var v } => typeof(bool).IsAssignableFrom(t) ? v : throw new DeserializeTypeError("bool", t.FullName),
                CbAst.Num { Item: var v } => t switch
                {
                    var _ when typeof(sbyte).IsAssignableFrom(t) => v.I8(),
                    var _ when typeof(short).IsAssignableFrom(t) => v.I16(),
                    var _ when typeof(int).IsAssignableFrom(t) => v.I32(),
                    var _ when typeof(long).IsAssignableFrom(t) => v.I64(),
                    var _ when typeof(byte).IsAssignableFrom(t) => v.U8(),
                    var _ when typeof(ushort).IsAssignableFrom(t) => v.U16(),
                    var _ when typeof(uint).IsAssignableFrom(t) => v.U32(),
                    var _ when typeof(ulong).IsAssignableFrom(t) => v.U64(),
                    var _ when typeof(float).IsAssignableFrom(t) => v.F32(),
                    var _ when typeof(double).IsAssignableFrom(t) => v.F64(),
                    var _ when typeof(decimal).IsAssignableFrom(t) => v.F128(),
                    var _ when typeof(bool).IsAssignableFrom(t) => v.F128() switch
                    {
                        1 => true,
                        0 => false,
                        _ => throw new DeserializeTypeError("number", t.FullName),
                    },
                    _ => throw new DeserializeTypeError("number", t.FullName),
                },
                CbAst.Hex { Item: var v } => t switch
                {
                    var _ when typeof(sbyte).IsAssignableFrom(t) => v.I8(),
                    var _ when typeof(short).IsAssignableFrom(t) => v.I16(),
                    var _ when typeof(int).IsAssignableFrom(t) => v.I32(),
                    var _ when typeof(long).IsAssignableFrom(t) => v.I64(),
                    var _ when typeof(byte).IsAssignableFrom(t) => v.U8(),
                    var _ when typeof(ushort).IsAssignableFrom(t) => v.U16(),
                    var _ when typeof(uint).IsAssignableFrom(t) => v.U32(),
                    var _ when typeof(ulong).IsAssignableFrom(t) => v.U64(),
                    var _ when typeof(float).IsAssignableFrom(t) => (float)v.I32(),
                    var _ when typeof(double).IsAssignableFrom(t) => (double)v.I64(),
                    var _ when typeof(decimal).IsAssignableFrom(t) => (decimal)v.U64(),
                    var _ when typeof(bool).IsAssignableFrom(t) => v.U64() switch
                    {
                        1 => true,
                        0 => false,
                        _ => throw new DeserializeTypeError("integer", t.FullName),
                    },
                    _ => throw new DeserializeTypeError("integer", t.FullName),
                },
                CbAst.Str { Item: var v } => typeof(char).IsAssignableFrom(t) && v.Length == 1 ? v[0] : throw new DeserializeTypeError("string", t.FullName),
                var a when a.IsNull => throw new DeserializeTypeError("null", t.FullName),
                var a when a.IsArr => throw new DeserializeTypeError("array", t.FullName),
                var a when a.IsObj => throw new DeserializeTypeError("object", t.FullName),
                _ => throw new NotImplementedException("never"),
            };
        
            public static object DeObj(Type t, Dictionary<string, CbAst> ast)
            {
                const BindingFlags ConstructorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                const BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public;

                var constructor = t.GetConstructor(ConstructorFlags, null, Type.EmptyTypes, null);
                var obj = constructor != null ? constructor.Invoke(Array.Empty<object>()) : t.GetConstructors(ConstructorFlags).Length == 0 ? 
                    FormatterServices.GetUninitializedObject(t) : throw new DeserializeError($"Cannot construct this type : {t.FullName}");

                var fields = t.GetFields(MemberFlags);
                foreach (var field in fields)
                {
                    if (ast.TryGetValue(field.Name, out var v))
                    {
                        var ft = field.FieldType;
                        CheckDeType(ft);
                        var va = ItemDe(ft, v);
                        field.SetValue(obj, va);
                    }
                }

                var props = t.GetProperties(MemberFlags);
                foreach (var prop in props)
                {
                    if (!prop.CanWrite) continue;
                    if (ast.TryGetValue(prop.Name, out var v))
                    {
                        var pt = prop.PropertyType;
                        CheckDeType(pt);
                        var va = ItemDe(pt, v);
                        prop.SetValue(obj, va);
                    }
                }

                return obj;
            }

        }
    }
    
}
