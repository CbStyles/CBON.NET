module TestParser.TestWord

open NUnit.Framework
open CbStyle.Cbon.Parser
open CbStyle.Cbon.Parser.Parser
open CbStyle.Cbon.Parser.Reader
open CbStyle.Cbon

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestWord1 () =
    let code = reader "asd"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), CbAst.Str "asd"), r)

[<Test>]
let TestWord2 () =
    let code = reader "123"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), CbAst.Num <| ANum "123"), r)

[<Test>]
let TestWord3 () =
    let code = reader "true"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), CbAst.Bool true), r)

[<Test>]
let TestWord4 () =
    let code = reader "false"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), CbAst.Bool false), r)

[<Test>]
let TestWord5 () =
    let code = reader "0x2a5f"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), CbAst.Hex <| AHex "2a5f"), r)

[<Test>]
let TestWord6 () =
    let code = reader "123.456"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), CbAst.Num <| ANum "123.456"), r)