using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestAst")]
[assembly: InternalsVisibleTo("TestParser")]
[assembly: InternalsVisibleTo("TestSerializer")]
[assembly: InternalsVisibleTo("TestSlice")]

namespace CbStyles.Cbon
{
    public class Cbon
    {
        public static List<CbVal> Parse(string source) => Parser.Parser.Parse(source);
        public static List<CbVal> Parse(string source, CBONParserOptions options) => Parser.Parser.Parse(source, options);

        public static List<CbVal> Parse(char[] source) => Parser.Parser.Parse(source);
        public static List<CbVal> Parse(char[] source, CBONParserOptions options) => Parser.Parser.Parse(source, options);
        
        public static List<CbVal> Parse<T>(T source) where T : IEnumerable<char> => Parser.Parser.Parse(source);
        public static List<CbVal> Parse<T>(T source, CBONParserOptions options) where T : IEnumerable<char> => Parser.Parser.Parse(source, options);

    }
}
