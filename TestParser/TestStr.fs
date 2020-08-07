module TestParser.TestStr

open NUnit.Framework
open CbStyles.Cbon.Parser
open CbStyles.Cbon.Parser.Parser
open CbStyles.Parser

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestStr1 () =
    let code = Reader.reader "'asd'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), "asd"), r)

[<Test>]
let TestStr2 () =
    let code = Reader.reader "\"asd\""
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), "asd"), r)

[<Test>]
let TestStr3 () =
    let code = Reader.reader "'a\nd'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), "a\nd"), r)

[<Test>]
let TestStrEscape1 () =
    let code = Reader.reader "'a\\nd'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), "a\nd"), r)

[<Test>]
let TestStrEscape2 () =
    let code = Reader.reader "'a\\'d'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), "a'd"), r)

[<Test>]
let TestStrEscape3 () =
    let code = Reader.reader "'a\\u2A5F'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), "a\u2a5f"), r)

[<Test>]
let TestStrEscape4 () =
    let code = Reader.reader "'a\\u{2A5F}'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), "a\u2a5f"), r)

[<Test>]
let TestStrEscape5 () =
    let code = Reader.reader "'a\\u{u}'"
    let f () = str code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)
    ()

[<Test>]
let TestStrEscape6 () =
    let code = Reader.reader "'a\\u{}'"
    let f () = str code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)
    ()
    
[<Test>]
let TestStrEscape7 () =
    let code = Reader.reader "'a\\u{'"
    let f () = str code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)
    ()