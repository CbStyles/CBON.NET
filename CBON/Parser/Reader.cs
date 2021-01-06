using System.Collections.Generic;
using CbStyles.Cbon.SrcPos;

namespace CbStyles.Cbon.Parser
{
    internal static class Reader
    {

        public static IEnumerable<Pos> ReadPos<A>(A code) where A : IEnumerable<char> {
            var r = false;
            nuint line = 0;
            nuint column = 0;
            foreach (var c in code)
            {
                yield return new Pos(line, column);
                if (c == '\r')
                {
                    line++;
                    column = 0;
                    r = true;
                }
                else if (c == '\n')
                {
                    if (!r)
                    {
                        line++;
                        column = 0;
                    }
                    r = false;
                }
                else
                {
                    column++;
                    r = false;
                }
            }
        }

    }
}
