module TestParser.TestWord

open NUnit.Framework
open CbStyles.Cbon.Parser
open CbStyles.Cbon.Parser.Parser
open CbStyles.Parser

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestWord1 () =
    let code = Reader.reader "asd"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Str "asd"), r)

[<Test>]
let TestWord2 () =
    let code = Reader.reader "123"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Num <| ANum "123"), r)

[<Test>]
let TestWord3 () =
    let code = Reader.reader "true"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Bool true), r)

[<Test>]
let TestWord4 () =
    let code = Reader.reader "false"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Bool false), r)

[<Test>]
let TestWord5 () =
    let code = Reader.reader "0x2a5f"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Hex <| AHex "2a5f"), r)

[<Test>]
let TestWord6 () =
    let code = Reader.reader "123.456"
    let r = word code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Num <| ANum "123.456"), r)