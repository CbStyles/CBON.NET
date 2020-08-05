module TestParser.TestSpace

open NUnit.Framework
open CbStyles.Cbon.Parser
open CbStyles.Cbon.Parser.Parser
open CbStyles.Parser

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestSpace1 () =
    let code = Reader.reader "  "
    let r = space code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), ()), r)