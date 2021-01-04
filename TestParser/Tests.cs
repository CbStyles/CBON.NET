using CbStyles.Cbon;
using CbStyles.Cbon.Parser;
using NUnit.Framework;

namespace TestParser
{
    public class Tests
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void Test1()
        {
            var code = "{a 1 b '2' c null d a e [] f {}}";
            var r = CBON.Parse(code);
            Assert.IsTrue(r[0].IsObj);
        }

        [Test]
        public void TestSpace1()
        {
            var code = "  ";
            Parser.RunInReader(code, code => {
                var r = Parser.Space(code);
                Assert.IsTrue(r != null);
            });
        }
    }
}