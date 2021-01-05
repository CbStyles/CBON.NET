using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code = CbStyles.Cbon.Unsafe.Sliced<char>;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CbStyles.Cbon.Parser
{

    public static class Parser
    {
        public unsafe static List<CbVal> Parse<T>(T source) where T: IEnumerable<char>
        {
            var code = Reader.Read(source).ToArray();
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
                var p = Reader.ReadPos(source).Skip((int)(e.at - 1)).First().AddOne();
                throw new ParserException(e.Message, p, e);
            }
            
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static (Code, bool) RootEndf(Code code) => (code, code.IsEmpty);

        public static unsafe R? RunInReader<T, R>(T source, Func<Code, R> f) where T : IEnumerable<char>
        {
            var code = Reader.Read(source).ToArray();
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
                throw new ParserException(e.Message, p, e);
            }
        }
        public static unsafe void RunInReader<T>(T source, Action<Code> f) where T : IEnumerable<char>
        {
            var code = Reader.Read(source).ToArray();
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
                throw new ParserException(e.Message, p, e);
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

        private static readonly Regex NumReg = new Regex(@"(\d+[\d_]*(\.(\d+[\d_]*)?)?([eE][-+]?\d+[\d_]*)?)|(\.\d+[\d_]*([eE][-+]?\d+[\d_]*)?)", RegexOptions.Compiled);
        private static readonly Regex HexReg = new Regex(@"0x[\da-fA-F]+[\da-fA-F_]*", RegexOptions.Compiled);

        //====================================================================================================

        public static (Code, CbVal)? Word(Code code)
        {
            switch (code.First)
            {
                case null: return null;
                case char c when NotWord(c): return null;
                default:
                    var e = FindIndex(code, 1, NotWord);
                    var s = code.SliceTo(e).ToString();
                    var v = s switch
                    {
                        "null" => CbVal.NewNull(),
                        "true" => CbVal.NewBool(true),
                        "false" => CbVal.NewBool(false),
                        _ when HexReg.IsMatch(s) => CbVal.NewHex(s[2..]),
                        _ when NumReg.IsMatch(s) => CbVal.NewNum(s),
                        _ => CbVal.NewStr(s),
                    };
                    return (code.Slice(e), v);
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
        private static nuint FindIndex(Code code, nuint index, Func<char, bool> f)
        {
            loop: switch (code[index])
            {
                case null: return code.Length;
                case char c: if (f(c)) return index; else index++; goto loop;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool IsHex(char c) => c is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z');
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static bool NotHex(char c) => !(c is (>= '0' and <= '9') or (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'));
    }
}
