using CbStyles.Cbon;
using CbStyles.Cbon.Parser;
using NUnit.Framework;

namespace TestParser
{
    public class TestWord
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void TestWord1()
        {
            var code = "asd";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewStr("asd"), r);
            });
        }

        [Test]
        public void TestWord2()
        {
            var code = "123";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("123"), r);
            });
        }

        [Test]
        public void TestWord3()
        {
            var code = "true";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewBool(true), r);
            });
        }

        [Test]
        public void TestWord4()
        {
            var code = "false";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewBool(false), r);
            });
        }

        [Test]
        public void TestWord5()
        {
            var code = "0x2a5f";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewHex("2a5f"), r);
            });
        }

        [Test]
        public void TestWord6()
        {
            var code = "123.456";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("123.456"), r);
            });
        }

        [Test]
        public void TestWord7()
        {
            var code = "-123";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("-123"), r);
            });
        }

        [Test]
        public void TestWord8()
        {
            var code = "123_456";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("123456"), r);
            });
        }

        [Test]
        public void TestWord9()
        {
            var code = "123e5";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("123e5"), r);
            });
        }

        [Test]
        public void TestWord10()
        {
            var code = "123e-5";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("123e-5"), r);
            });
        }
    }
}
