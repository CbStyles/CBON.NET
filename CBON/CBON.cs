using System.Collections.Generic;

namespace CbStyles.Cbon
{
    public static class CBON
    {
        public static List<CbVal> Parse(string source) => Parser.Parser.Parse(source);
        public static List<CbVal> Parse(string source, CBONParserOptions options) => Parser.Parser.Parse(source, options);

        public static List<CbVal> Parse(char[] source) => Parser.Parser.Parse(source);
        public static List<CbVal> Parse(char[] source, CBONParserOptions options) => Parser.Parser.Parse(source, options);
        
        public static List<CbVal> Parse<T>(T source) where T : IEnumerable<char> => Parser.Parser.Parse(source);
        public static List<CbVal> Parse<T>(T source, CBONParserOptions options) where T : IEnumerable<char> => Parser.Parser.Parse(source, options);

    }
}
