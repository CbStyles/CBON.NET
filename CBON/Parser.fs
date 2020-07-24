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
    let r = num code =|=>= CbVal.fNum
        =>> (fun () -> str code =|=>= CbVal.fStr) 
    match r with
    | ValueNone -> failwith "never"
    | ValueSome (code: Code Span, v) -> 
        item.Add(v)
        if code.IsEmpty then (code, item)
        else arr_loop_body code item

and num code = 
    match code.Get 0 with 
    | ValueSome '0' -> check_num_zero code
    | ValueSome '.' -> num_body_may_float code 1
    | ValueSome c when c <? ('1', '9') -> num_body code 1
    | _ -> ValueNone
and check_num_zero code =
    match code.Get 1 with
    | ValueSome 'x' -> num_body_hex code 2
    | ValueSome '.' -> num_body_float code 2
    | ValueSome c when c <? ('0', '9') || c = '_' -> num_body code 2
    | _ -> ValueSome (code.[1..], new Num(comStr code.[..1]))
and num_body code index =
    let e = code.[index..] |> Seq.tryFindIndex (fun c -> ! (c <? ('0', '9') || c = '_') ) =>= (fun v -> v + index) ?= code.Length
    match code.Get e with
    | ValueSome '.' -> num_body_float code (e + 1)
    | ValueSome c when c = 'e' || c = 'E' -> num_body_radix code (e + 1)
    | _ -> ValueSome (code.[e..], new Num(comStr code.[..e]))
and num_body_may_float code index =
    let e = code.[index..] |> Seq.tryFindIndex (fun c -> c <> '_' ) =>= (fun v -> v + index) ?= code.Length
    match code.Get e with
    | ValueSome c when c <? ('0', '9') -> num_body_float code (e + 1)
    | _ -> ValueNone
and num_body_float code index =
    let e = code.[index..] |> Seq.tryFindIndex (fun c -> ! (c <? ('0', '9') || c = '_') ) =>= (fun v -> v + index) ?= code.Length
    match code.Get e with
    | ValueSome c when c = 'e' || c = 'E' -> num_body_radix code (e + 1)
    | _ -> ValueSome (code.[e..], new Num(comStr code.[..e]))
and num_body_radix code index = 
    if code.[(index - 1)..].IsEmpty then ValueNone
    else 
        let e = 
            match code.Get index with 
            | ValueSome '+' | ValueSome '-' -> index + 1
            | _ -> index
        let e = code.[e..] |> Seq.tryFindIndex (fun c -> c <> '_' ) =>= (fun v -> v + e) ?= code.Length
        let e = 
            match code.Get e with 
            | ValueSome c when c <? ('0', '9') -> 
                let e = e + 1
                code.[e..] |> Seq.tryFindIndex (fun c -> ! (c <? ('0', '9') || c = '_') ) =>= (fun v -> v + e) ?= code.Length
            | _ -> index - 1
        ValueSome (code.[e..], new Num(comStr code.[..e]))
and num_body_hex code index =
    let e = code.[index..] |> Seq.tryFindIndex (fun c -> c <> '_' ) =>= (fun v -> v + index) ?= code.Length
    let e = 
        match code.Get e with
        | ValueSome c when c <? ('0', '9') || c <? ('a', 'f') || c <? ('A', 'F') ->
            let e = e + 1
            code.[e..] |> Seq.tryFindIndex (fun c -> ! (c <? ('0', '9') || c <? ('a', 'f') || c <? ('A', 'F') || c = '_') ) =>= (fun v -> v + e) ?= code.Length
        | _ -> index - 1
    ValueSome (code.[e..], new Num(comStr code.[..e]))

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
    | _ -> raise <| ParserError ("Unicode escape not close or illegal characters \t at "  + string(code.RawIndex index))
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









//let parser code = loop  code
