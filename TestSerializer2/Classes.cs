using CbStyles.Cbon;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestSerializer
{
    abstract class AOTestA
    {
        public int a;
        private int b = 0;

        public AOTestA() { }
        public AOTestA(int a) => this.a = a;

        public override bool Equals(object obj) => obj is AOTestA de && a == de.a && b == de.b;
        public override string ToString() => $"{{ a: {a}, b: {b} }}";
    }

    [Serializable]
    class OTestA1 : AOTestA
    {
        public OTestA1() { }

        public OTestA1(int a) : base(a) { }
    }

    [Cbon]
    class OTestA2 : AOTestA
    {
        public OTestA2() { }

        public OTestA2(int a) : base(a) { }
    }

    [Cbon]
    class OTest3
    {
        public int a;
        public bool b;
        public string c;
        [Cbon(Ignore = true)]
        public int d = 0;
        [CbonIgnore]
        public int e = 0;

        public OTest3() { }

        public OTest3(int a, bool b, string c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override bool Equals(object obj) => obj is OTest3 test && a == test.a && b == test.b && c == test.c && d == test.d && e == test.e;

        public override string ToString() => $"{{ a: {a}, b: {b}, c: {c}, d: {d}, e: {e} }}";
    }

    [Cbon(CbonMember.Opt)]
    class OTest4
    {
        [Cbon]
        public int a;
        [Cbon]
        public bool b;
        [Cbon]
        public string? c;
        public int d = 0;
        public int e = 0;

        public OTest4() { }

        public OTest4(int a, bool b, string? c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override bool Equals(object obj) => obj is OTest4 test && a == test.a && b == test.b && c == test.c && d == test.d && e == test.e;

        public override string ToString() => $"{{ a: {a}, b: {b}, c: {c}, d: {d}, e: {e} }}";
    }

    [Serializable]
    enum ETest1
    {
        A, B, C
    }

    [CbonUnion]
    enum ETest2
    {
        A, B, C
    }

    [Cbon(Union = true)]
    enum ETest3
    {
        A, B, C
    }

    [CbonUnion]
    enum ETest4
    {
        A, 
        B, 
        [Cbon("three")]
        C
    }

    [CbonUnion(typeof(UTestA1), typeof(UTestB1), typeof(UTestCU1))]
    interface UTest1 { }

    [CbonUnionItem]
    class UTestA1 : UTest1 
    {
        public int a;

        public UTestA1() { }
        public UTestA1(int a) => this.a = a;

        public override bool Equals(object obj) => obj is UTestA1 a && this.a == a.a;
        public override string ToString() => $"{{ a: {a} }}";
    }

    [CbonUnionItem("str")]
    class UTestB1 : UTest1 
    {
        public string a;

        public UTestB1() { }
        public UTestB1(string a) => this.a = a;

        public override bool Equals(object obj) => obj is UTestB1 a && this.a == a.a;
        public override string ToString() => $"{{ a: {a} }}";
    }

    [CbonUnionItem("n")]
    [CbonUnion(typeof(UTestCUA1))]
    abstract class UTestCU1 : UTest1 { }

    [CbonUnionItem("u")]
    class UTestCUA1 : UTestCU1
    {
        public int a;

        public UTestCUA1() { }
        public UTestCUA1(int a) => this.a = a;

        public override bool Equals(object obj) => obj is UTestCUA1 a && this.a == a.a;
        public override string ToString() => $"{{ a: {a} }}";
    }

}
