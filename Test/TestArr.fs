module TestParser.TestArr

open NUnit.Framework
open CbStyle.Cbon.Parser
open CbStyle.Cbon.Reader
open CbStyle.Cbon
open System.Collections.Generic

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestArr1 () =
    let code = reader "[]"
    let r = arr_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ArrToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)
    let e = new List<CbAst>()
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestArr2 () =
    let code = reader "[ 123 ]"
    let r = arr_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ArrToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)
    let e = new List<CbAst>()
    e.Add(CbAst.Num (Num "123"))
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestArr3 () =
    let code = reader "[ asd true ]"
    let r = arr_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ArrToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)
    let e = new List<CbAst>()
    e.Add(CbAst.Str "asd")
    e.Add(CbAst.Bool true)
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestArr4 () =
    let code = reader "[ asd, true ]"
    let r = arr_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ArrToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)
    let e = new List<CbAst>()
    e.Add(CbAst.Str "asd")
    e.Add(CbAst.Bool true)
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestArr5 () =
    let code = reader "[ a = 1 ]"
    let f () = arr_loop code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)

[<Test>]
let TestArr6 () =
    let code = reader "["
    let r = arr_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ArrToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)
    let e = new List<CbAst>()
    CollectionAssert.AreEqual(e, r)