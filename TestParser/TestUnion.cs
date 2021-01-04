using CbStyles.Cbon;
using CbStyles.Cbon.Parser;
using NUnit.Framework;

namespace TestParser
{
    public class TestUnion
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void TestUnion1()
        {
            var code = "(asd) 123";
            Parser.RunInReader(code, code => {
                var r = Parser.Union(code).Value.Item2;
                Assert.AreEqual(new CbUnion("asd", CbVal.NewNum("123")), r);
            });
        }

        [Test]
        public void TestUnion2()
        {
            var code = "('asd')(123)true";
            Parser.RunInReader(code, code => {
                var r = Parser.Union(code).Value.Item2;
                Assert.AreEqual(new CbUnion("asd", CbVal.NewUnion("123", CbVal.NewBool(true))), r);
            });
        }

        [Test]
        public void TestUnion3()
        {
            var code = "()";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.Union(code);
            }));
            Assert.AreEqual("Expected union tag but not found \t at 1:1", e.Message);
        }

        [Test]
        public void TestUnion4()
        {
            var code = "(asd";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.Union(code);
            }));
            Assert.AreEqual("Tag not close \t at 1:4", e.Message);
        }

        [Test]
        public void TestUnion5()
        {
            var code = "(asd)";
            var e = Assert.Throws<ParserException>(() => Parser.RunInReader(code, code => {
                Parser.Union(code);
            }));
            Assert.AreEqual("Unexpected EOF \t at 1:5", e.Message);
        }
    }
}
