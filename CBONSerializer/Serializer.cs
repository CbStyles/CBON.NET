#nullable enable
using CbStyles.Cbon.Errors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace CbStyles.Cbon
{
    namespace Errors
    {
        public class SerializeError : Exception
        {
            public SerializeError(string message) : base(message) { }
        }
        public class SerializeTypeError : SerializeError
        {
            public SerializeTypeError(string type) : base($"This type <{type}> cannot be serialized") { }
        }
    }

    public static partial class SeDe
    {
        internal static class Se
        {
            public struct SeCtx
            {
                public int tab;

                public SeCtx(int tab) => this.tab = tab;

                public string Tab => new string(' ', tab);
            }

            static readonly ConditionalWeakTable<Type, Type> CheckSeTypeTemp = new ConditionalWeakTable<Type, Type>();
            public static Type CheckSeType(Type t)
            {
                if(CheckSeTypeTemp.TryGetValue(t, out var _)) return t;
                if (!t.IsSerializable 
                    && t.GetCustomAttribute<CbonAttribute>() == null 
                    && t.GetCustomAttribute<CbonUnionAttribute>() == null 
                    && t.GetCustomAttribute<CbonUnionItemAttribute>() == null) throw new SerializeError("This type cannot be serialized");
                if (t.GetCustomAttribute<CbonUnionAttribute>() == null)
                {
                    if (t.IsAbstract) throw new SerializeError("Cannot serialize abstract class");
                    if (t.IsInterface) throw new SerializeError("Cannot serialize interface");
                }
                if (t.IsCOMObject) throw new SerializeError("Cannot serialize COM object");
                if (t.IsPointer) throw new SerializeError("Cannot serialize pointer");
                CheckSeTypeTemp.Add(t, t);
                return t;
            }

            public static string ArrSe(Type t, IEnumerable items, SeCtx ctx)
            {
                IEnumerable<object> f()
                {
                    foreach (var item in items)
                    {
                        yield return item;
                    }
                }
                return string.Join(" ", (from item in f() select ItemSe(t, item, ctx)));
            }

            public static string ItemSe(Type t, object o, SeCtx ctx)
            {
                if (o == null) return "null";
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) return SeNullable(t, o, ctx);
                if (t.IsPrimitive) return SePrimitive(t, o);
                if (typeof(string).IsAssignableFrom(t)) return SeStr(o);
                if (typeof(IEnumerable).IsAssignableFrom(t)) return SeArr(o, ctx);
                if (t.IsEnum) return SeEnum(t, o);
                if (t.GetCustomAttribute<CbonUnionAttribute>() != null) return SeUnion(t, o, ctx);
                return SeObj(t, o, ctx);
            }

            static Regex not_word_reg = new Regex(@"[\[\]\{\}\(\)'"":=,;\s]+", RegexOptions.Compiled);
            public static string SeStr(object o)
            {
                var s = o.ToString();
                if (s.Length == 0 || not_word_reg.IsMatch(s) || s == "true" || s == "false" || s == "null" || Parser.Parser.num_reg.IsMatch(s) || Parser.Parser.hex_reg.IsMatch(s)) return SeStrQuot(s);
                return s;
            }

            public static string SeStrQuot(string s)
            {
                return $"'{s.Replace("'", "\\'")}'";
            }

            public static string SePrimitive(Type t, object o) => o switch
            {
                bool v => v ? "true" : "false",
                byte v => v.ToString(),
                ushort v => v.ToString(),
                uint v => v.ToString(),
                ulong v => v.ToString(),
                sbyte v => v.ToString(),
                short v => v.ToString(),
                int v => v.ToString(),
                long v => v.ToString(),
                float v => v.ToString(),
                double v => v.ToString(),
                decimal v => v.ToString(),
                char v => v.ToString(),
                _ => throw new SerializeTypeError(t.FullName),
            };

            public static string SeNullable(Type t, object o, SeCtx ctx)
            {
                if (o == null) return "null";
                var Value = t.GetProperty("HasValue");
                var gt = CheckSeType(Nullable.GetUnderlyingType(t));
                return ItemSe(gt, Value.GetValue(o), ctx);
            }

            public static string SeArr(object o, SeCtx ctx)
            {
                var arr = (IEnumerable)(o);
                IEnumerable<string> f()
                {
                    foreach (var v in arr)
                    {
                        var t = v.GetType();
                        CheckSeType(t);
                        yield return ItemSe(t, v, ctx);
                    }
                }
                return $"[{string.Join(" ", f())}]";
            }

            public static string SeObj(Type t, object o, SeCtx ctx)
            {
                var cb = t.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                var items = new List<string>();
                if (cb.Member == 0) return "{}";
                if (cb.Member.HasFlag(CbonMember.Fields))
                { 
                    var fields = t.GetFields(BindingFlags.Instance | (cb.Member.HasFlag(CbonMember.Public) ? BindingFlags.Public : 0) | (cb.Member.HasFlag(CbonMember.Private) ? BindingFlags.NonPublic : 0));
                    foreach (var field in fields)
                    {
                        if (field.GetCustomAttribute<CbonIgnoreAttribute>() != null) continue;
                        if (cb.Member.HasFlag(CbonMember.OptIn)) if (field.GetCustomAttribute<CbonAttribute>() == null && field.GetCustomAttribute<DataContractAttribute>() == null) continue;
                        var fcb = field.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                        if (fcb.Ignore || field.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                        var name = SeKey(fcb.Name ?? field.Name);
                        var ft = CheckSeType(field.FieldType);
                        var fs = ItemSe(ft, field.GetValue(o), ctx);
                        items.Add($"{name} {fs}");
                    }
                }
                if (cb.Member.HasFlag(CbonMember.Properties))
                {
                    var props = t.GetProperties(BindingFlags.Instance | (cb.Member.HasFlag(CbonMember.Public) ? BindingFlags.Public : 0) | (cb.Member.HasFlag(CbonMember.Private) ? BindingFlags.NonPublic : 0));
                    foreach (var prop in props)
                    {
                        if (!prop.CanRead) continue;
                        if (prop.GetCustomAttribute<CbonIgnoreAttribute>() != null) continue;
                        if (cb.Member.HasFlag(CbonMember.OptIn)) if (prop.GetCustomAttribute<CbonAttribute>() == null && prop.GetCustomAttribute<DataContractAttribute>() == null) continue;
                        var pcb = prop.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                        if (pcb.Ignore || prop.GetCustomAttribute<NonSerializedAttribute>() != null) continue;
                        var name = SeKey(pcb.Name ?? prop.Name);
                        var pt = CheckSeType(prop.PropertyType);
                        var ps = ItemSe(pt, prop.GetValue(o), ctx);
                        items.Add($"{name} {ps}");
                    }
                }
                return $"{{{string.Join(" ", items)}}}";
            }

            public static string SeKey(string s)
            {
                if (s.Length == 0 || not_word_reg.IsMatch(s)) return SeStrQuot(s);
                return s;
            }

            public static string SeEnum(Type t, object o)
            {
                var cb = t.GetCustomAttribute<CbonAttribute>() ?? CbonAttribute.Default;
                if(cb.Union || t.GetCustomAttribute<CbonUnionAttribute>() != null)
                {
                    var raw_name = Enum.GetName(t, o);
                    var variant = t.GetField(raw_name, BindingFlags.Public | BindingFlags.Static);
                    var name = variant?.GetCustomAttribute<CbonAttribute>()?.Name ?? raw_name;
                    return SeStr(name);
                }
                return Convert.ChangeType(o, ((Enum)o).GetTypeCode()).ToString();
            }

            public static string SeUnion(Type t, object o, SeCtx ctx)
            {
                if (t.IsAbstract || t.IsInterface) return SeUnionClass(t, o, ctx);
                throw new SerializeError($"<{t.FullName}> cannot be union");
            }

            public static string SeUnionClass(Type t, object o, SeCtx ctx)
            {
                var ot = o.GetType();
                var cbu = t.GetCustomAttribute<CbonUnionAttribute>();
                var variants = cbu.CheckItems(t);
                (string, Type)? getVariant()
                {
                    foreach (var variant in variants)
                    {
                        if (variant.Value.IsAssignableFrom(ot)) return (variant.Key, variant.Value);
                    }
                    return null;
                }
                var variant = getVariant();
                if(variant == null) throw new SerializeError($"<{ot.FullName}> is not variant of <{t.FullName}>");
                (string vn, Type vt) = variant.Value;
                CheckSeType(vt);
                var val = ItemSe(vt, o, ctx);
                var tag = SeKey(vn);
                return $"({tag}){val}";
            }

        }
    }
}
