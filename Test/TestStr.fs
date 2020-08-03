module TestParser.TestStr

open NUnit.Framework
open CbStyle.Cbon.Parser
open CbStyle.Cbon.Parser.Parser
open CbStyle.Cbon.Parser.Reader
open CbStyle.Cbon

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestStr1 () =
    let code = reader "'asd'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), "asd"), r)

[<Test>]
let TestStr2 () =
    let code = reader "\"asd\""
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), "asd"), r)

[<Test>]
let TestStr3 () =
    let code = reader "'a\nd'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), "a\nd"), r)

[<Test>]
let TestStrEscape1 () =
    let code = reader "'a\\nd'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), "a\nd"), r)

[<Test>]
let TestStrEscape2 () =
    let code = reader "'a\\'d'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), "a'd"), r)

[<Test>]
let TestStrEscape3 () =
    let code = reader "'a\\u2A5F'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), "a\u2a5f"), r)

[<Test>]
let TestStrEscape4 () =
    let code = reader "'a\\u{2A5F}'"
    let r = str code
    printf "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>(ref [||]), "a\u2a5f"), r)

[<Test>]
let TestStrEscape5 () =
    let code = reader "'a\\u{u}'"
    let f () = str code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)
    ()

[<Test>]
let TestStrEscape6 () =
    let code = reader "'a\\u{}'"
    let f () = str code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)
    ()
    
[<Test>]
let TestStrEscape7 () =
    let code = reader "'a\\u{'"
    let f () = str code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)
    ()