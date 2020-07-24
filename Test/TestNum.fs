module TestParser.TestNum

open NUnit.Framework
open CbStyle.Cbon.Parser
open CbStyle.Cbon.Reader
open CbStyle.Cbon

[<SetUp>]
let Setup () =
    ()

//[<Test>]
//let TestNum1 () =
//    let code = reader "0"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("0")), r)

//[<Test>]
//let TestNum2 () =
//    let code = reader "123"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("123")), r)

//[<Test>]
//let TestNum3 () =
//    let code = reader "1.5"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("1.5")), r)

//[<Test>]
//let TestNum4 () =
//    let code = reader ".5"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num(".5")), r)

//[<Test>]
//let TestNum5 () =
//    let code = reader ".__5"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num(".__5")), r)

//[<Test>]
//let TestNum6 () =
//    let code = reader "123_456"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("123_456")), r)

//[<Test>]
//let TestNum7 () =
//    let code = reader "5."
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("5.")), r)

//[<Test>]
//let TestNum8 () =
//    let code = reader "123e5"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("123e5")), r)

//[<Test>]
//let TestNum9 () =
//    let code = reader "123.456e5"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("123.456e5")), r)

//[<Test>]
//let TestNum10 () =
//    let code = reader "123e-5"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("123e-5")), r)

//[<Test>]
//let TestNum11 () =
//    let code = reader "123e+5"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("123e+5")), r)

//[<Test>]
//let TestNum12 () =
//    let code = reader "123e__5"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("123e__5")), r)

//[<Test>]
//let TestNum13 () =
//    let code = reader "123e__"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref <| "e__".ToCharArray()), new Num("123")), r)

//[<Test>]
//let TestNum14 () =
//    let code = reader "0x123"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("0x123")), r)

//[<Test>]
//let TestNum15 () =
//    let code = reader "0x2A5f"
//    let r = num code
//    printf "%s" (r.ToString())
//    Assert.AreEqual(ValueSome (Span<Code>(ref [||]), new Num("0x2A5f")), r)
