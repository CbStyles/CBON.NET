module TestParser.TestUnion

open NUnit.Framework
open CbStyles.Cbon.Parser
open CbStyles.Cbon.Parser.Parser
open CbStyles.Parser
open System.Collections.Generic


[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestUnion1 () =
    let code = Reader.reader "(asd) 123"
    let r = union code
    printf "%s" (r.ToString())
    Assert.Pass()
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Union (AUnion("asd", CbAst.Num (ANum "123")))), r)

[<Test>]
let TestUnion2 () =
    let code = Reader.reader "('asd') (123) true"
    let r = union code
    printf "%s" (r.ToString())
    Assert.Pass()
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Union (AUnion("asd", CbAst.Union (AUnion ("123", CbAst.Bool true))))), r)