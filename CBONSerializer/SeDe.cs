using System;
using System.Collections.Generic;
using System.Text;
using static CbStyles.Parser.Reader;
using static CbStyles.Cbon.Parser.Parser;
using static CbStyles.Cbon.SeDe.De;
using static CbStyles.Cbon.SeDe.Se;
using CbStyles.Cbon.Errors;

namespace CbStyles.Cbon
{
    public static partial class SeDe
    {
        public static List<T> DoDeArr<T>(string code)
        {
            var ast = parser(reader(code));
            var t = CheckDeType(typeof(T));
            return ArrDe<T>(t, ast);
        }

        public static T DoDe<T>(string code)
        {
            var ast = parser(reader(code));
            var t = CheckDeType(typeof(T));
            if (ast.Count == 0) throw new DeserializeError("Nothing to deserialize");
            return ItemDe<T>(t, ast[0]);
        }

        public static string DoSeArr<T>(IList<T> items)
        {
            var t = CheckSeType(typeof(T));
            return ArrSe(t, items, new SeCtx());
        }

        public static string DoSe<T>(T value)
        {
            var t = CheckSeType(typeof(T));
            return ItemSe(t, value, new SeCtx());
        }
    }
}
