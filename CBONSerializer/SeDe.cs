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

        internal class DeCbVal
        {
            public static CbVal ValDe(CbAst ast)
            {
                return ast switch
                {
                    CbAst.Bool { Item: var v } => CbVal.NewBool(v),
                    CbAst.Num { Item: var v } => CbVal.NewNum(v.F128()),
                    CbAst.Hex { Item: var v } => CbVal.NewNum(v.U64()),
                    CbAst.Str { Item: var v } => CbVal.NewStr(v),
                    CbAst.Arr { Item: var v } => ArrDe(v),
                    CbAst.Obj { Item: var v } => ObjDe(v),
                    CbAst.Union {  Item: var v } => UnionDe(v.tag, v.value),
                    var a when a.IsNull => null,
                    _ => throw new NotImplementedException("never")
                };
            }

            public static CbVal ArrDe(List<CbAst> asts) => CbVal.NewArr((from ast in asts select ValDe(ast)).ToList());

            public static CbVal ObjDe(Dictionary<string, CbAst> asts)
            {
                var obj = new Dictionary<string, CbVal>();
                foreach (var (key, ast) in asts)
                {
                    obj.Add(key, ValDe(ast));
                }
                return CbVal.NewObj(obj);
            }

            public static CbVal UnionDe(string tag, CbAst ast) => CbVal.NewUnion(tag, ValDe(ast));
        }
    }
}
