module rec CbStyles.Cbon.Parser.Parser
open System.Text.RegularExpressions
open CbStyles.Cbon.Utils
open CbStyles.Cbon.Parser
open CbStyles.Parser
open System.Text

type ParserError(msg) =
    inherit System.Exception(msg)

let inline comStr (code: Code Span) = code |> Seq.toArray |> System.String

let inline is_hex c = c <? ('0', '9') || c <? ('a', 'f') || c <? ('A', 'F')
let inline not_hex c = not <| is_hex c
let inline (|Hex|NotNex|) c = if is_hex c then Hex else NotNex

let inline not_word c = c = '[' || c = '{' || c = '(' || c = ']' || c = '}' || c = ')' || c = ''' || c = '"' || c = ':' || c = '=' || c = ',' || c = ';' || System.Char.IsWhiteSpace c
let inline is_word c = not <| not_word c

let inline find_not (code: Code Span) (index: int32) f = code.[index..] |> Seq.tryFindIndex f =>= (fun v -> v + index) ?= code.Length

let num_reg = Regex (@"(\d+[\d_]*(\.(\d+[\d_]*)?)?([eE][-+]?\d+[\d_]*)?)|(\.\d+[\d_]*([eE][-+]?\d+[\d_]*)?)", RegexOptions.Compiled)
let hex_reg = Regex (@"0x[\da-fA-F]+[\da-fA-F_]*", RegexOptions.Compiled)

//====================================================================================================

let parser (code: Code Span) = arr_loop_body code (new MutList<CbAst>()) (fun code -> (code.IsEmpty, code)) |> sr

//====================================================================================================

let arr_loop (code: Code Span) = 
    match code.TryGet 0 with
    | ValueSome '[' -> ValueSome <| arr_loop_body code.[1..] (new MutList<CbAst>()) (fun code -> 
        if code.IsEmpty then (true, code) else
        match code.TryGet 0 with
        | ValueSome ']' -> (true, code.[1..])
        | _ -> (false, code) )
    | _ -> ValueNone
let arr_loop_body (code: Code Span) (item: CbAst MutList) (endf: Code Span -> struct(bool * Code Span)) = 
    match endf code with
    | (true, code) -> (code, item)
    | (false, code) -> 
    let r = str code =|=>=? CbAst.Str
        =>> (fun _ -> space code =|=>= none)
        =>> (fun _ -> comma code =|=>= none)
        =>> (fun _ -> union code =|=>=? CbAst.Union)
        =>> (fun _ -> arr_loop code =|=>=? CbAst.Arr)
        =>> (fun _ -> obj_loop code =|=>=? CbAst.Obj)
        =>> (fun _ -> word code =|=>= ValueSome)
    match r with
    | ValueNone -> raise <| ParserError ("Unexpected symbol \t at " + string(code.RawIndex 0))
    | ValueSome (code: Code Span, v) -> 
    match v with
    | ValueSome v -> item.Add(v)
    | _ -> ()
    arr_loop_body code item endf

//====================================================================================================

let obj_loop (code: Code Span) =
    match code.TryGet 0 with
    | ValueSome '{' -> ValueSome <| obj_loop_body code.[1..] (new MutMap<string, CbAst>()) (fun code ->
         if code.IsEmpty then (true, code) else
         match code.TryGet 0 with
         | ValueSome '}' -> (true, code.[1..])
         | _ -> (false, code) )
    | _ -> ValueNone
let obj_loop_body (code: Code Span) (item: MutMap<string, CbAst>) (endf: Code Span -> struct(bool * Code Span)) =
    match comma code =>> (fun _ -> space code) with
    | ValueSome (code, _) -> obj_loop_body code item endf
    | ValueNone ->
    match endf code with
    | (true, code) -> (code, item)
    | (false, code) ->
    let mutable code = space code =>== sl ?== code
    let struct(ncode, k) = str code =>> (fun _ -> key code) ?==! ParserError ("Expected key but not found \t at " + string(code.RawIndex 0))
    code <- ncode
    code <- space code =>== sl ?== code
    code <- split code =>== sl ?== code
    code <- space code =>== sl ?== code
    let r = str code =|=>= CbAst.Str
        =>> (fun _ -> union code =|=>= CbAst.Union)
        =>> (fun _ -> arr_loop code =|=>= CbAst.Arr)
        =>> (fun _ -> obj_loop code =|=>= CbAst.Obj)
        =>> (fun _ -> word code)
    match r with
    | ValueNone -> raise <| ParserError ("Unexpected symbol \t at " + string(code.RawIndex 0))
    | ValueSome (code: Code Span, v) -> 
    item.Add(k, v)
    obj_loop_body code item endf

//====================================================================================================

let str code =
    match code.TryGet 0 with
    | ValueSome '"' -> ValueSome <| str_body code 1 '"' (StringBuilder())
    | ValueSome ''' -> ValueSome <| str_body code 1 ''' (StringBuilder())
    | _ -> ValueNone
//////////////////////////////////////////////////////////////////////////////////////////////////////
//      ..."... ...'... ...\?...
// index is ^    or ^     or ^
let str_body code index quote sb =
    let e = find_not code index (fun c -> c = quote || c = '\\' )
    match code.TryGet e with
    | ValueSome '\\' ->
        sb.Append (comStr code.[index..e]) |> ignore
        let (e, c: char) = str_escape code (e + 1)
        sb.Append(c) |> ignore
        str_body code e quote sb
    | ValueSome c when c = quote -> 
        sb.Append (comStr code.[index..e]) |> ignore
        (code.[(e + 1)..], sb.ToString())
    | _ -> 
        let e = e - 1
        sb.Append (comStr code.[index..e]) |> ignore
        (code.[e..], sb.ToString())
//////////////////////////////////////////////////////////////////////////////////////////////////////
//      ...\...
// index is ^ 
let str_escape code index =
    match code.TryGet index with
    | ValueSome '\\' -> (index + 1, '\\')
    | ValueSome 'n' -> (index + 1, '\n')
    | ValueSome 'r' -> (index + 1, '\r')
    | ValueSome 'a' -> (index + 1, '\a')
    | ValueSome 'b' -> (index + 1, '\b')
    | ValueSome 'f' -> (index + 1, '\f')
    | ValueSome 't' -> (index + 1, '\t')
    | ValueSome 'v' -> (index + 1, '\v')
    | ValueSome 'u' -> str_escape_unicode code (index + 1)
    | ValueSome c -> (index + 1, c)
    | ValueNone -> raise <| ParserError ("string not closed \t at " + string(code.RawIndex index))
//////////////////////////////////////////////////////////////////////////////////////////////////////
//     ...\u...
// index is ^ 
let str_escape_unicode code index =
    match code.TryGet index with
    | ValueSome '{' -> str_escape_unicode_block code (index + 1)
    | ValueSome Hex -> str_escape_unicode_char code (index + 1)
    | _ -> (index, 'u')
//////////////////////////////////////////////////////////////////////////////////////////////////////
//    ...\u{...
// index is ^
let str_escape_unicode_block code index = 
    let e = find_not code index (fun c -> not_hex c )
    match code.TryGet e with
    | ValueSome '}' -> 
        let s = comStr code.[index..e]
        let mutable c: uint64 = 0UL
        if System.UInt64.TryParse(s, System.Globalization.NumberStyles.AllowHexSpecifier, null, &c) then
            let c = char c
            (e + 1, c)
        else raise <| ParserError ("Illegal Unicode escape \t at " + string(code.RawIndex index))
    | _ -> raise <| ParserError ("Unicode escape not close or illegal characters \t at " + string(code.RawIndex index))
//////////////////////////////////////////////////////////////////////////////////////////////////////
//    ...\uX...
// index is ^ 
let str_escape_unicode_char code index = 
    let e = find_not code index (fun c -> not_hex c ) |> min (index + 6)
    let s = comStr code.[(index - 1)..e]
    let mutable c: uint64 = 0UL
    if System.UInt64.TryParse(s, System.Globalization.NumberStyles.AllowHexSpecifier, null, &c) then
        let c = char c
        (e, c)
    else raise <| ParserError ("Illegal Unicode escape \t at " + string(code.RawIndex index))

//====================================================================================================

let space code =
    match code.TryGet 0 with
    | ValueSome c when System.Char.IsWhiteSpace c -> 
        let e = find_not code 1 (fun c -> System.Char.IsWhiteSpace c |> not)
        ValueSome (code.[e..], ())
    | _ -> ValueNone

//====================================================================================================

let word code =
    match code.TryGet 0 with
    | ValueNone -> ValueNone 
    | ValueSome c when not_word c -> ValueNone
    | _ -> 
        let e = find_not code 1 (fun c -> not_word c)
        let s = comStr code.[..e]
        let v = 
            match s with
            | "true" -> CbAst.Bool true
            | "false" -> CbAst.Bool false
            | "null" -> CbAst.Null
            | _ when hex_reg.IsMatch s -> CbAst.Hex (AHex <| s.Substring(2))
            | _ when num_reg.IsMatch s -> CbAst.Num (ANum s)
            | _ -> CbAst.Str s
        ValueSome (code.[e..], v)

//====================================================================================================

let comma code = 
    match code.TryGet 0 with
    | ValueSome ',' | ValueSome ';' -> ValueSome (code.[1..], ())
    | _ -> ValueNone

//====================================================================================================

let key code =
    match code.TryGet 0 with
    | ValueNone -> ValueNone 
    | ValueSome c when not_word c -> ValueNone
    | _ -> 
        let e = find_not code 1 (fun c -> not_word c)
        let s = comStr code.[..e]
        ValueSome (code.[e..], s)

//====================================================================================================

let split code = 
    match code.TryGet 0 with
    | ValueSome ':' | ValueSome '=' -> ValueSome (code.[1..], ())
    | _ -> ValueNone

//====================================================================================================

let union code =
    match code.TryGet 0 with
    | ValueSome '(' -> 
        let mutable code = code.[1..]
        code <- space code =>== sl ?== code
        let struct(ncode, k) = str code =>> (fun _ -> key code) ?==! ParserError ("Expected tag but not found \t at " + string(code.RawIndex 0))
        code <- ncode
        code <- space code =>== sl ?== code
        code <- 
            (match code.TryGet 0 with
            | ValueSome ')' -> ValueSome code.[1..]
            | _ -> ValueNone) ?==! ParserError ("Tag not close \t at " + string(code.RawIndex 0))
        code <- space code =>== sl ?== code
        let r = str code =|=>= CbAst.Str
            =>> (fun _ -> union code =|=>= CbAst.Union)
            =>> (fun _ -> arr_loop code =|=>= CbAst.Arr)
            =>> (fun _ -> obj_loop code =|=>= CbAst.Obj)
            =>> (fun _ -> word code)
        match r with
        | ValueNone -> raise <| ParserError ("Unexpected symbol \t at " + string(code.RawIndex 0))
        | ValueSome (code: Code Span, v) -> ValueSome (code, AUnion(k, v))
    | _ -> ValueNone
