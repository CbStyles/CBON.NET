using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code = CbStyles.Cbon.Unsafe.Sliced<char>;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CbStyles.Cbon
{
    public record CBONParserOptions
    {
        public bool RestoreSrcPos = false;
    }
}

namespace CbStyles.Cbon.Parser
{
    public static class Parser
    {

        public unsafe static List<CbVal> Parse(string code)
        {
            if (code.Length == 0) return new List<CbVal>();
            try
            {
                fixed (char* ptr = code)
                {
                    var slice = new Code(ptr, (nuint)code.Length);
                    return ArrLoopBody(slice, RootEndf).Item2;
                }
            }
            catch (ParserError e)
            {
                throw new ParserException(e.Message, e.at, e);
            }
        }

        public unsafe static List<CbVal> Parse(string code, CBONParserOptions options)
        {
            var RestoreSrcPos = options.RestoreSrcPos;

            if (code.Length == 0) return new List<CbVal>();
            try
            {
                fixed (char* ptr = code)
                {
                    var slice = new Code(ptr, (nuint)code.Length);
                    return ArrLoopBody(slice, RootEndf).Item2;
                }
            }
            catch (ParserError e)
            {
                if (RestoreSrcPos)
                {
                    var p = Reader.ReadPos(code).Skip((int)(e.at - 1)).First().AddOne();
                    throw new ParserException(e.Message, e.at ,p, e);
                } 
                else
                {
                    throw new ParserException(e.Message, e.at, e);
                }
            }
        }

        public unsafe static List<CbVal> Parse<T>(T source) where T : IEnumerable<char>
        {
            var code = source.ToArray();
            return Parse(code);
        }

        public unsafe static List<CbVal> Parse<T>(T source, CBONParserOptions options) where T : IEnumerable<char>
        {
            var code = source.ToArray();
            return Parse(code, options);
        }

        public unsafe static List<CbVal> Parse(char[] code)
        {
            if (code.LongLength == 0) return new List<CbVal>();
            try
            {
                fixed (char* ptr = code)
                {
                    var slice = new Code(ptr, (nuint)code.LongLength);
                    return ArrLoopBody(slice, RootEndf).Item2;
                }
            }
            catch (ParserError e)
            {
                throw new ParserException(e.Message, e.at, e);
            }
        }

