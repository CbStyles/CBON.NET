using System;
using System.Collections.Generic;
using System.Linq;
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
        static Codes()
        {
            codes.TryAdd(typeof(bool), new SeDeBool());
            codes.TryAdd(typeof(string), new SeDeStr());
            codes.TryAdd(typeof(DateTime), new SeDeDate());
            codes.TryAdd(typeof(Guid), new SeDeUUID());
        }

        private static string SeStrQuot(string s)
        {
            return $"'{s.Replace("'", "\\'")}'";
        }

        private class SeDeBool : TypedSeDe<bool>
        {
            public override bool DeT(CbVal ast)
            {
                return ast.Bool;
            }

            public override void SeT(bool self, SeStack ctx)
            {
                ctx.DoTab();
                ctx.Append(self ? "true" : "false");
            }
        }

        private class SeDeStr : TypedSeDe<string>
        {
            public override string DeT(CbVal ast)
            {
                return ast.Str;
            }

            public override void SeT(string self, SeStack ctx)
            {
                ctx.DoTab();
                ctx.Append(SeStrQuot(self));
            }
        }

        private class SeDeDate : TypedSeDe<DateTime>
        {
            public override DateTime DeT(CbVal ast)
            {
                return ast.Date;
            }

            public override void SeT(DateTime self, SeStack ctx)
            {
                ctx.DoTab();
                ctx.Append(self.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"));
            }
        }

        private class SeDeUUID : TypedSeDe<Guid>
        {
            public override Guid DeT(CbVal ast)
            {
                return ast.UUID;
            }

            public override void SeT(Guid self, SeStack ctx)
            {
                ctx.DoTab();
                ctx.Append(self.ToString());
            }
        }
    }
}
