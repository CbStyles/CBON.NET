using NUnit.Framework;
using CbStyles.Cbon.Parser;

namespace TestParser
{
    public class TestStr
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void TestStr1()
        {
            var code = "'asd'";
            Parser.RunInReader(code, code => {
                var r = Parser.Str(code).Value.Item2;
                Assert.AreEqual("asd", r);
            });
        }

        [Test]
        public void TestStr2()
        {
            var code = "\"asd\"";
            Parser.RunInReader(code, code => {
                var r = Parser.Str(code).Value.Item2;
                Assert.AreEqual("asd", r);
            });
        }

        [Test]
        public void TestStr3()
        {
            var code = "'a\nd'";
            Parser.RunInReader(code, code => {
                var r = Parser.Str(code).Value.Item2;
                Assert.AreEqual("a\nd", r);
            });
        }

        [Test]
        public void TestStrEscape1()
        {
            var code = "'a\\nd'";
            Parser.RunInReader(code, code => {
                var r = Parser.Str(code).Value.Item2;
                Assert.AreEqual("a\nd", r);
            });
        }

        [Test]
        public void TestStrEscape2()
        {
            var code = "'a\\'d'";
            Parser.RunInReader(code, code => {
                var r = Parser.Str(code).Value.Item2;
                Assert.AreEqual("a'd", r);
            });
        }

        [Test]
        public void TestStrEscape3()
        {
            var code = "'\\u2A5F'";
            Parser.RunInReader(code, code => {
                var r = Parser.Str(code).Value.Item2;
                Assert.AreEqual("\u2a5f", r);
            });
        }

        [Test]
        public void TestStrEscape4()
        {
            var code = "'\\u{2A5F}'";
            Parser.RunInReader(code, code => {
                var r = Parser.Str(code).Value.Item2;
                Assert.AreEqual("\u2a5f", r);
            });
        }

        [Test]
        public void TestStrEscape5()
        {
            var code = "'\\u{u}'";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.Str(code);
            }));
            Assert.AreEqual("Illegal Unicode escape \t at 1:4", e.Message);
        }

        [Test]
        public void TestStrEscape6()
        {
            var code = "'\\u{}'";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.Str(code);
            }));
            Assert.AreEqual("Illegal Unicode escape \t at 1:4", e.Message);
        }

        [Test]
        public void TestStrEscape7()
        {
            var code = "'\\u{'";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.Str(code);
            }));
            Assert.AreEqual("Unicode escape not close or illegal characters \t at 1:4", e.Message);
        }
    }
}
