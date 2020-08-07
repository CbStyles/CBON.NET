module TestParser.TestObj

open NUnit.Framework
open CbStyles.Cbon.Parser
open CbStyles.Cbon.Parser.Parser
open CbStyles.Parser
open System.Collections.Generic

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestObj1 () =
    let code = Reader.reader "{}"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>([||]), s)
    let e = new Dictionary<string, CbAst>()
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj2 () =
    let code = Reader.reader "{ a 1 }"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>([||]), s)

    let e = new Dictionary<string, CbAst>()
    e.Add("a", CbAst.Num (ANum "1"))
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj3 () =
    let code = Reader.reader "{ a 1, b true }"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>([||]), s)

    let e = new Dictionary<string, CbAst>()
    e.Add("a", CbAst.Num (ANum "1"))
    e.Add("b", CbAst.Bool true)
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj4 () =
    let code = Reader.reader "{ a false b null }"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>([||]), s)

    let e = new Dictionary<string, CbAst>()
    e.Add("a", CbAst.Bool false)
    e.Add("b", CbAst.Null)
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj5 () =
    let code = Reader.reader "{ \"a\": \"123\", 'b' = '321' ; ; c asd }"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>([||]), s)

    let e = new Dictionary<string, CbAst>()
    e.Add("a", CbAst.Str "123")
    e.Add("b", CbAst.Str "321")
    e.Add("c", CbAst.Str "asd")
    CollectionAssert.AreEqual(e, r)

[<Test>]
let TestObj6 () =
    let code = Reader.reader "{ a }"
    let f () = obj_loop code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)

[<Test>]
let TestObj7 () =
    let code = Reader.reader "{ : }"
    let f () = obj_loop code |> ignore
    let e = Assert.Throws<ParserError> (new TestDelegate(f))
    printf "%s" (e.Message)

[<Test>]
let TestObj8 () =
    let code = Reader.reader "{"
    let r = obj_loop code
    Assert.True(r.IsSome)
    let struct(s, r) = r.Value
    printf "(%s, %s)" (s.ToString()) (CbAst.ObjToString r)
    Assert.AreEqual(Span<Code>([||]), s)
    let e = new Dictionary<string, CbAst>()
    CollectionAssert.AreEqual(e, r)