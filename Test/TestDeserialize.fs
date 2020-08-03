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
    let r = DeserializeArray<int> code
    printf "%s" (CbAst.ArrToString r)
    Assert.AreEqual(1, r.[0])

