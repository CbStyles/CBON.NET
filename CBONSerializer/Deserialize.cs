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
            public DeserializeError(string message, Exception innerException) : base(message, innerException) { } 
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
            public static Type CheckDeType(Type t)
            {
                if (!t.IsSerializable
                    && t.GetCustomAttribute<CbonAttribute>() == null
                    && t.GetCustomAttribute<CbonUnionAttribute>() == null
                    && t.GetCustomAttribute<CbonUnionItemAttribute>() == null) throw new DeserializeError("This type cannot be deserialized");
                if (t.IsAbstract) throw new DeserializeError("Cannot deserialize abstract class");
                if (t.IsInterface) throw new DeserializeError("Cannot deserialize interface");
                if (t.IsCOMObject) throw new DeserializeError("Cannot deserialize COM object");
                if (t.IsPointer) throw new DeserializeError("Cannot deserialize pointer");
                return t;
            }

            public static List<T> ArrDe<T>(Type t, List<CbAst> ast) => ast.Count == 0 ? new List<T>() : (from v in ast select ItemDe<T>(t, v)).ToList();
            public static T ItemDe<T>(Type t, CbAst ast) => (T)ItemDe(t, ast);
            public static object ItemDe(Type t, CbAst ast) => t.IsPrimitive ? DePrimitive(t, ast) : t.IsAssignableFrom(typeof(string)) ? DeStr(t, ast) : ast switch
            {
                CbAst.Obj { Item: var v } => DeObj(t, v),
                CbAst.Arr { Item: var v } => DeArr(t, v),
                CbAst.Union _ => throw new NotImplementedException("todo"),
                var a when a.IsNull => null,
                _ => DePrimitive(t, ast),
            };

            public static object DeStr(Type t, CbAst ast) => ast switch
            {
                CbAst.Str { Item: var v } => v,
                CbAst.Bool { Item: var v } => v ? "true" : "false",
                CbAst.Num { Item: var v } => v.raw,
                CbAst.Hex { Item: var v } => v.raw,
                var a when a.IsNull => null,
                var a when a.IsArr => throw new DeserializeTypeError("array", t.FullName),
                var a when a.IsObj => throw new DeserializeTypeError("object", t.FullName),
                var a when a.IsUnion => throw new DeserializeTypeError("union", t.FullName),
                _ => throw new NotImplementedException("never"),
            };

            public static object DePrimitive(Type t, CbAst ast) => ast switch
            {
                CbAst.Bool { Item: var v } => t.IsAssignableFrom(typeof(bool)) ? v : throw new DeserializeTypeError("bool", t.FullName),
                CbAst.Num { Item: var v } => t switch
                {
                    var _ when t.IsAssignableFrom(typeof(sbyte)) => v.I8(),
                    var _ when t.IsAssignableFrom(typeof(short)) => v.I16(),
                    var _ when t.IsAssignableFrom(typeof(int)) => v.I32(),
                    var _ when t.IsAssignableFrom(typeof(long)) => v.I64(),
                    var _ when t.IsAssignableFrom(typeof(byte)) => v.U8(),
                    var _ when t.IsAssignableFrom(typeof(ushort)) => v.U16(),
                    var _ when t.IsAssignableFrom(typeof(uint)) => v.U32(),
                    var _ when t.IsAssignableFrom(typeof(ulong)) => v.U64(),
                    var _ when t.IsAssignableFrom(typeof(float)) => v.F32(),
                    var _ when t.IsAssignableFrom(typeof(double)) => v.F64(),
                    var _ when t.IsAssignableFrom(typeof(decimal)) => v.F128(),
                    var _ when t.IsAssignableFrom(typeof(bool)) => v.F128() switch
                    {
                        1 => true,
                        0 => false,
                        _ => throw new DeserializeTypeError("number", t.FullName),
                    },
                    _ => throw new DeserializeTypeError("number", t.FullName),
                },
                CbAst.Hex { Item: var v } => t switch
                {
                    var _ when t.IsAssignableFrom(typeof(sbyte)) => v.I8(),
                    var _ when t.IsAssignableFrom(typeof(short)) => v.I16(),
                    var _ when t.IsAssignableFrom(typeof(int)) => v.I32(),
                    var _ when t.IsAssignableFrom(typeof(long)) => v.I64(),
                    var _ when t.IsAssignableFrom(typeof(byte)) => v.U8(),
                    var _ when t.IsAssignableFrom(typeof(ushort)) => v.U16(),
                    var _ when t.IsAssignableFrom(typeof(uint)) => v.U32(),
                    var _ when t.IsAssignableFrom(typeof(ulong)) => v.U64(),
                    var _ when t.IsAssignableFrom(typeof(float)) => (float)v.I32(),
                    var _ when t.IsAssignableFrom(typeof(double)) => (double)v.I64(),
                    var _ when t.IsAssignableFrom(typeof(decimal)) => (decimal)v.U64(),
                    var _ when t.IsAssignableFrom(typeof(bool)) => v.U64() switch
                    {
                        1 => true,
                        0 => false,
                        _ => throw new DeserializeTypeError("integer", t.FullName),
                    },
                    _ => throw new DeserializeTypeError("integer", t.FullName),
                },
                CbAst.Str { Item: var v } => t.IsAssignableFrom(typeof(char)) && v.Length == 1 ? v[0] : throw new DeserializeTypeError("string", t.FullName),
                var a when a.IsNull => throw new DeserializeTypeError("null", t.FullName),
                var a when a.IsArr => throw new DeserializeTypeError("array", t.FullName),
                var a when a.IsObj => throw new DeserializeTypeError("object", t.FullName),
                var a when a.IsUnion => throw new DeserializeTypeError("union", t.FullName),
                _ => throw new NotImplementedException("never"),
            };

            const BindingFlags ConstructorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            public static object DeObj(Type t, Dictionary<string, CbAst> ast)
            {
                var cb = t.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                var constructor = t.GetConstructor(ConstructorFlags, null, Type.EmptyTypes, null);
                var obj = constructor != null ? constructor.Invoke(Array.Empty<object>()) : t.GetConstructors(ConstructorFlags).Length == 0 ? 
                    FormatterServices.GetUninitializedObject(t) : throw new DeserializeError($"Cannot construct this type : {t.FullName}");
                if (cb.Member == 0) return obj;

                if (cb.Member.HasFlag(CbonMember.Fields))
                {
                    var fields = t.GetFields(BindingFlags.Instance | (cb.Member.HasFlag(CbonMember.Public) ? BindingFlags.Public : 0) | (cb.Member.HasFlag(CbonMember.Private) ? BindingFlags.NonPublic : 0));
                    foreach (var field in fields)
                    {
                        if (field.GetCustomAttribute<CbonIgnoreAttribute>() != null) continue;
                        if (cb.Member.HasFlag(CbonMember.OptIn)) if (field.GetCustomAttribute<CbonAttribute>() == null && field.GetCustomAttribute<DataContractAttribute>() == null) continue;
                        var fcb = field.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                        if (fcb.Ignore || field.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                        var name = fcb.Name ?? field.Name;
                        if (ast.TryGetValue(name, out var v))
                        {
                            var ft = field.FieldType;
                            CheckDeType(ft);
                            var va = ItemDe(ft, v);
                            field.SetValue(obj, va);
                        } 
                        else if(cb.Required || fcb.Required) throw new DeserializeError($"Need <{t.FullName}.{field.Name}> but not found");
                    }
                }

                if (cb.Member.HasFlag(CbonMember.Properties))
                {
                    var props = t.GetProperties(BindingFlags.Instance | (cb.Member.HasFlag(CbonMember.Public) ? BindingFlags.Public : 0) | (cb.Member.HasFlag(CbonMember.Private) ? BindingFlags.NonPublic : 0));
                    foreach (var prop in props)
                    {
                        if (!prop.CanWrite) continue;
                        if (prop.GetCustomAttribute<CbonIgnoreAttribute>() != null) continue;
                        if (cb.Member.HasFlag(CbonMember.OptIn)) if (prop.GetCustomAttribute<CbonAttribute>() == null && prop.GetCustomAttribute<DataContractAttribute>() == null) continue;
                        var pcb = prop.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                        if (pcb.Ignore || prop.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                        var name = pcb.Name ?? prop.Name;
                        if (ast.TryGetValue(name, out var v))
                        {
                            var pt = prop.PropertyType;
                            CheckDeType(pt);
                            var va = ItemDe(pt, v);
                            prop.SetValue(obj, va);
                        }
                    }
                }

                return obj;
            }

            public static readonly Type ic = typeof(ICollection<>);
            public static object DeArr(Type t, List<CbAst> asts)
            {
                if(t.IsArray)
                {
                    var et = CheckDeType(t.GetElementType());
                    var arr = Array.CreateInstance(et, asts.Count);
                    foreach ((var ast, var i) in asts.Select((v, i) => (v, i)))
                    {
                        var v = ItemDe(et, ast);
                        arr.SetValue(v, i);
                    }
                    return arr;
                }

                var ifs = t.GetInterfaces();
                var ics = (from i in ifs where i.IsGenericType && i.GetGenericTypeDefinition() == ic select i).ToList();
                if (ics.Count == 0) throw new DeserializeError($"Deserialize <array> need target <{t.FullName}> implement <{ic.FullName}>");
                foreach (var ic in ics) CheckDeType(ic.GetGenericArguments()[0]);

                var constructor = t.GetConstructor(ConstructorFlags, null, Type.EmptyTypes, null);
                var obj = constructor != null ? constructor.Invoke(Array.Empty<object>()) : t.GetConstructors(ConstructorFlags).Length == 0 ?
                    FormatterServices.GetUninitializedObject(t) : throw new DeserializeError($"Cannot construct this type : {t.FullName}");

                var errs = new List<DeserializeError>();
                foreach (var ast in asts)
                {
                    
                    foreach (var ic in ics)
                    {
                        var gt = ic.GetGenericArguments()[0];
                        try
                        {
                            var f = ic.GetMethod("Add", new[] { gt });
                            var v = ItemDe(gt, ast);
                            f.Invoke(obj, new[] { v });
                            goto aloop;
                        } 
                        catch(DeserializeError deErr)
                        {
                            errs.Add(deErr);
                        }
                    }
                    if(errs.Count > 0) throw new DeserializeError("Multiple error", new AggregateException(errs));
                    aloop:
                    continue;
                }
                return obj;
            }

        }
    }
    
}
