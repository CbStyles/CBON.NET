using System;
using System.Collections.Generic;
using System.Text;
using static CbStyles.Parser.Reader;
using static CbStyles.Cbon.Parser.Parser;
using static CbStyles.Cbon.Cbon.De;
using static CbStyles.Cbon.Cbon.Se;
using CbStyles.Cbon.Errors;
using CbStyles.Cbon.Parser;
using System.Linq;

namespace CbStyles.Cbon
{
    public static partial class Cbon
    {

        public static string DoSeArr<T>(IEnumerable<T> items) => ArrSe(CheckSeType(typeof(T)), items, new SeCtx());

        public static string DoSe<T>(T value) => ItemSe(CheckSeType(typeof(T)), value, new SeCtx());


        public static List<T> DoDeArr<T>(string code) => DoDeAstArr<T>(Parse(code));

        public static T DoDe<T>(string code)
        {
            var ast = Parse(code);
            if (ast.Count == 0) throw new DeserializeError("Nothing to deserialize");
            return DoDeAst<T>(ast[0]);
        }

        public static CbVal DoDeValArr(string code) => DeCbVal.ArrDe(Parse(code));

        public static CbVal DoDeVal(string code)
        {
            var ast = Parse(code);
            if (ast.Count == 0) throw new DeserializeError("Nothing to deserialize");
            return DeCbVal.ValDe(ast[0]);
        }

        public static List<T> DoDeAstArr<T>(List<CbAst> ast) => ArrDe<T>(CheckDeType(typeof(T)), ast);

        public static T DoDeAst<T>(CbAst ast) => ItemDe<T>(CheckDeType(typeof(T)), ast);

        public static CbVal DoDeAstValArr(List<CbAst> ast) => DeCbVal.ArrDe(ast);

        public static CbVal DoDeAstVal(CbAst ast) => DeCbVal.ValDe(ast);

        public static List<CbAst> Parse(string code) => parser(reader(code));

    }
}
