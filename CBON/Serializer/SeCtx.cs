using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public enum SeQuality : byte
    {
        Min, Common, Fast
    }

    public record SeOptions(uint TabSize = 2, SeStyle Style = SeStyle.None, SeQuality Quality = SeQuality.Common)
    {
        public SeOptions(SeStyle Style) : this() => this.Style = Style;
        public SeOptions(SeQuality Quality) : this() => this.Quality = Quality;

        public readonly static SeOptions Default = new SeOptions();
        public readonly static SeOptions Beautify = new SeOptions()
        {
            Style = SeStyle.Beautify,
        };
        public readonly static SeOptions Min = new SeOptions()
        {
            Quality = SeQuality.Min,
        };
        public readonly static SeOptions Fast = new SeOptions()
        {
            Quality = SeQuality.Fast,
        };
    }
    internal class SeCtx
    {
        public readonly SeOptions Options;
        public readonly StringBuilder sb = new StringBuilder();
        public SeStack Stack => new SeStack(this);

        public SeCtx() : this(SeOptions.Default) { }
        public SeCtx(SeOptions options)
        {
            Options = options;
        }
    }
    internal partial struct SeStack
    {
        internal readonly SeCtx ctx;
        public readonly nuint tab;
        public bool linefirst;

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

        public static readonly MethodInfo MI_DoTab = typeof(SeStack).GetMethod(nameof(SeStack.DoTab))!;
        public void DoTab()
        {
            if (!linefirst) return;
            for (nuint i = 0u; i < tab; i++)
            {
                Append(' ');
            }
        }

        public static readonly MethodInfo MI_DoSplit = typeof(SeStack).GetMethod(nameof(SeStack.DoSplit))!;
        public void DoSplit()
        {
            Append(' ');
        }

        public static readonly MethodInfo MI_DoObjStart = typeof(SeStack).GetMethod(nameof(SeStack.DoObjStart))!;
        public SeStack DoObjStart()
        {
            Append('{');
            if (Options.Style.HasFlag(SeStyle.WrapObjStart))
            {
                Append('\n');
                var nstack = Options.Style.HasFlag(SeStyle.TabObjItem) ? TabIn : this;
                nstack.DoTab();
                return nstack;
            }
            return Body;
        }

        public static readonly MethodInfo MI_DoObjKey = typeof(SeStack).GetMethod(nameof(SeStack.DoObjKey))!;
        public void DoObjKey(string key)
        {
            Append(key);
            Append(' ');
        }

        public static readonly MethodInfo MI_DoFinishObjItem = typeof(SeStack).GetMethod(nameof(SeStack.DoFinishObjItem))!;
        public void DoFinishObjItem()
        {
            if (Options.Style.HasFlag(SeStyle.WrapObjItem))
            {
                Append('\n');
                linefirst = true;
                DoTab();
            }
        }

        public static readonly MethodInfo MI_DoFinishObjItemBody = typeof(SeStack).GetMethod(nameof(SeStack.DoFinishObjItemBody))!;
        public void DoFinishObjItemBody()
        {
            if (Options.Style.HasFlag(SeStyle.WrapObjItem))
            {
                Append('\n');
                linefirst = true;
                DoTab();
            }
            else
            {
                Append(' ');
            }
        }

        public static readonly MethodInfo MI_DoObjEnd = typeof(SeStack).GetMethod(nameof(SeStack.DoObjEnd))!;
        public void DoObjEnd()
        {
            if (Options.Style.HasFlag(SeStyle.WrapObjEnd))
            {
                Append('\n');
                DoTab();
            }
            Append('}');
        }

        public static readonly MethodInfo MI_DoArrStart = typeof(SeStack).GetMethod(nameof(SeStack.DoArrStart))!;
        public SeStack DoArrStart()
        {
            Append('[');
            if (Options.Style.HasFlag(SeStyle.WrapArrStart))
            {
                Append('\n');
                var nstack = Options.Style.HasFlag(SeStyle.TabArrItem) ? TabIn : this;
                nstack.DoTab();
                return nstack;
            }
            return Body;
        }

        public static readonly MethodInfo MI_DoFinishArrItem = typeof(SeStack).GetMethod(nameof(SeStack.DoFinishArrItem))!;
        public void DoFinishArrItem()
        {
            if (Options.Style.HasFlag(SeStyle.WrapArrItem))
            {
                Append('\n');
                linefirst = true;
                DoTab();
            }
        }

        public static readonly MethodInfo MI_DoFinishArrItemBody = typeof(SeStack).GetMethod(nameof(SeStack.DoFinishArrItemBody))!;
        public void DoFinishArrItemBody()
        {
            if (Options.Style.HasFlag(SeStyle.WrapArrItem))
            {
                Append('\n');
                linefirst = true;
                DoTab();
            }
            else
            {
                Append(' ');
            }
        }

        public static readonly MethodInfo MI_DoArrEnd = typeof(SeStack).GetMethod(nameof(SeStack.DoArrEnd))!;
        public void DoArrEnd()
        {
            if (Options.Style.HasFlag(SeStyle.WrapArrEnd))
            {
                Append('\n');
                DoTab();
            }
            Append(']');
        }


        private static readonly MethodInfo MI_DoSe_ = typeof(SeStack).GetMethod(nameof(SeStack.DoSe))!;
        public static MethodInfo MI_DoSe<T>() => MI_DoSe(typeof(T));
        public static MethodInfo MI_DoSe(Type T) => MI_DoSe_.MakeGenericMethod(T);
        public void DoSe<T>(T sede, object obj) where T : ISeDe
        {
            if (obj == null) Append("null");
            else sede.Se(obj, this);
        }


        public void DoSeTS<T, V>(T sede, V obj) where T : ITypedSeDe<V> where V : struct
        {
            sede.SeT(obj, this);
        }

        public void DoSeTC<T, V>(T sede, V obj) where T : ITypedSeDe<V> where V : class
        {
            if (obj == null) Append("null");
            else sede.SeT(obj, this);
        }

        public static readonly MethodInfo MI_Append = typeof(SeStack).GetMethod(nameof(SeStack.Append), new Type[] { typeof(string) })!;
        public void Append(string s)
        {
            ctx.sb.Append(s);
        }

        public static readonly MethodInfo MI_Append_char = typeof(SeStack).GetMethod(nameof(SeStack.Append), new Type[] { typeof(char) })!;
        public void Append(char s)
        {
            ctx.sb.Append(s);
        }

        public SeStack Body => new SeStack(ctx, tab, false);
        public SeStack TabIn => new SeStack(ctx, tab + ctx.Options.TabSize, true);
    }
}
