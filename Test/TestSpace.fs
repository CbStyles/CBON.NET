module TestParser.TestSpace

open NUnit.Framework
open CbStyle.Cbon.Parser
open CbStyle.Cbon.Parser.Parser
open CbStyle.Cbon.Parser.Reader
open CbStyle.Cbon

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestSpace1 () =
    let code = reader "  "
    let r = space code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), ()), r)