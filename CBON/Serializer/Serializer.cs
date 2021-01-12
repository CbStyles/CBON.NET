using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CbStyles.Cbon.Serializer
{
    internal static class Serializer
    {
        public static string Se<T>(T val)
        {
            var sede = Codes.GetCode(typeof(T));
            var ctx = new SeCtx();
            sede.Se(val!, ctx.Stack);
            return ctx.sb.ToString();
        }
    }
}
