using System;
using System.Collections.Generic;
using System.Text;
using static CbStyles.Parser.Reader;
using static CbStyles.Cbon.Parser.Parser;
using static CbStyles.Cbon.SeDe.De;
using CbStyles.Cbon.Errors;

namespace CbStyles.Cbon
{
    public static partial class SeDe
    {
        public static List<T> DoDeArr<T>(string code)
        {
            var ast = parser(reader(code));
            var t = typeof(T);
            CheckDeType(t);
            return ArrDe<T>(t, ast);
        }

        public static T DoDe<T>(string code)
        {
            var ast = parser(reader(code));
            var t = typeof(T);
            CheckDeType(t);
            if (ast.Count == 0) throw new DeserializeError("Nothing to deserialize");
            return ItemDe<T>(t, ast[0]);
        }
    }
}
