using CbStyles.Cbon.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CbStyles.Cbon
{
    public static partial class Cbon
    {
        internal static class DeCbVal
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
                    CbAst.Union { Item: var v } => UnionDe(v.tag, v.value),
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
