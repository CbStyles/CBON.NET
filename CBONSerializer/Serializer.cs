using CbStyles.Cbon.Errors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            public static void CheckSeType(Type t)
            {
                if (!t.IsSerializable && t.GetCustomAttribute<CbonAttribute>() == null && t.GetCustomAttribute<CbonUnionAttribute>() == null) throw new SerializeError("This type cannot be serialized");
                if (t.IsAbstract) throw new SerializeError("Cannot serialize abstract class");
                if (t.IsInterface) throw new SerializeError("Cannot serialize interface");
                if (t.IsCOMObject) throw new SerializeError("Cannot serialize COM object");
                if (t.IsPointer) throw new SerializeError("Cannot serialize pointer");
            }

            public static string ArrSe(Type t, List<object> items, SeCtx ctx)
            {
                return string.Concat((from item in items select ItemSe(t, item, ctx)));
            }

            public static string ItemSe(Type t, object o, SeCtx ctx)
            {
                if (o == null) return "null";
                if (t.IsPrimitive) return SePrimitive(t, o);
                if (typeof(string).IsAssignableFrom(t)) return SeStr(o);
                if (typeof(IEnumerable).IsAssignableFrom(t)) return SeArr(t, o, ctx);
                if (t.IsEnum) return SeEnum(t, o, ctx);
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

            public static string SePrimitive(Type t, object o)
            {
                return o switch
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
            }

            public static string SeArr(Type t, object o, SeCtx ctx)
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
                return $"[${string.Join(" ", f())}]";
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
                        
                    }
                }
                throw new NotImplementedException("todo");
            }

            public static string SeEnum(Type t, object o, SeCtx ctx)
            {
                throw new NotImplementedException("todo");
            }

            public static string SeUnion(Type t, object o, SeCtx ctx)
            {
                throw new NotImplementedException("todo");
            }
        }
    }
}
