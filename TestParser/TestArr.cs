using CbStyles.Cbon;
using CbStyles.Cbon.Parser;
using NUnit.Framework;
using System.Collections.Generic;

namespace TestParser
{
    public class TestArr
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void TestArr1()
        {
            var code = "[]";
            Parser.RunInReader(code, code => {
                var r = Parser.ArrLoop(code).Value.Item2;
                Assert.AreEqual(new List<CbVal>(), r);
            });
        }

        [Test]
        public void TestArr2()
        {
            var code = "[ 123 ]";
            Parser.RunInReader(code, code => {
                var r = Parser.ArrLoop(code).Value.Item2;
                Assert.AreEqual(new List<CbVal>() { CbVal.NewNum("123") }, r);
            });
        }

        [Test]
        public void TestArr3()
        {
            var code = "[ asd true ]";
            Parser.RunInReader(code, code => {
                var r = Parser.ArrLoop(code).Value.Item2;
                Assert.AreEqual(new List<CbVal>() { CbVal.NewStr("asd"), CbVal.NewBool(true) }, r);
            });
        }

        [Test]
        public void TestArr4()
        {
            var code = "['asd', null]";
            Parser.RunInReader(code, code => {
                var r = Parser.ArrLoop(code).Value.Item2;
                Assert.AreEqual(new List<CbVal>() { CbVal.NewStr("asd"), CbVal.NewNull() }, r);
            });
        }

        [Test]
        public void TestArr5()
        {
            var code = "[ a = 1 ]";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.ArrLoop(code);
            }));
            Assert.AreEqual("Expected array value but not found \t at 1:5", e.Message);
        }

        [Test]
        public void TestArr6()
        {
            var code = "[";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.ArrLoop(code);
            }));
            Assert.AreEqual("Unexpected EOF \t at 1:1", e.Message);
        }
    }
}
