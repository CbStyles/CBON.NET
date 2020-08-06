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
    printfn "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Num <| ANum "123.456"), r)
    let struct(_, n) = r.Value
    match n with
    | Num n -> 
        let v = n.F64()
        printfn "%s" (v.ToString())
        Assert.AreEqual(123.456, v)
    | _ -> ()

[<Test>]
let TestWord7 () =
    let code = Reader.reader "-123"
    let r = word code
    printfn "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Num <| ANum "-123"), r)
    let struct(_, n) = r.Value
    match n with
    | Num n -> 
        let v = n.I32()
        printfn "%s" (v.ToString())
        Assert.AreEqual(-123, v)
    | _ -> ()

[<Test>]
let TestWord8 () =
    let code = Reader.reader "123_456"
    let r = word code
    printfn "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Num <| ANum "123456"), r)
    let struct(_, n) = r.Value
    match n with
    | Num n -> 
        let v = n.U32()
        printfn "%s" (v.ToString())
        Assert.AreEqual(123456u, v)
    | _ -> ()

[<Test>]
let TestWord9 () =
    let code = Reader.reader "123e5"
    let r = word code
    printfn "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Num <| ANum "123e5"), r)
    let struct(_, n) = r.Value
    match n with
    | Num n -> 
        let v = n.I32()
        printfn "%s" (v.ToString())
        Assert.AreEqual(1230_0000, v)
    | _ -> ()

[<Test>]
let TestWord10 () =
    let code = Reader.reader "123e-5"
    let r = word code
    printfn "%s" (r.ToString())
    Assert.AreEqual(ValueSome struct (Span<Code>([||]), CbAst.Num <| ANum "123e-5"), r)
    let struct(_, n) = r.Value
    match n with
    | Num n -> 
        let v = n.F32()
        printfn "%s" (v.ToString())
        Assert.AreEqual(0.00123f, v)
    | _ -> ()