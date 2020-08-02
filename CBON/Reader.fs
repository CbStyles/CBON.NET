namespace CbStyle.Cbon
open System.Collections.Generic
open System.Collections

[<Struct>]
type Pos =
    val mutable line: uint64
    val mutable column: uint64
    new(line, column) = { line = line; column = column }
    override self.ToString () = System.String.Format("{{ Pos {0}:{1} }}", self.line, self.column)

[<Struct>]
type Loc = 
    val mutable from: Pos
    val mutable ``to``: Pos
    new(from, ``to``) = { from = from; ``to`` = ``to`` }
    new(pos) = { from = pos; ``to`` = pos }
    new(line, column) = Loc(Pos(line, column))
    new(line1, column1, line2, column2) = Loc(Pos(line1, column1), Pos(line2, column2))
    override self.ToString () = System.String.Format("{{ Loc {0}:{1} .. {2}:{3} }}", self.from.line, self.from.column, self.``to``.line, self.``to``.column)

type Code = char

[<Struct>]
[<CustomEquality>]
[<NoComparison>]
type Span<'T>(arr: 'T [] ref, from: int, toend: int) =
    new(arr) = Span(arr, 0, arr.Value.Length)
    member _.Raw = arr
    member _.From = from
    member _.To = toend
    member _.Length = toend - from
    member inline self.IsEmpty = self.Length = 0
    member inline self.IsNotEmpty = self.Length > 0
    member _.RawIndex idx = from + idx
    member self.GetSlice(startIdx, endIdx) = 
        let s, e = (defaultArg startIdx 0, defaultArg endIdx self.Length)
        Span(arr, from + s, from + e)
    member _.Item
        with get idx = arr.Value.[from + idx]
        and set idx v = arr.Value.[from + idx] <- v
    member self.Get idx = 
        if from + idx >= toend then ValueNone
        else ValueSome self.[idx]
    interface seq<'T> with
        member self.GetEnumerator(): IEnumerator = upcast new SpanIter<'T>(self)
        member self.GetEnumerator(): IEnumerator<'T> = upcast new SpanIter<'T>(self)
    override self.Equals other =
        match other with
        | :? Span<'T> as s when self.Length = s.Length && self.Length = 0 -> true
        | :? Span<'T> as s when self.Length = s.Length -> (Seq.zip self s |> Seq.tryFindIndex (fun ((a, b)) -> not <| (a :> obj).Equals b)).IsNone
        | :? ('T []) as a when self.Length = a.Length && self.Length = 0 -> true
        | :? ('T []) as a when self.Length = a.Length -> (Seq.zip self a |> Seq.tryFindIndex (fun ((a, b)) -> not <| (a :> obj).Equals b)).IsNone
        | _ -> false
    override self.GetHashCode() = (self.Raw.Value :> obj).GetHashCode() + self.From.GetHashCode() + self.To.GetHashCode()
    override self.ToString () = System.String.Format("Span[{0}]", System.String.Join(", ", self |> Seq.map (fun v -> v.ToString())) ) 

and SpanIter<'T>(span: Span<'T>) =
    let mutable i = 0
    interface IEnumerator<'T> with
        member _.Current: 'T = span.[i - 1]
        member _.Current: obj = upcast span.[i - 1]
        member _.Dispose(): unit = ()
        member _.Reset(): unit = i <- 0
        member _.MoveNext() = 
            if i >= span.Length then false
            else 
                i <- i + 1
                true

module Reader = 
    let reader (code: #seq<char>) : Code Span = 
        let mutable r = false
        let r = Seq.toArray <| seq {
            for c in code do
            match c with
            | '\r' -> 
                yield '\n'
                r <- true
            | '\n' ->
                if not r then yield '\n'
                r <- false
            | c -> 
                yield c
                r <- false
        }
        Span(ref r)
        