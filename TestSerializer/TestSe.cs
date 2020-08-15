using CbStyles.Cbon;
using NUnit.Framework;
using System;

namespace TestSerializer
{
    public class TestSe
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSe1()
        {
            var obj = new OTestA1(3);
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("{a 3}", r);
        }

        [Test]
        public void TestSe2()
        {
            var obj = new OTestA2(3);
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("{a 3}", r);
        }

        [Test]
        public void TestSe3()
        {
            var obj = new OTest3(3, true, "some 123");
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("{a 3 b true c 'some 123'}", r);
        }

        [Test]
        public void TestSe4()
        {
            var r = SeDe.DoSe("null");
            Console.WriteLine(r);
            Assert.AreEqual("'null'", r);
        }

        [Test]
        public void TestSe5()
        {
            var obj = new OTest3(3, true, "some");
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("{a 3 b true c some}", r);
        }

        [Test]
        public void TestSe6()
        {
            var obj = new OTest4(3, true, null);
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("{a 3 b true c null}", r);
        }

        [Test]
        public void TestSe7()
        {
            var obj = new[] { 1, 2, 3 };
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("[1 2 3]", r);
        }

        [Test]
        public void TestSe8()
        {
            var obj = new[] { 1, 2, 3 };
            var r = SeDe.DoSeArr(obj);
            Console.WriteLine(r);
            Assert.AreEqual("1 2 3", r);
        }

        [Test]
        public void TestSe9()
        {
            var obj = ETest1.C;
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
           Assert.AreEqual("2", r);
        }

        [Test]
        public void TestSe10()
        {
            var obj = ETest2.C;
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("C", r);
        }

        [Test]
        public void TestSe11()
        {
            var obj = ETest3.C;
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("C", r);
        }

        [Test]
        public void TestSe12()
        {
            var obj = ETest4.C;
            var r = SeDe.DoSe(obj);
            Console.WriteLine(r);
            Assert.AreEqual("three", r);
        }

        [Test]
        public void TestSe13()
        {
            var obj = new UTestA1(3);
            var r = SeDe.DoSe<UTest1>(obj);
            Console.WriteLine(r);
            Assert.AreEqual("(UTestA1){a 3}", r);
        }

        [Test]
        public void TestSe14()
        {
            var obj = new UTestB1("123");
            var r = SeDe.DoSe<UTest1>(obj);
            Console.WriteLine(r);
            Assert.AreEqual("(str){a '123'}", r);
        }

        [Test]
        public void TestSe15()
        {
            var obj = new UTestCUA1(3);
            var r = SeDe.DoSe<UTest1>(obj);
            Console.WriteLine(r);
            Assert.AreEqual("(n)(u){a 3}", r);
        }
    }
}