        public unsafe static List<CbVal> Parse(char[] code, CBONParserOptions options)
        {
            var RestoreSrcPos = options.RestoreSrcPos;

            if (code.LongLength == 0) return new List<CbVal>();
            try
            {
                fixed (char* ptr = code)
                {
                    var slice = new Code(ptr, (nuint)code.LongLength);
                    return ArrLoopBody(slice, RootEndf).Item2;
                }
            }
            catch (ParserError e)
            {
                if (RestoreSrcPos)
                {
                    var p = Reader.ReadPos(code).Skip((int)(e.at - 1)).First().AddOne();
                    throw new ParserException(e.Message, e.at, p, e);
                }
                else
                {
                    throw new ParserException(e.Message, e.at, e);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (Code, bool) RootEndf(Code code) => (code, code.IsEmpty);

        //====================================================================================================

        public static unsafe R? RunInReader<T, R>(T source, Func<Code, R> f) where T : IEnumerable<char>
        {
            var code = source.ToArray();
            if (code.LongLength == 0) return default;
            try
            {
                fixed (char* ptr = code)
                {
                    var slice = new Code(ptr, (nuint)code.LongLength);
                    return f(slice);
                }
            }
            catch (ParserError e)
            {
                var p = Reader.ReadPos(source).Skip((int)(e.at - 1)).First().AddOne();
                throw new ParserException(e.Message, e.at, p, e);
            }
        }
        public static unsafe void RunInReader<T>(T source, Action<Code> f) where T : IEnumerable<char>
        {
            var code = source.ToArray();
            if (code.LongLength == 0) return;
            try
            {
                fixed (char* ptr = code)
                {
                    var slice = new Code(ptr, (nuint)code.LongLength);
                    f(slice);
                    return;
                }
            }
            catch (ParserError e)
            {
                var p = Reader.ReadPos(source).Skip((int)(e.at - 1)).First().AddOne();
                throw new ParserException(e.Message, e.at, p, e);
            }
        }

        //====================================================================================================

        public static (Code, List<CbVal>)? ArrLoop(Code code) => code.First switch
        {
            '[' => ArrLoopBody(code.Tail, ArrLoopEndf),
            _ => null,
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (Code, bool) ArrLoopEndf(Code code) => code.First switch { ']' => (code.Tail, true), _ => (code, false), };

        private static (Code, List<CbVal>) ArrLoopBody(Code code, Func<Code, (Code, bool)> endf)
        {
            var items = new List<CbVal>();
            loop:
            switch (endf(code))
            {
                case (var c1, true): return (c1, items);
                case (var c1, false): code = c1; break;
            }
            if (code.IsEmpty) throw new ParserError("Unexpected EOF", code.Offset);
            else if (Str(code) is (var c2, var vs))
            {
                code = c2;
                items.Add(CbVal.NewStr(vs));
            }
            else if (Space(code) is Code c3) code = c3;
            else if (Comma(code) is Code c4) code = c4;
            else if (Union(code) is (var c5, var vu))
            {
                code = c5;
                items.Add(CbVal.NewUnion(vu));
            }
            else if (ArrLoop(code) is (var c6, var va))
            {
                code = c6;
                items.Add(CbVal.NewArr(va));
            }
            else if (ObjLoop(code) is (var c7, var vo))
            {
                code = c7;
                items.Add(CbVal.NewObj(vo));
            }
            else if (Word(code) is (var c, var vw))
            {
                code = c;
                items.Add(vw);
            }
            else throw new ParserError("Expected array value but not found", code.RawIndex(1u));
            goto loop;
        }

        //====================================================================================================

        public static (Code, Dictionary<string, CbVal>)? ObjLoop(Code code) => code.First switch
        {
            '{' => ObjLoopBody(code.Tail, ObjLoopEndf),
            _ => null,
        };
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (Code, bool) ObjLoopEndf(Code code) => code.First switch { '}' => (code.Tail, true), _ => (code, false), };
        private static (Code, Dictionary<string, CbVal>) ObjLoopBody(Code code, Func<Code, (Code, bool)> endf)
        {
            var items = new Dictionary<string, CbVal>();
            loop:
            if ((Comma(code) ?? Space(code)) is Code c0) { code = c0; goto loop; }
            switch (endf(code))
            {
                case (var c1, true): return (c1, items);
                case (var c1, false): code = c1; break;
            }
            code = Space(code) ?? code;
            if (code.IsEmpty) throw new ParserError("Unexpected EOF", code.Offset);
            string k;
            (code, k) = Str(code) switch { (Code, string) t => t, _ => Key(code) ?? throw new ParserError("Expected object tag but not found", code.Offset), };
            code = Space(code) ?? code;
            code = Split(code) ?? code;
            code = Space(code) ?? code;
            if (code.IsEmpty) throw new ParserError("Unexpected EOF", code.Offset);
            else if (Str(code) is (var c2, var vs))
            {
                code = c2;
                items.Add(k, CbVal.NewStr(vs));
            }
            else if (Union(code) is (var c3, var vu))
            {
                code = c3;
                items.Add(k, CbVal.NewUnion(vu));
            }
            else if (ArrLoop(code) is (var c4, var va))
            {
                code = c4;
                items.Add(k, CbVal.NewArr(va));
            }
            else if (ObjLoop(code) is (var c5, var vo))
            {
                code = c5;
                items.Add(k, CbVal.NewObj(vo));
            }
            else if (Word(code) is (var c, var vw))
            {
                code = c;
                items.Add(k, vw);
            }
            else throw new ParserError("Expected object value but not found", code.RawIndex(1u));
            goto loop;
        }

        //====================================================================================================

        public static (Code, string)? Str(Code code) => code.First switch
        {
            '"' => StrBody(code, '"', 1),
            '\'' => StrBody(code, '\'', 1),
            _ => null,
        };
        //      ..."... ...'... ...\?...
        // index is ^    or ^     or ^
        private static (Code, string) StrBody(Code code, char quote, nuint index)
        {
            var sb = new StringBuilder();
            loop:
            var e = FindIndex(code, index, new StrBodyFindInexF { quote = quote }.F);
            switch (code[e])
            {
                case '\\':
                    sb.Append(code.Slice(index, e).ToString());
                    string c;
                    (index, c) = StrEscape(code, e + 1);
                    sb.Append(c);
                    goto loop;
                case char v when v == quote:
                    sb.Append(code.Slice(index, e).ToString());
                    return (code.Slice(e + 1), sb.ToString());
                default:
                    --e;
                    sb.Append(code.Slice(index, e).ToString());
                    return (code.Slice(e), sb.ToString());
            }
        }
        private struct StrBodyFindInexF { public char quote; public bool F(char c) => c == quote || c == '\\'; }
        //      ...\...
        // index is ^ 
        private static (nuint, string) StrEscape(Code code, nuint index) => code[index] switch
        {
            '\\' => (index + 1, "\\"),
            'n' => (index + 1, "\n"),
            'r' => (index + 1, "\r"),
            'a' => (index + 1, "\a"),
            'b' => (index + 1, "\b"),
            'f' => (index + 1, "\f"),
            't' => (index + 1, "\t"),
            'v' => (index + 1, "\v"),
            'u' => StrEscapeUnicode(code, index + 1),
            char c => (index + 1, c.ToString()),
            null => throw new ParserError("string not closed", code.RawIndex(index)),
        };
        //     ...\u...
        // index is ^ 
        private static (nuint, string) StrEscapeUnicode(Code code, nuint index) => code[index] switch
        {
            '{' => StrEscapeUnicodeBlock(code, index + 1),
            char c when IsHex(c) => StrEscapeUnicodeChar(code, index + 1),
            _ => (index, "u"),
        };
        //    ...\u{...
        // index is ^
        private static (nuint, string) StrEscapeUnicodeBlock(Code code, nuint index)
        {
            var e = FindIndex(code, index, NotHex);
            switch (code[e])
            {
                case '}':
                    var s = code.Slice(index, e).ToString();
                    if (uint.TryParse(s, NumberStyles.AllowHexSpecifier, null, out var n)) 
                        return (e + 1, ((char)n).ToString());
                    else throw new ParserError("Illegal Unicode escape", code.RawIndex(index));
                default: throw new ParserError("Unicode escape not close or illegal characters", code.RawIndex(index));
            };
        }
        //    ...\uX...
        // index is ^ 
        private static (nuint, string) StrEscapeUnicodeChar(Code code, nuint index)
        {
            var e = FindIndex(code, index, NotHex).Min(index + 6);
            var s = code.Slice(index - 1, e).ToString();
            if (uint.TryParse(s, NumberStyles.AllowHexSpecifier, null, out var n))
                return (e, ((char)n).ToString());
            throw new ParserError("Illegal Unicode escape", code.RawIndex(index));
        }

        //====================================================================================================

        public static Code? Space(Code code) => code.First switch
        {
            char c when char.IsWhiteSpace(c) => code.Slice(FindIndex(code, 1, NotWhiteSpace)),
            _ => null,
        };
        private static bool NotWhiteSpace(char c) => !char.IsWhiteSpace(c);

        //====================================================================================================

        private static readonly Regex RegNotWord = new Regex(@"(^0[xX]_*[\da-fA-F]+[\da-fA-F_]*$)|(^[+-]?\d+[\d_]*(\.(_*\d+[\d_]*)?)?([eE][-+]?_*\d+[\d_]*)?$)|(^[+-]?\.\d+[\d_]*([eE][-+]?_*\d+[\d_]*)?$)|(^[\da-fA-F]{8}-[\da-fA-F]{4}-[\da-fA-F]{4}-[\da-fA-F]{4}-[\da-fA-F]{12}$)|(^.*[\[\]\{\}\(\)'"":=,;\s#]+.*$)", RegexOptions.Compiled);

        //====================================================================================================

        public static (Code, CbVal)? Word(Code code)
        {
            nuint index;
            switch (code.First)
            {
                case null: return null;
                case char c when NotWord(c): return null;
                case '0': goto num_hex_date;
                case '-' or '+': //goto num_date0;
                    index = 1;
                    goto num;
                case char c when c is (>= '1' and <= '9'): 
                    goto num_date_uuid1;
                case '.':
                    index = 1;
                    goto num_dot;
                case 'n':
                    if (code[1u] is 'u' && code[2u] is 'l' && code[3u] is 'l' && code[4u] switch { null => true, char c when NotWord(c) => true, _ => false })
                        return (code.Slice(4u), CbVal.NewNull());
                    break;
                case 't':
                    if (code[1u] is 'r' && code[2u] is 'u' && code[3u] is 'e' && code[4u] switch { null => true, char c when NotWord(c) => true, _ => false })
                        return (code.Slice(4u), CbVal.NewBool(true));
                    break;
                case 'f':
                    if (code[1u] is 'a' && code[2u] is 'l' && code[3u] is 's' && code[4u] is 'e' && code[5u] switch { null => true, char c when NotWord(c) => true, _ => false })
                        return (code.Slice(5u), CbVal.NewBool(false));
                    break;
                case char c when c is (>= 'a' and <= 'f') or (>= 'A' and <= 'F'):
                    goto uuid1;
                default: break;
            }
            index = 1;
            goto word;
        //         0 …
        // start at ^ 
        num_hex_date:
            switch (code[1])
            {
                case 'x' or 'X':
                    index = 2;
                    goto hex;
                case '.':
                    index = 2;
                    goto num_dot;
                case '_':
                    index = 2;
                    goto num;
                case char c when IsNum(c):
                    goto num_date2;
                case char c when NotWord(c):
                case null:
                    index = 1;
                    goto NumEnd;
                default:
                    index = 1;
                    goto word;
            }
        //        0x …
        // start at ^ 
        hex:
            {
                index = FindIndex(code, index, NotUnderscore);
                switch (code[index])
                {
                    case char c when IsHex(c): break;
                    case null:
                    case char c when NotWord(c):
                        goto WordEnd;
                    default: goto word;
                }
                index = FindIndex(code, index, NotHexWithUnderscore);
                switch (code[index])
                {
                    case null:
                    case char c when NotWord(c):
                        var s = code.Slice(2u, index).ToString();
                        return (code.Slice(index), CbVal.NewHex(s));
                    default: goto word;
                }
            }
        num_date_uuid1:
            index = 1;
            {
            loop:
                switch (code[index])
                {
                    case '-' when index == 4:
                        index++;
                        goto date;
                    case '-' when index == 8:
                        index++;
                        goto uuid;
                    case char c when IsNum(c) && index == 8:
                        index++;
                        goto num;
                    case char c when IsNum(c):
                        index++;
                        goto loop;
                    case 'e' or 'E' when index == 7:
                        index++;
                        goto uuid_num_e_8;
                    case 'e' or 'E':
                        index++;
                        goto uuid_num_e;
                    case char c when c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'):
                        index++;
                        goto uuidn;
                    case '.':
                        index++;
                        goto num_dot;
                    case '_':
                        index++;
                        goto num;
                    case null:
                    case char c when NotWord(c):
                        goto NumEnd;
                    default:
                        goto word;
                }
            }
        //         X …
        // start at ^ 
        uuid1:
            index = 1;
        //     … XXX - …
        // start at ^      index < 8
        uuidn:
            if (!MatchN(code, ref index, IsHex, 8u - index))
            {
                switch (code[index])
                {
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
            }
            switch (code[index])
            {
                case '-':
                    index++;
                    goto uuid;
                case null: case char c when NotWord(c): goto WordEnd;
                default: goto word;
            }
        // XXXXXXXX- …
        // start at ^       index = 9
        uuid:
            {
                if (!MatchN(code, ref index, IsHex, 4u))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case '-': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                goto uuid_part2;
            }
        // XXXXXXXX- …
        // start at ^       index = 14
        uuid_part2:
            {
                if (!MatchN(code, ref index, IsHex, 4u))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case '-': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsHex, 4u))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case '-': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsHex, 12u))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case null: case char c when NotWord(c):
                        var s = code.SliceTo(index).ToString();
                        return (code.Slice(index), CbVal.NewUUID(new Guid(s)));
                    default: goto word;
                }
            }
        //    0[0-9] …
        // start at ^ 
        num_date2:
            index = 2;
        //   … [0-9] …
        // start at ^ 
        // num_date:
            {
            loop:
                switch (code[index])
                {
                    case '-' when index == 4:
                        index++;
                        goto date;
                    case char c when IsNum(c) && index == 4:
                        index++;
                        goto num;
                    case char c when IsNum(c):
                        index++;
                        goto loop;
                    case '.':
                        index++;
                        goto num_dot;
                    case '_':
                        index++;
                        goto num;
                    case 'e' or 'E':
                        index++;
                        goto num_e;
                    case null:
                    case char c when NotWord(c):
                        goto NumEnd;
                    default:
                        goto word;
                }
            }
        //   …[0-9_] …
        // start at ^ 
        num:
            {
                index = FindIndex(code, index, NotNumBody);
                switch (code[index])
                {
                    case '.':
                        index++;
                        goto num_dot;
                    case 'e' or 'E':
                        index++;
                        goto num_e;
                    case null:
                    case char c when NotWord(c):
                        goto NumEnd;
                    default:
                        goto word;
                }
            }
        //       … . …
        // start at ^ 
        num_dot:
            {
                index = FindIndex(code, index, NotUnderscore);
                switch (code[index])
                {
                    case 'e' or 'E':
                        index++;
                        goto num_e;
                    case char c when IsNum(c): break;
                    case null:
                    case char c when NotWord(c):
                        goto WordEnd;
                    default: goto word;
                }
                index = FindIndex(code, index, NotNumBody);
                switch (code[index])
                {
                    case 'e' or 'E':
                        index++;
                        goto num_e;
                    case null:
                    case char c when NotWord(c):
                        goto NumEnd;
                    default: goto word;
                }
            }
            throw new NotImplementedException("never");
        //       … e …
        // start at ^ 
        num_e:
            {
                switch (code[index])
                {
                    case '+' or '-' or '_':
                        index++;
                        goto num_e_underscore;
                    case char c when IsNum(c):
                        index++;
                        goto num_e_numpart_no_underscore;
                    case null:
                    case char c when NotWord(c):
                        goto WordEnd;
                    default: goto word;
                }
            }
        num_e_numpart_no_underscore:
            index = FindIndex(code, index, NotNumBody);
            {
                switch (code[index])
                {
                    case '-' when index == 13:
                        index++;
                        goto uuid_part2;
                    case char c1 when c1 is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') && index < 13:
                        if (!MatchN(code, ref index, IsHex, 13 - index))
                        {
                            switch (code[index])
                            {
                                case null: case char c when NotWord(c): goto WordEnd;
                                default: goto word;
                            }
                        }
                        switch (code[index])
                        {
                            case '-': break;
                            case null: case char c when NotWord(c): goto WordEnd;
                            default: goto word;
                        }
                        index++;
                        goto uuid_part2;
                    case char c1 when c1 is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') && index == 13:
                        switch (code[index + 1])
                        {
                            case '-': break;
                            case null: case char c when NotWord(c): goto WordEnd;
                            default: goto word;
                        }
                        index++;
                        goto uuid_part2;
                    case null:
                    case char c when NotWord(c):
                        goto NumEnd;
                    default: goto word;
                }
            }
        num_e_underscore:
            {
                index = FindIndex(code, index, NotUnderscore);
                switch (code[index])
                {
                    case char c when IsNum(c): goto num_e_numpart;
                    case null:
                    case char c when NotWord(c):
                        goto WordEnd;
                    default: goto word;
                }
            }
        num_e_numpart:
            index = FindIndex(code, index, NotNumBody);
            {
                switch (code[index])
                {
                    case null:
                    case char c when NotWord(c):
                        goto NumEnd;
                    default: goto word;
                }
            }
        //       … e …
        // start at ^    index < 8
        uuid_num_e:
            {
                switch (code[index])
                {
                    case '+' or '_' or '-':
                        index++;
                        goto num_e_underscore;
                    case char c when c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'):
                        index++;
                        goto uuidn;
                    case char c when IsNum(c):
                        index++;
                        goto uuid_num_e_n;
                    case null:
                    case char c when NotWord(c):
                        goto WordEnd;
                    default: goto word;
                }
            }
        //      … en …
        // start at ^    index <= 8
        uuid_num_e_n:
            {
            loop:
                switch (code[index])
                {
                    case '_':
                        index++;
                        goto num_e_numpart;
                    case '-' when index == 8:
                        index++;
                        goto uuid;
                    case char c when c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'):
                        index++;
                        goto uuidn;
                    case char c when IsNum(c) && index == 8:
                        index++;
                        goto num_e_numpart_no_underscore;
                    case char c when IsNum(c):
                        index++;
                        goto loop;
                    case null:
                    case char c when NotWord(c):
                        goto NumEnd;
                    default: goto word;
                }
            }
        //       … e …
        // start at ^    index = 8
        uuid_num_e_8:
            {
                switch (code[index])
                {
                    case '-':
                        index++;
                        switch (code[index])
                        {
                            case '_':
                                goto num_e_underscore;
                            case char c when c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'):
                                goto uuid;
                            case char c when IsNum(c):
                                index++;
                                goto num_e_numpart_no_underscore;
                            case null: case char c when NotWord(c):
                                goto WordEnd;
                            default: goto word;
                        }
                    case '+' or '_':
                        index++;
                        goto num_e_underscore;
                    case char c when IsNum(c):
                        index++;
                        goto num_e_numpart_no_underscore;
                    case null:
                    case char c when NotWord(c):
                        goto WordEnd;
                    default: goto word;
                }
            }
        //  ±YYYYYY- …
        //     YYYY- …
        // start at ^ 
        date:
            #region date
            {
                if (!MatchN(code, ref index, IsNum, 2))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case '-': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsNum, 2))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case 'T': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsNum, 2))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case ':': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsNum, 2))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case ':': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsNum, 2))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case '.': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsNum, 3))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case 'Z':
                        index++;
                        switch (code[index])
                        {
                            case null: case char c when NotWord(c): goto DateEnd;
                            default: goto word;
                        }
                    case '+' or '-': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsNum, 2))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case ':': break;
                    case null: case char c when NotWord(c): goto WordEnd;
                    default: goto word;
                }
                index++;
                if (!MatchN(code, ref index, IsNum, 2))
                {
                    switch (code[index])
                    {
                        case null: case char c when NotWord(c): goto WordEnd;
                        default: goto word;
                    }
                }
                switch (code[index])
                {
                    case null: case char c when NotWord(c): goto DateEnd;
                    default: goto word;
                }
            DateEnd:
                {
                    var s = code.SliceTo(index).ToString();
                    return (code.Slice(index), CbVal.NewDate(s));
                }
            }
            #endregion
            throw new NotImplementedException("never");
        word:
            {
                index = FindIndex(code, index, NotWord);
                goto WordEnd;
            }
        NumEnd:
            {
                var s = code.SliceTo(index).ToString();
                return (code.Slice(index), CbVal.NewNum(s));
            }
        WordEnd:
            {
                var s = code.SliceTo(index).ToString();
                return (code.Slice(index), CbVal.NewStr(s));
            }
        }

        //====================================================================================================

        public static Code? Comma(Code code) => code.First switch
        {
            ',' or ';' => code.Tail,
            _ => null,
        };

        //====================================================================================================

        public static (Code, string)? Key(Code code)
        {
            switch (code.First)
            {
                case null: return null;
                case char c when NotWord(c): return null;
                default:
                    var e = FindIndex(code, 1, NotWord);
                    var s = code.SliceTo(e).ToString();
                    return (code.Slice(e), s);
            }
        }

        //====================================================================================================

        public static Code? Split(Code code) => code.First switch
        {
            ':' or '=' => code.Tail,
            _ => null,
        };

        //====================================================================================================

        public static (Code, CbUnion)? Union(Code code)
        {
            switch (code.First)
            {
                case '(':
                    code = code.Tail;
                    code = Space(code) ?? code;
                    if (code.IsEmpty) throw new ParserError("Unexpected EOF", code.Offset);
                    string k;
                    (code, k) = Str(code) switch { (Code, string) t => t, _ => Key(code) ?? throw new ParserError("Expected union tag but not found", code.Offset), };
                    code = Space(code) ?? code;
                    code = code.First switch { ')' => code.Tail, _ => throw new ParserError("Tag not close", code.Offset), };
                    code = Space(code) ?? code;
                    if (code.IsEmpty) throw new ParserError("Unexpected EOF", code.Offset);
                    else if (Str(code) is (var cs, var vs)) return (cs, new CbUnion(k, CbVal.NewStr(vs)));
                    else if (Union(code) is (var cu, var vu)) return (cu, new CbUnion(k, CbVal.NewUnion(vu)));
                    else if (ArrLoop(code) is (var ca, var va)) return (ca, new CbUnion(k, CbVal.NewArr(va)));
                    else if (ObjLoop(code) is (var co, var vo)) return (co, new CbUnion(k, CbVal.NewObj(vo)));
                    else if (Word(code) is (var cw, var vw)) return (cw, new CbUnion(k, vw));
                    else throw new ParserError("Expected union value but not found", code.RawIndex(1u));
                default: return null;
            }
        }

        //====================================================================================================

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool NotWord(char c) => c switch
        {
            '#' or '"' or '\'' or '(' or ')' or ',' or ':' or ';' or '=' or '[' or ']' or '{' or '}' => true,
            _ => char.IsWhiteSpace(c),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool NotUnderscore(char c) => c != '_';

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static nuint FindIndex(Code code, nuint index, Func<char, bool> f)
        {
            loop: switch (code[index])
            {
                case null: return code.Length;
                case char c: if (f(c)) return index; else index++; goto loop;
            }
        }

        /// after loop index is next not read char
        /// false index is now read char
        private static bool MatchN(Code code, ref nuint index, Func<char, bool> f, nuint n)
        {
            if (n == 0) return false;
        loop: switch (code[index])
            {
                case char c when f(c):
                    index++;
                    n--;
                    if (n == 0) return true;
                    goto loop;
                default: return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool IsHex(char c) => c is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z');

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool NotHex(char c) => !(c is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'));

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool NotHexWithUnderscore(char c) => !(c is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_');

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool NotNumBody(char c) => !(c is (>= '0' and <= '9') or '_');

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool IsNum(char c) => c is >= '0' and <= '9';
    }
}
