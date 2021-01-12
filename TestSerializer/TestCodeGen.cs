using CbStyles.Cbon.Serializer;
using NUnit.Framework;
using System.Reflection;
using System.Reflection.Emit;

namespace TestSerializer
{
    public class Tests
    {
        [SetUp]
        public void Setup() { }

        [Cbon]
        class Foo
        {
            public string a = "123";
            public string b = "asd";
        }

        [Test]
        public void Test1()
        {
            var str = Serializer.Se(new Foo());
        }


        private static void SaveAssembly(Assembly a, string name = "debug")
        {
            var g = new Lokad.ILPack.AssemblyGenerator();
            g.GenerateAssembly(a, $"./debug_{name}.dll");
        }
    }
}