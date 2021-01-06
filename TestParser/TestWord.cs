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

        [Test]
        public void TestWord11()
        {
            var code = "2021-01-05T10:15:35.123Z";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewDate("2021-01-05T10:15:35.123Z"), r);
            });
        }

        [Test]
        public void TestWord12()
        {
            var code = "123.__456";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("123.456"), r);
            });
        }

        [Test]
        public void TestWord13()
        {
            var code = "123456789";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("123456789"), r);
            });
        }

        [Test]
        public void TestWord14()
        {
            var code = "1234567e-123";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewNum("1234567e-123"), r);
            });
        }

        [Test]
        public void TestWord15()
        {
            var code = "7efed01d-c654-414e-be95-bf6cf66a6927";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewUUID("7efed01d-c654-414e-be95-bf6cf66a6927"), r);
            });
        }

        [Test]
        public void TestWord16()
        {
            var code = "1234567e-1234-1234-1234-123456789abc";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewUUID("1234567e-1234-1234-1234-123456789abc"), r);
            });
        }

        [Test]
        public void TestWord17()
        {
            var code = "12345e-1234-1234-1234-123456789abc";
            Parser.RunInReader(code, code => {
                var r = Parser.Word(code).Value.Item2;
                Assert.AreEqual(CbVal.NewStr("12345e-1234-1234-1234-123456789abc"), r);
            });
        }
    }
}
