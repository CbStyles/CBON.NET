module internal CbStyle.Cbon.Utils

type MutList<'a> = System.Collections.Generic.List<'a>

/// char is in range
let inline (<?) c (f, t) = c >= f && c <= t

/// AndThen
let inline (>>=) a b = 
    match a with
    | ValueSome v -> b v
    | _ -> ValueNone

/// Map
let inline (=>=) a b = 
    match a with
    | Some v -> Some <| b v
    | _ -> None

/// Default
let inline (?=) a b =
    match a with
    | None -> b
    | Some v -> v

/// OrElse
let inline (=>>) a b = 
    match a with
    | ValueNone -> b ()
    | _ -> a

/// Not
let inline (!) a = not a

/// Map
let inline (=>==) a b =
    match a with
    | ValueSome v -> ValueSome <| b v
    | _ -> ValueNone

/// Map Right
let inline (=|=>=) a b =
    match a with
    | ValueSome struct (l, r) -> ValueSome (l, b r)
    | _ -> ValueNone

/// Map Left
let inline (=>=|=) a b =
    match a with
    | ValueSome struct (l, r) -> ValueSome (b l, r)
    | _ -> ValueNone

/// Map Right
let inline r f = fun ((l, r)) -> (l, f r)

/// Map Left
let inline l f = fun ((l, r)) -> (f l, r)