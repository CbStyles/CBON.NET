using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CbStyles.Cbon.Serializer
{
    [Flags]
    public enum SeStyle : byte
    {
        None = 0,
        WrapObjStart = 0b__001,
        WrapObjEnd = 0b__010,
        WrapObjItem = 0b__100,
        WrapArrStart = 0b__001__000,
        WrapArrEnd = 0b__010__000,
        WrapArrItem = 0b__100__000,
        TabObjItem = 0b__01__000_000,
        TabArrItem = 0b__10__000_000,
        WrapObj = WrapObjStart | WrapObjEnd | WrapObjItem | TabObjItem,
        WrapArr = WrapArrStart | WrapArrEnd | WrapArrItem | TabArrItem,
        Beautify = WrapObj | WrapArr,
    }

    public record SeOptions
    {
        public uint TabSize = 2;
        public SeStyle Style = SeStyle.None;

        internal static SeOptions Default = new SeOptions();
        internal static SeOptions Beautify = new SeOptions()
        {
            Style = SeStyle.Beautify,
        };
    }
    internal class SeCtx
    {
        public readonly SeOptions Options;
        public StringBuilder sb = new StringBuilder();
        public SeStack Stack => new SeStack(this);

        public SeCtx() : this(SeOptions.Default) { }
        public SeCtx(SeOptions options)
        {
            Options = options;
        }
    }
    public readonly struct SeStack
    {
        internal readonly SeCtx ctx;
        public readonly nuint tab;
        public readonly bool linefirst;

        internal SeStack(SeCtx ctx)
        {
            this.ctx = ctx;
            tab = 0u;
            linefirst = true;
        }

        private SeStack(SeCtx ctx, nuint tab, bool lf)
        {
            this.ctx = ctx;
            this.tab = tab;
            linefirst = lf;
        }

        public SeOptions Options => ctx.Options;

        public void DoTab()
        {
            if (!linefirst) return;
            for (nuint i = 0u; i < tab; i++)
            {
                ctx.sb.Append(' ');
            }
        }

        public SeStack DoObjStart()
        {
            Append("{");
            if (Options.Style.HasFlag(SeStyle.WrapObjStart))
            {
                ctx.sb.Append('\n');
                var nstack = Options.Style.HasFlag(SeStyle.TabObjItem) ? TabIn : this;
                nstack.DoTab();
                return nstack;
            }
            return this;
        }

        public void DoFinishObjItem()
        {
            if (Options.Style.HasFlag(SeStyle.WrapObjItem))
            {
                ctx.sb.Append('\n');
                DoTab();
            }
        }

        public void DoObjEnd()
        {
            if (Options.Style.HasFlag(SeStyle.WrapObjEnd))
            {
                ctx.sb.Append('\n');
                DoTab();
            }
            Append("}");
        }

        public void Append(string s)
        {
            ctx.sb.Append(s);
        }
        public void Append(char s)
        {
            ctx.sb.Append(s);
        }

        public SeStack Body => new SeStack(ctx, tab, false);
        public SeStack TabIn => new SeStack(ctx, tab + ctx.Options.TabSize, true);
    }

    class Serializer
    {
    }
}
