using CbStyles.Cbon;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestSerializer
{
    public class TestDe
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestDe1()
        {
            var code = "{ a 3 b 2 }";
            var r = SeDe.DoDe<OTestA1>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new OTestA1(3), r);
        }

        [Test]
        public void TestDe2()
        {
            var code = "{ a 3 b 2 }";
            var r = SeDe.DoDe<OTestA2>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new OTestA2(3), r);
        }

        [Test]
        public void TestDe3()
        {
            var code = "[1 2 3]";
            var r = SeDe.DoDe<List<int>>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new[] { 1, 2, 3 }.ToList(), r);
        }

    }

    
}