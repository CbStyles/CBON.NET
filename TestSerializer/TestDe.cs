using CbStyles.Cbon;
using CbStyles.Cbon.Parser;
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
            var r = Cbon.DoDe<OTestA1>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new OTestA1(3), r);
        }

        [Test]
        public void TestDe2()
        {
            var code = "{ a 3 b 2 }";
            var r = Cbon.DoDe<OTestA2>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new OTestA2(3), r);
        }

        [Test]
        public void TestDe3()
        {
            var code = "[1 2 3]";
            var r = Cbon.DoDe<List<int>>(code);
            Console.WriteLine(CbAst.ArrToString<List<int>, int>(r));
            Assert.AreEqual(new[] { 1, 2, 3 }.ToList(), r);
        }


        [Test]
        public void TestDe4()
        {
            var code = "[1 2 3]";
            var r = Cbon.DoDe<int[]>(code);
            Console.WriteLine(CbAst.ArrToString<int[], int>(r));
            Assert.AreEqual(new[] { 1, 2, 3 }, r);
        }

        [Test]
        public void TestDe5()
        {
            var code = "asd";
            var r = Cbon.DoDe<string>(code);
            Console.WriteLine(r);
            Assert.AreEqual("asd", r);
        }

        [Test]
        public void TestDe6()
        {
            var code = "true";
            var r = Cbon.DoDe<bool>(code);
            Console.WriteLine(r);
            Assert.AreEqual(true, r);
        }

        [Test]
        public void TestDe7()
        {
            var code = "null";
            var r = Cbon.DoDe<bool?>(code);
            Console.WriteLine(r);
            Assert.AreEqual(null, r);
        }

        [Test]
        public void TestDe8()
        {
            var code = "true";
            var r = Cbon.DoDe<bool?>(code);
            Console.WriteLine(r);
            Assert.AreEqual(true, r);
        }

        [Test]
        public void TestDe9()
        {
            var code = "{a 3 b true c 'some 123' d 1 e 2}";
            var r = Cbon.DoDe<OTest3>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new OTest3(3, true, "some 123"), r);
        }

        [Test]
        public void TestDe10()
        {
            var code = "{a 3 b true c null d 1 e 2}";
            var r = Cbon.DoDe<OTest4>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new OTest4(3, true, null), r);
        }

        [Test]
        public void TestDe11()
        {
            var code = "2";
            var r = Cbon.DoDe<ETest1>(code);
            Console.WriteLine(r);
            Assert.AreEqual(ETest1.C, r);
        }

        [Test]
        public void TestDe12()
        {
            var code = "C";
            var r = Cbon.DoDe<ETest2>(code);
            Console.WriteLine(r);
            Assert.AreEqual(ETest2.C, r);
        }

        [Test]
        public void TestDe13()
        {
            var code = "C";
            var r = Cbon.DoDe<ETest3>(code);
            Console.WriteLine(r);
            Assert.AreEqual(ETest3.C, r);
        }

        [Test]
        public void TestDe14()
        {
            var code = "three";
            var r = Cbon.DoDe<ETest4>(code);
            Console.WriteLine(r);
            Assert.AreEqual(ETest4.C, r);
        }

        [Test]
        public void TestDe15()
        {
            var code = "(UTestA1){a 3}";
            var r = Cbon.DoDe<UTest1>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new UTestA1(3), r);
        }

        [Test]
        public void TestDe16()
        {
            var code = "(str){a '123'}";
            var r = Cbon.DoDe<UTest1>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new UTestB1("123"), r);
        }

        [Test]
        public void TestDe17()
        {
            var code = "(n)(u){a 3}";
            var r = Cbon.DoDe<UTest1>(code);
            Console.WriteLine(r);
            Assert.AreEqual(new UTestCUA1(3), r);
        }

        [Test]
        public void TestDe18()
        {
            var code = "1 2 3";
            var r = Cbon.DoDeArr<int>(code);
            Console.WriteLine(CbAst.ArrToString<List<int>, int>(r));
            Assert.AreEqual(new[] { 1, 2, 3 }.ToList(), r);
        }

        [Test]
        public void TestDe19()
        {
            var code = "{1 3 2 2 3 1}";
            var r = Cbon.DoDe<Dictionary<int, int>>(code);
            Console.WriteLine(CbAst.ObjToString<Dictionary<int, int>, int, int>(r));
            var obj = new Dictionary<int, int> { { 1, 3 }, { 2, 2 }, { 3, 1 } };
            Assert.AreEqual(obj, r);
        }
    }

}