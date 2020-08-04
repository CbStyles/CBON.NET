module TestSerializer.TestDeserialize

open NUnit.Framework
open CbStyle.Cbon.Serializer
open CbStyle.Cbon.Parser

[<SetUp>]
let Setup () =
    ()

[<Test>]
let TestDe1 () =
    let code = "1"
    let r = Deserialize<int> code
    printf "%s" (r.ToString())
    Assert.AreEqual(1, r)

[<Test>]
let TestDe2 () =
    let code = "true"
    let r = Deserialize<bool> code
    printf "%s" (r.ToString())
    Assert.AreEqual(true, r)

[<Test>]
let TestDe3 () =
    let code = "0x2a5f"
    let r = Deserialize<float> code
    printf "%s" (r.ToString())
    Assert.AreEqual(10847.0, r)

[<Test>]
let TestDe4 () =
    let code = "a"
    let r = Deserialize<char> code
    printf "%s" (r.ToString())
    Assert.AreEqual('a', r)

[<Test>]
let TestDe5 () =
    let code = "1"
    let r = Deserialize<bool> code
    printf "%s" (r.ToString())
    Assert.AreEqual(true, r)

[<Test>]
let TestDe6 () =
    let code = "asd"
    let r = Deserialize<string> code
    printf "%s" (r.ToString())
    Assert.AreEqual("asd", r)

[<Test>]
let TestDe7 () =
    let code = "123"
    let r = Deserialize<string> code
    printf "%s" (r.ToString())
    Assert.AreEqual("123", r)

type TestDe8Obj =
    val mutable a: int
    val b: int
    new () = { a = 0; b = 0 }
    new (a) = { a = a; b = 0 }
    override self.ToString() = "{ a: " + self.a.ToString() + ", b: " + self.b.ToString() + " }"
[<Test>]
let TestDe8 () =
    let code = "{ a 1; b 2 }"
    let r = Deserialize<TestDe8Obj> code
    printf "%s" (r.ToString())
    let o = new TestDe8Obj(1)
    Assert.AreEqual(o.a, r.a)
    Assert.AreEqual(o.b, r.b)