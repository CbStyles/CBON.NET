module CbStyle.Cbon.Parser
open CbStyle.Cbon.Utils
open CbStyle.Cbon
open System.Text

type ParserError(msg) =
    inherit System.Exception(msg)

let inline comStr (code: Code Span) = code |> Seq.toArray |> System.String

let inline is_hex c = c <? ('0', '9') || c <? ('a', 'f') || c <? ('A', 'F')
let inline not_hex c = not <| is_hex c
let inline (|Hex|NotNex|) c = if is_hex c then Hex else NotNex

let inline find_not (code: Code Span) index f = code.[index..] |> Seq.tryFindIndex f =>= (fun v -> v + index) ?= code.Length

let rec arr_loop (code: Code Span) = 
    failwith "todo"
and arr_loop_body (code: Code Span) (item: CbVal MutList) = 
    let r = str code =|=>=? CbVal.fStr
        =>> (fun () -> space code =|=>= none)
    match r with
    | ValueNone -> failwith "never"
    | ValueSome (code: Code Span, v) -> 
        match v with
        | ValueSome v -> item.Add(v)
        | _ -> ()
        if code.IsEmpty then struct(code, item)
        else arr_loop_body code item

//====================================================================================================

and str code =
    match code.Get 0 with
    | ValueSome '"' -> ValueSome <| str_body code 1 '"' (StringBuilder())
    | ValueSome ''' -> ValueSome <| str_body code 1 ''' (StringBuilder())
    | _ -> ValueNone
//////////////////////////////////////////////////////////////////////////////////////////////////////
//      ..."... ...'... ...\?...
// index is ^    or ^     or ^
and str_body code index quote sb =
    let e = find_not code index (fun c -> c = quote || c = '\\' )
    match code.Get e with
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
and str_escape code index =
    match code.Get index with
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
    | ValueNone -> raise <| ParserError ("string not closed " + string(code.RawIndex index))
//////////////////////////////////////////////////////////////////////////////////////////////////////
//     ...\u...
// index is ^ 
and str_escape_unicode code index =
    match code.Get index with
    | ValueSome '{' -> str_escape_unicode_block code (index + 1)
    | ValueSome Hex -> str_escape_unicode_char code (index + 1)
    | _ -> (index, 'u')
//////////////////////////////////////////////////////////////////////////////////////////////////////
//    ...\u{...
// index is ^
and str_escape_unicode_block code index = 
    let e = find_not code index (fun c -> not_hex c )
    match code.Get e with
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
and str_escape_unicode_char code index = 
    let e = find_not code index (fun c -> not_hex c ) |> min (index + 6)
    let s = comStr code.[(index - 1)..e]
    let mutable c: uint64 = 0UL
    if System.UInt64.TryParse(s, System.Globalization.NumberStyles.AllowHexSpecifier, null, &c) then
        let c = char c
        (e, c)
    else raise <| ParserError ("Illegal Unicode escape \t at " + string(code.RawIndex index))

//====================================================================================================

and space code =
    match code.Get 0 with
    | ValueSome c when System.Char.IsWhiteSpace c -> 
        let e = find_not code 1 (fun c -> System.Char.IsWhiteSpace c |> not)
        ValueSome (code.[e..], ())
    | _ -> ValueNone

//====================================================================================================





//let parser code = loop  code
