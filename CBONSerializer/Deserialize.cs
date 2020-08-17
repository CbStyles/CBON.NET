#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using CbStyles.Cbon.Parser;
using CbStyles.Cbon.Errors;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Globalization;

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

    public static partial class Cbon
    {
        internal static class De
        {

            static readonly ConditionalWeakTable<Type, Type> CheckDeTypeTemp = new ConditionalWeakTable<Type, Type>();

            public static Type CheckDeType(Type t)
            {
                if (CheckDeTypeTemp.TryGetValue(t, out var _)) return t;
                if (!t.IsSerializable
                    && t.GetCustomAttribute<CbonAttribute>() == null
                    && t.GetCustomAttribute<CbonUnionAttribute>() == null
                    && t.GetCustomAttribute<CbonUnionItemAttribute>() == null) throw new DeserializeError("This type cannot be deserialized");
                if (t.GetCustomAttribute<CbonUnionAttribute>() == null)
                {
                    if (t.IsAbstract) throw new DeserializeError("Cannot deserialize abstract class");
                    if (t.IsInterface) throw new DeserializeError("Cannot deserialize interface");
                }
                if (t.IsCOMObject) throw new DeserializeError("Cannot deserialize COM object");
                if (t.IsPointer) throw new DeserializeError("Cannot deserialize pointer");
                CheckDeTypeTemp.Add(t, t);
                return t;
            }

            public static List<T> ArrDe<T>(Type t, List<CbAst> ast) => ast.Count == 0 ? new List<T>() : (from v in ast select ItemDe<T>(t, v)).ToList();
            public static T ItemDe<T>(Type t, CbAst ast) => (T)ItemDe(t, ast)!;
            public static object? ItemDe(Type t, CbAst ast) => t.IsPrimitive ? DePrimitive(t, ast) : t.IsEnum ? DeEnum(t, ast) : t.IsAssignableFrom(typeof(string)) ? DeStr(t, ast) : ast switch
            {
                CbAst.Obj { Item: var v } => DeObj(t, v),
                CbAst.Arr { Item: var v } => DeArr(t, v),
                CbAst.Union { Item: AUnion { tag: var tag, value: var val } } => DeUnion(t, tag, val),
                var a when a.IsNull => null,
                _ => DePrimitive(t, ast),
            };

            public static object? DeStr(Type t, CbAst ast) => ast switch
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
                if (t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)) != null) return DeMap(t, ast);

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
                    if (t.GetArrayRank() > 1) throw new DeserializeError("Not support multidimensional arrays");
                    var et = CheckDeType(t.GetElementType());
                    var arr = Array.CreateInstance(et, asts.Count);
                    foreach ((var ast, var i) in asts.Select((v, i) => (v, i)))
                    {
                        var v = ItemDe(et, ast);
                        arr.SetValue(v, i);
                    }
                    return arr;
                } 
                else if(t.IsGenericType)
                {
                    var tgd = t.GetGenericTypeDefinition();
                    if (tgd == typeof(List<>))
                    {
                        var et = CheckDeType(t.GetGenericArguments()[0]);
                        var listctor = t.GetConstructor(ConstructorFlags, null, new[] { typeof(int) }, null);
                        var list = listctor.Invoke(new object[] { asts.Count });
                        var add = t.GetMethod("Add");
                        foreach (var ast in asts)
                        {
                            var v = ItemDe(et, ast);
                            add.Invoke(list, new object?[] { v });
                        }
                        return list;
                    }
                }

                var ifs = t.GetInterfaces();
                var ics = (from i in ifs where i.IsGenericType && i.GetGenericTypeDefinition() == ic select i).ToArray();
                if (ics.Length == 0) throw new DeserializeError($"Deserialize <array> need target <{t.FullName}> implement <{ic.FullName}>");

                var constructor = t.GetConstructor(ConstructorFlags, null, Type.EmptyTypes, null);
                var obj = constructor != null ? constructor.Invoke(Array.Empty<object>()) : t.GetConstructors(ConstructorFlags).Length == 0 ?
                    FormatterServices.GetUninitializedObject(t) : throw new DeserializeError($"Cannot construct this type : {t.FullName}");

                var errs = new List<DeserializeError>();
                foreach (var ast in asts)
                {
                    
                    foreach (var ic in ics)
                    {
                        var gt = CheckDeType(ic.GetGenericArguments()[0]);
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
                    errs.Clear();
                    continue;
                }
                return obj;
            }

            public static readonly Type id = typeof(IDictionary<,>);
            public static object DeMap(Type t, Dictionary<string, CbAst> asts)
            {
                var ids = t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)).ToArray();
                if (ids.Length == 0) throw new DeserializeError($"Deserialize <map> need target <{t.FullName}> implement <{id.FullName}>");

                var constructor = t.GetConstructor(ConstructorFlags, null, Type.EmptyTypes, null);
                var obj = constructor != null ? constructor.Invoke(Array.Empty<object>()) : t.GetConstructors(ConstructorFlags).Length == 0 ?
                    FormatterServices.GetUninitializedObject(t) : throw new DeserializeError($"Cannot construct this type : {t.FullName}");

                var errs = new List<DeserializeError>();
                foreach (var kv in asts)
                {
                    var k = kv.Key;
                    var ast = kv.Value;
                    foreach (var id in ids)
                    {
                        var kt = id.GetGenericArguments()[0];
                        var et = CheckDeType(id.GetGenericArguments()[1]);
                        try
                        {
                            var key = DeKey(kt, k);
                            var f = id.GetMethod("Add", new[] { kt, et });
                            var v = ItemDe(et, ast);
                            f.Invoke(obj, new[] { key, v });
                            goto aloop;
                        }
                        catch (DeserializeError deErr)
                        {
                            errs.Add(deErr);
                        }
                    }
                    if (errs.Count > 0) throw new DeserializeError("Multiple error", new AggregateException(errs));
                    aloop:
                    errs.Clear();
                    continue;
                }

                return obj;
            }

            public static object? DeKey(Type t, string o)
            {
                if (t.IsAssignableFrom(typeof(string))) return o;
                if (o == "null") return null;
                if (t.IsAssignableFrom(typeof(bool))) return bool.Parse(o);
                if (t.IsAssignableFrom(typeof(sbyte))) return sbyte.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(short))) return short.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(int))) return int.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(long))) return long.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(byte))) return byte.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(ushort))) return ushort.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(uint))) return uint.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(ulong))) return ulong.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(float))) return float.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(double))) return double.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(decimal))) return decimal.Parse(o, NumberStyles.Any, null);
                if (t.IsAssignableFrom(typeof(char)) && o.Length == 1) return o[0];
                throw new DeserializeTypeError("key", t.FullName);
            }

            public static object DeEnum(Type t, CbAst ast)
            {
                var cb = t.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                if (cb.Union || t.GetCustomAttribute<CbonUnionAttribute>() != null)
                {
                    (var name, var go) = ast switch
                    {
                        CbAst.Str { Item: var v } => (v, false),
                        var a when a.IsBool => throw new DeserializeTypeError("bool", t.FullName),
                        var a when a.IsNull => throw new DeserializeTypeError("null", t.FullName),
                        var a when a.IsArr => throw new DeserializeTypeError("array", t.FullName),
                        var a when a.IsObj => throw new DeserializeTypeError("object", t.FullName),
                        var a when a.IsUnion => throw new DeserializeTypeError("union", t.FullName),
                        _ => (null, true)
                    };
                    if (go) goto numenum;
                    var variant = t.GetFields(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m => m.DeclaringType == t && m.GetCustomAttribute<CbonAttribute>()?.Name == name);
                    if (variant != null) return variant.GetValue(null);
                    return Enum.Parse(t, name, true);
                }
            numenum:
                var it = Enum.GetUnderlyingType(t);
                var o = ast switch
                {
                    CbAst.Num { Item: var v } => t switch
                    {
                        var _ when it.IsAssignableFrom(typeof(sbyte)) => Enum.ToObject(t, v.I8()),
                        var _ when it.IsAssignableFrom(typeof(short)) => Enum.ToObject(t, v.I16()),
                        var _ when it.IsAssignableFrom(typeof(int)) => Enum.ToObject(t, v.I32()),
                        var _ when it.IsAssignableFrom(typeof(long)) => Enum.ToObject(t, v.I64()),
                        var _ when it.IsAssignableFrom(typeof(byte)) => Enum.ToObject(t, v.U8()),
                        var _ when it.IsAssignableFrom(typeof(ushort)) => Enum.ToObject(t, v.U16()),
                        var _ when it.IsAssignableFrom(typeof(uint)) => Enum.ToObject(t, v.U32()),
                        var _ when it.IsAssignableFrom(typeof(ulong)) => Enum.ToObject(t, v.U64()),
                        _ => throw new DeserializeTypeError("number", t.FullName),
                    },
                    CbAst.Hex { Item: var v } => t switch
                    {
                        var _ when it.IsAssignableFrom(typeof(sbyte)) => Enum.ToObject(t, v.I8()),
                        var _ when it.IsAssignableFrom(typeof(short)) => Enum.ToObject(t, v.I16()),
                        var _ when it.IsAssignableFrom(typeof(int)) => Enum.ToObject(t, v.I32()),
                        var _ when it.IsAssignableFrom(typeof(long)) => Enum.ToObject(t, v.I64()),
                        var _ when it.IsAssignableFrom(typeof(byte)) => Enum.ToObject(t, v.U8()),
                        var _ when it.IsAssignableFrom(typeof(ushort)) => Enum.ToObject(t, v.U16()),
                        var _ when it.IsAssignableFrom(typeof(uint)) => Enum.ToObject(t, v.U32()),
                        var _ when it.IsAssignableFrom(typeof(ulong)) => Enum.ToObject(t, v.U64()),
                        _ => throw new DeserializeTypeError("integer", t.FullName),
                    },
                    var a when a.IsStr => throw new DeserializeTypeError("string", t.FullName),
                    var a when a.IsBool => throw new DeserializeTypeError("bool", t.FullName),
                    var a when a.IsNull => throw new DeserializeTypeError("null", t.FullName),
                    var a when a.IsArr => throw new DeserializeTypeError("array", t.FullName),
                    var a when a.IsObj => throw new DeserializeTypeError("object", t.FullName),
                    var a when a.IsUnion => throw new DeserializeTypeError("union", t.FullName),
                    _ => throw new NotImplementedException("never"),
                };
                return o;
            }

            public static object? DeUnion(Type t, string tag, CbAst ast)
            {
                if (t.IsAbstract || t.IsInterface) return DeUnionClass(t, tag, ast);
                throw new DeserializeTypeError("union", t.FullName);
            }

            public static object? DeUnionClass(Type t, string tag, CbAst ast)
            {
                var cbu = t.GetCustomAttribute<CbonUnionAttribute>();
                if (cbu == null) throw new DeserializeError($"Cannot deserialize <union> to => {t.FullName} , the target not have <{typeof(CbonUnionAttribute).FullName}>");
                var variants = cbu.CheckItems(t);
                Type? getVariant()
                {
                    foreach (var variant in variants)
                    {
                        if (variant.Key == tag) return variant.Value;
                    }
                    return null;
                }
                var variant = getVariant();
                if (variant == null) throw new DeserializeError($"<{t.FullName}> does not have a variant with tag {Se.SeStrQuot(tag)}");
                if (!t.IsAssignableFrom(variant)) throw new DeserializeError($"the variant <{variant.FullName}> not assignable to the union <{t.FullName}>");
                CheckDeType(variant);
                return ItemDe(variant, ast);
            }

        }
    }
}
