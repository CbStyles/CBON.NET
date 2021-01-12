using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CbStyles.Cbon.Serializer
{
    internal interface ISeDe
    {
        object? De(CbVal ast);
        void Se(object self, SeStack ctx);
    }

    internal interface ITypedSeDe<T> : ISeDe
    {
        T? DeT(CbVal ast);
        void SeT(T self, SeStack ctx);
    }

    internal abstract class TypedSeDe<T> : ITypedSeDe<T>
    {
        public object? De(CbVal ast) => DeT(ast);

        public void Se(object self, SeStack ctx) => SeT((T)self, ctx);

        public abstract T? DeT(CbVal ast);

        public abstract void SeT(T self, SeStack ctx);
    }

    internal static partial class Codes
    {
        private static string SeStrQuotMin(string str)
        {
            nuint d = 0u, s = 0u;
            foreach (var c in str)
            {
                if (c is '"') d++;
                if (c is '\'') s++;
            }
            if (d < s) return SeStrQuotDouble(str);
            else return SeStrQuot(str);
        }

        private static string SeStrQuotDouble(string s)
        {
            return $"\"{s.Replace("\"", "\\\"")}\"";
        }

        private static string SeStrQuot(string s)
        {
            return $"'{s.Replace("'", "\\'")}'";
        }

        private static string SeStrCommon(string s)
        {
            if (s.Length == 0) return "''";
            if (s.Length > 128) return SeStrQuotDouble(s);
            if (s.Length == 0 || Parser.Parser.RegNotWord.IsMatch(s))
                return SeStrQuot(s);
            return s;
        }

        static Codes()
        {
            codes.TryAdd(typeof(bool), new SeDeBool());
            codes.TryAdd(typeof(string), new SeDeStr());
            codes.TryAdd(typeof(DateTime), new SeDeDate());
            codes.TryAdd(typeof(Guid), new SeDeUUID());
            codes.TryAdd(typeof(char), new SeDeChar());
            codes.TryAdd(typeof(byte), new SeDeU8());
            codes.TryAdd(typeof(ushort), new SeDeU16());
        }

        private abstract class TypedSeDeBasic<T> : TypedSeDe<T>
        {
            public override void SeT(T self, SeStack ctx)
            {
                ctx.DoTab();
                ctx.Append(DoSeT(self, ctx));
            }

            protected abstract string DoSeT(T self, SeStack ctx);
        }

        private class SeDeBool : TypedSeDeBasic<bool>
        {
            public override bool DeT(CbVal ast) => ast.Bool;

            protected override string DoSeT(bool self, SeStack ctx) => self ? "true" : "false";
        }

        private class SeDeStr : TypedSeDeBasic<string>
        {
            public override string DeT(CbVal ast) => ast.Str;

            protected override string DoSeT(string self, SeStack ctx) => ctx.ctx.Options.Quality switch
            {
                SeQuality.Min => SeStrQuotMin(self),
                SeQuality.Common => SeStrCommon(self),
                SeQuality.Fast => SeStrQuotDouble(self),
                _ => throw new NotImplementedException("never"),
            };
        }

        private class SeDeDate : TypedSeDeBasic<DateTime>
        {
            public override DateTime DeT(CbVal ast) => ast.Date;

            protected override string DoSeT(DateTime self, SeStack ctx) => self.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK");
        }

        private class SeDeUUID : TypedSeDeBasic<Guid>
        {
            public override Guid DeT(CbVal ast) => ast.UUID;

            protected override string DoSeT(Guid self, SeStack ctx) => self.ToString();
        }

        private class SeDeU8 : TypedSeDeBasic<byte>
        {
            public override byte DeT(CbVal ast) => ast.U8();

            protected override string DoSeT(byte self, SeStack ctx) => self.ToString();
        }

        private class SeDeU16 : TypedSeDeBasic<ushort>
        {
            public override ushort DeT(CbVal ast) => ast.U16();

            protected override string DoSeT(ushort self, SeStack ctx) => self.ToString();
        }

        private class SeDeU32 : TypedSeDeBasic<uint>
        {
            public override uint DeT(CbVal ast) => ast.U32();

            protected override string DoSeT(uint self, SeStack ctx) => self.ToString();
        }

        private class SeDeU64 : TypedSeDeBasic<ulong>
        {
            public override ulong DeT(CbVal ast) => ast.U64();

            protected override string DoSeT(ulong self, SeStack ctx) => self.ToString();
        }

        private class SeDeUSize : TypedSeDeBasic<nuint>
        {
            public override nuint DeT(CbVal ast) => ast.USize();

            protected override string DoSeT(nuint self, SeStack ctx) => self.ToString();
        }

        private class SeDeI8 : TypedSeDeBasic<sbyte>
        {
            public override sbyte DeT(CbVal ast) => ast.I8();

            protected override string DoSeT(sbyte self, SeStack ctx) => self.ToString();
        }

        private class SeDeI16 : TypedSeDeBasic<short>
        {
            public override short DeT(CbVal ast) => ast.I16();

            protected override string DoSeT(short self, SeStack ctx) => self.ToString();
        }

        private class SeDeI32 : TypedSeDeBasic<int>
        {
            public override int DeT(CbVal ast) => ast.I32();

            protected override string DoSeT(int self, SeStack ctx) => self.ToString();
        }

        private class SeDeI64 : TypedSeDeBasic<long>
        {
            public override long DeT(CbVal ast) => ast.I64();

            protected override string DoSeT(long self, SeStack ctx) => self.ToString();
        }

        private class SeDeISize : TypedSeDeBasic<nint>
        {
            public override nint DeT(CbVal ast) => ast.ISize();

            protected override string DoSeT(nint self, SeStack ctx) => self.ToString();
        }

        private class SeDeF16 : TypedSeDeBasic<Half>
        {
            public override Half DeT(CbVal ast) => ast.F16();

            protected override string DoSeT(Half self, SeStack ctx) => self.ToString();
        }

        private class SeDeF32 : TypedSeDeBasic<float>
        {
            public override float DeT(CbVal ast) => ast.F32();

            protected override string DoSeT(float self, SeStack ctx) => self.ToString();
        }

        private class SeDeF64 : TypedSeDeBasic<double>
        {
            public override double DeT(CbVal ast) => ast.F64();

            protected override string DoSeT(double self, SeStack ctx) => self.ToString();
        }

        private class SeDeF128 : TypedSeDeBasic<decimal>
        {
            public override decimal DeT(CbVal ast) => ast.F128();

            protected override string DoSeT(decimal self, SeStack ctx) => self.ToString();
        }

        private class SeDeChar : TypedSeDeBasic<char>
        {
            public override char DeT(CbVal ast) => ast.Char()!.Value;

            protected override string DoSeT(char self, SeStack ctx) => ctx.ctx.Options.Quality switch
            {
                SeQuality.Min => SeStrQuotMin(self.ToString()),
                SeQuality.Common => SeStrCommon(self.ToString()),
                SeQuality.Fast => SeStrQuotDouble(self.ToString()),
                _ => throw new NotImplementedException("never"),
            };
        }

        private class SeDeBigInt : TypedSeDeBasic<BigInteger>
        {
            public override BigInteger DeT(CbVal ast) => ast.BigInt();

            protected override string DoSeT(BigInteger self, SeStack ctx) => self.ToString();
        }

        private static string SeKey(string key)
        {
            if (key.Length == 0) return "''";
            if (key.Length == 0 || Parser.Parser.RegNotWord_ForKey.IsMatch(key))
                return SeStrQuotMin(key);
            return key;
        }

        private class SeIter<T, V> where T : IEnumerable<V>
        {
            private readonly ISeDe sede;

            public SeIter(ISeDe sede) => this.sede = sede;

            public void SeT(T self, SeStack ctx)
            {
                ctx.DoTab();
                var body = ctx.DoArrStart();
                bool first = true;
                foreach (var item in self)
                {
                    if (first) first = false;
                    else body.DoFinishArrItemBody();
                    if (item == null) body.Append("null");
                    else body.DoSe(sede, item);
                }
                if (!first) body.DoFinishArrItem();
                ctx.DoArrEnd();
            }
        }
    }
}
