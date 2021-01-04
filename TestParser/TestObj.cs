using CbStyles.Cbon;
using CbStyles.Cbon.Parser;
using NUnit.Framework;
using System.Collections.Generic;


namespace TestParser
{
    public class TestObj
    {

        [SetUp]
        public void Setup() { }

        [Test]
        public void TestObj1()
        {
            var code = "{}";
            Parser.RunInReader(code, code => {
                var r = Parser.ObjLoop(code).Value.Item2;
                Assert.AreEqual(new Dictionary<string, CbVal>(), r);
            });
        }

        [Test]
        public void TestObj2()
        {
            var code = "{ a 1 }";
            Parser.RunInReader(code, code => {
                var r = Parser.ObjLoop(code).Value.Item2;
                Assert.AreEqual(new Dictionary<string, CbVal>() { { "a", CbVal.NewNum("1") } }, r);
            });
        }

        [Test]
        public void TestObj3()
        {
            var code = "{ a 1, b true }";
            Parser.RunInReader(code, code => {
                var r = Parser.ObjLoop(code).Value.Item2;
                Assert.AreEqual(new Dictionary<string, CbVal>() { { "a", CbVal.NewNum("1") }, { "b", CbVal.NewBool(true) } }, r);
            });
        }

        [Test]
        public void TestObj4()
        {
            var code = "{ a false b null }";
            Parser.RunInReader(code, code => {
                var r = Parser.ObjLoop(code).Value.Item2;
                Assert.AreEqual(new Dictionary<string, CbVal>() { { "a", CbVal.NewBool(false) }, { "b", CbVal.NewNull() } }, r);
            });
        }

        [Test]
        public void TestObj5()
        {
            var code = "{ \"a\": \"123\", 'b' = '321' ; ; c asd }";
            Parser.RunInReader(code, code => {
                var r = Parser.ObjLoop(code).Value.Item2;
                Assert.AreEqual(new Dictionary<string, CbVal>() { { "a", CbVal.NewStr("123") }, { "b", CbVal.NewStr("321") }, { "c", CbVal.NewStr("asd") } }, r);
            });
        }

        [Test]
        public void TestObj6()
        {
            var code = "{ a }";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.ObjLoop(code);
            }));
            Assert.AreEqual("Expected object value but not found \t at 1:5", e.Message);
        }

        [Test]
        public void TestObj7()
        {
            var code = "{ : }";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.ObjLoop(code);
            }));
            Assert.AreEqual("Expected object tag but not found \t at 1:2", e.Message);
        }

        [Test]
        public void TestObj8()
        {
            var code = "{";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.ObjLoop(code);
            }));
            Assert.AreEqual("Unexpected EOF \t at 1:1", e.Message);
        }
    }
}
