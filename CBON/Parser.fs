module VolumeLight.Cbon.Parser
open VolumeLight.Cbon

let comStr (code: Code Span) = code |> Seq.toArray |> System.String

let inline private (<?) c (f, t) = c >= f && c <= t
let inline private (>>=) a b = ValueOption.bind b a
let inline private (=>=) a b = Option.map b a
let inline private (?=) a b = Option.defaultValue b a
let inline private (!) a = not a

let rec loop (code: Code Span) = 
    failwith "todo"
and loop_body (code: Code Span) = 
    failwith "todo"

and num (code: Code Span) = 
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













//let parser code = loop  code
