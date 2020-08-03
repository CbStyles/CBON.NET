module TestParser.TestObj

open NUnit.Framework
open CbStyle.Cbon.Parser
open CbStyle.Cbon.Parser.Parser
open CbStyle.Cbon.Parser.Reader
open CbStyle.Cbon
open System.Collections.Generic

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestObj1 () =
    let code = reader "{}"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)
    let e = new Dictionary<string, CbAst>()
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj2 () =
    let code = reader "{ a 1 }"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)

    let e = new Dictionary<string, CbAst>()
    e.Add("a", CbAst.Num (ANum "1"))
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj3 () =
    let code = reader "{ a 1, b true }"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)

    let e = new Dictionary<string, CbAst>()
    e.Add("a", CbAst.Num (ANum "1"))
    e.Add("b", CbAst.Bool true)
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj4 () =
    let code = reader "{ a false b null }"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)

    let e = new Dictionary<string, CbAst>()
    e.Add("a", CbAst.Bool false)
    e.Add("b", CbAst.Null)
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj5 () =
    let code = reader "{ \"a\": \"123\", 'b' = '321' ; ; c asd }"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)

    let e = new Dictionary<string, CbAst>()
    e.Add("a", CbAst.Str "123")
    e.Add("b", CbAst.Str "321")
    e.Add("c", CbAst.Str "asd")
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj6 () =
    let code = reader "{ a }"
    let f () = obj_loop code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)

[<Test>]
let TestObj7 () =
    let code = reader "{ : }"
    let f () = obj_loop code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)

[<Test>]
let TestObj8 () =
    let code = reader "{"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>(ref [||]), s)
    let e = new Dictionary<string, CbAst>()
    CollectionAssert.AreEqual(e, r)