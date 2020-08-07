using CbStyles.Cbon;
using NUnit.Framework;
using System;

namespace TestSerializer
{
    public class TestDe
    {
        [SetUp]
        public void Setup()
        {
        }

        [Serializable]
        class OTestDe1
        {
            public int a;
            private int b = 0;

            public OTestDe1() { }
            public OTestDe1(int a) => this.a = a;

            public override bool Equals(object obj) => obj is OTestDe1 de && a == de.a && b == de.b;
            public override string ToString() => $"{{ a: {a}, b: {b} }}";
            public override int GetHashCode() => throw new NotImplementedException();
        }
        [Test]
        public void TestDe1()
        {
            var code = "{ a 3 b 2 }";
            var r = SeDe.DoDe<OTestDe1>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new OTestDe1(3), r);
        }

        [Cbon]
        class OTestDe2
        {
            public int a;
            private int b = 0;

            public OTestDe2() { }
            public OTestDe2(int a) => this.a = a;

            public override bool Equals(object obj) => obj is OTestDe2 de && a == de.a && b == de.b;
            public override string ToString() => $"{{ a: {a}, b: {b} }}";
            public override int GetHashCode() => throw new NotImplementedException();
        }
        [Test]
        public void TestDe2()
        {
            var code = "{ a 3 b 2 }";
            var r = SeDe.DoDe<OTestDe2>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new OTestDe2(3), r);
        }
    }

    
}