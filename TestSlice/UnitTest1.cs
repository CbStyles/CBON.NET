using CbStyles.Cbon.Unsafe;
using NUnit.Framework;

namespace TestSlice
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public unsafe void Test1()
        {
            var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            fixed (int* ptr = arr)
            {
                var s = new Sliced<int>(ptr, (nuint)arr.LongLength);
                Assert.AreEqual(s.First, 1);
                Assert.AreEqual(s[1], 2);
                var t = s.Tail;
                Assert.AreEqual(t.First, 2);
                Assert.AreEqual(t[3], 5);
            }
        }

        [Test]
        public unsafe void TestStackAlloc()
        {
            var ptr = stackalloc[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var s = new Sliced<int>(ptr, 10u);
            Assert.AreEqual(s.First, 1);
            Assert.AreEqual(s[1], 2);
            var t = s.Tail;
            Assert.AreEqual(t.First, 2);
            Assert.AreEqual(t[3], 5);
        }

        [Test]
        public unsafe void TestSlice()
        {
            var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            fixed (int* ptr = arr)
            {
                var s = new Sliced<int>(ptr, (nuint)arr.LongLength);
                var a = s.Slice(2);
                Assert.AreEqual(a.First, 3);
                var b = s.SliceTo(3);
                Assert.AreEqual(b.Last, 3);
                var c = s.Slice(1, 3);
                Assert.AreEqual(c.First, 2);
                Assert.AreEqual(c.Last, 3);
            }
        }

        [Test]
        public unsafe void TestToArray()
        {
            var arr = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            fixed (int* ptr = arr)
            {
                var s = new Sliced<int>(ptr, (nuint)arr.LongLength);
                var a = s.ToArray();
                Assert.AreEqual(a, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            }
        }

        [Test]
        public unsafe void TestToString()
        {
            var str = "1234567890";
            fixed (char* ptr = str)
            {
                var s = new Sliced<char>(ptr, (nuint)str.Length);
                var a = s.ToString();
                Assert.AreEqual(a, "1234567890");
            }
        }


    }
}