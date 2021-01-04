using CbStyles.Cbon;
using NUnit.Framework;
using System.Collections.Generic;

namespace TestAst
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestNull()
        {
            var a = CbVal.NewNull();
            Assert.IsTrue(a.IsNull);
        }

        [Test]
        public void TestBool()
        {
            var a = CbVal.NewBool(true);
            Assert.IsTrue(a.IsBool);
            Assert.IsTrue(a.Bool);
        }

        [Test]
        public void TestNum()
        {
            var a = CbVal.NewNum("123");
            Assert.IsTrue(a.IsNum);
            Assert.AreEqual(a.Num, "123");
        }

        [Test]
        public void TestHex()
        {
            var a = CbVal.NewHex("123");
            Assert.IsTrue(a.IsHex);
            Assert.AreEqual(a.Hex, "123");
        }

        [Test]
        public void TestStr()
        {
            var a = CbVal.NewStr("123");
            Assert.IsTrue(a.IsStr);
            Assert.AreEqual(a.Str, "123");
        }

        [Test]
        public void TestArr()
        {
            var a = CbVal.NewArr(new List<CbVal>{ CbVal.NewNull() });
            Assert.IsTrue(a.IsArr);
            Assert.AreEqual(a.Arr, new List<CbVal> { CbVal.NewNull() });
        }

        [Test]
        public void TestObj()
        {
            var a = CbVal.NewObj(new Dictionary<string, CbVal> { { "asd", CbVal.NewNull() } });
            Assert.IsTrue(a.IsObj);
            Assert.AreEqual(a.Obj, new Dictionary<string, CbVal> { { "asd", CbVal.NewNull() } });
        }

        [Test]
        public void TestUnion()
        {
            var a = CbVal.NewUnion(new CbUnion("asd", CbVal.NewNull()));
            Assert.IsTrue(a.IsUnion);
            Assert.AreEqual(a.Union, new CbUnion("asd", CbVal.NewNull()));
        }
    }
}