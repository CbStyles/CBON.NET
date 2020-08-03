module TestParser.Test

open NUnit.Framework
open CbStyle.Cbon.Parser
open CbStyle.Cbon.Parser.Parser
open CbStyle.Cbon.Parser.Reader
open CbStyle.Cbon
open System.Collections.Generic

[<SetUp>]
let Setup () =
    ()

[<Test>]
let Test1 () =
    let code = reader "{a 1 b '2' c null d a e [] f {}}"
    let r = parser code
    let s = CbAst.ToString r
    printf "%s" s
    Assert.AreEqual("""[ { "a": 1, "b": "2", "c": null, "d": "a", "e": [ ], "f": { } } ]""", s)