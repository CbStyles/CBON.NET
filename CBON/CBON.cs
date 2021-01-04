using System.Collections.Generic;

namespace CbStyles.Cbon
{
    public static class CBON
    {
        public static List<CbVal> Parse<T>(T source) where T : IEnumerable<char> => Parser.Parser.Parse<T>(source);
    }
}
