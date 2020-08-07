module TestParser.Test

open NUnit.Framework
open CbStyles.Cbon.Parser
open CbStyles.Cbon.Parser.Parser
open CbStyles.Parser
open System.Collections.Generic

[<SetUp>]
let Setup () =
    ()

[<Test>]
let Test1 () =
    let code = Reader.reader "{a 1 b '2' c null d a e [] f {}}"
    let r = parser code
    let s = CbAst.ToString r
    printf "%s" s
    Assert.AreEqual("""[ { "a": 1, "b": "2", "c": null, "d": "a", "e": [ ], "f": { } } ]""", s)