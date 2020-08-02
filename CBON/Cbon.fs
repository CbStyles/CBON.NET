namespace CbStyle.Cbon
open System.Collections.Generic
open System.Globalization

type CbVal = 
    | Bool of bool
    | Num of decimal
    | Str of string
    | Arr of CbVal
    | Obj of Dictionary<string, CbVal>

type CbAst =
    | Null
    | Bool of bool
    | Num of Num
    | Hex of Hex
    | Str of string
    | Arr of CbAst List
    | Obj of Dictionary<string, CbAst>

    static member inline fNull () = CbAst.Null
    static member inline fBool v = CbAst.Bool v
    static member inline fNum v = CbAst.Num v
    static member inline fHex v = CbAst.Hex v
    static member inline fStr v = CbAst.Str v
    static member inline fArr v = CbAst.Arr v
    static member inline fObj v = CbAst.Obj v

    static member ArrToString (arr: #IList<'v>) = 
        "[" + System.String.Join(",", arr |> Seq.map (fun v -> " " + v.ToString())) + " ]"
    static member ObjToString (obj: #IDictionary<'k, 'v>) = 
        "{" + System.String.Join(",", obj |> Seq.map (fun kv -> " \"" + kv.Key.ToString().Replace("\"", "\\\"") + "\": " + kv.Value.ToString())) + " }"

    static member ToString (arr: #IList<'v>) = CbAst.ArrToString arr
    static member ToString (obj: #IDictionary<'k, 'v>) = CbAst.ObjToString obj
    static member ToString (ast: CbAst) = ast.ToString()
    static member ToString (b: bool) = if b then "true" else "false"
    static member ToString (s: string) = "\"" + s.Replace("\"", "\\\"") + "\""
    static member ToString (n: Num) = n.raw
    static member ToString (n: Hex) = n.raw

    override self.ToString() = 
        match self with
        | Null -> "null"
        | Bool b -> if b then "true" else "false"
        | Num n -> n.raw
        | Hex n -> n.raw
        | Str s -> "\"" + s.Replace("\"", "\\\"") + "\""
        | Arr a -> CbAst.ArrToString a
        | Obj o -> CbAst.ObjToString o

and [<Struct>] Num =
    val raw: string
    new(raw) = { raw = raw }
    override self.ToString() = System.String.Format("Num({0})", self.raw)

    member self.I8 () = System.SByte.Parse(self.raw, NumberStyles.Any)
    member self.I8 (style: NumberStyles) = System.SByte.Parse(self.raw, style)
    member self.I8 (provider: System.IFormatProvider) = System.SByte.Parse(self.raw, provider)
    member self.I8 (style: NumberStyles, provider: System.IFormatProvider) = System.SByte.Parse(self.raw, style, provider)

    member self.I16 () = System.Int16.Parse(self.raw, NumberStyles.Any)
    member self.I16 (style: NumberStyles) = System.Int16.Parse(self.raw, style)
    member self.I16 (provider: System.IFormatProvider) = System.Int16.Parse(self.raw, provider)
    member self.I16 (style: NumberStyles, provider: System.IFormatProvider) = System.Int16.Parse(self.raw, style, provider)

    member self.I32 () = System.Int32.Parse(self.raw, NumberStyles.Any)
    member self.I32 (style: NumberStyles) = System.Int32.Parse(self.raw, style)
    member self.I32 (provider: System.IFormatProvider) = System.Int32.Parse(self.raw, provider)
    member self.I32 (style: NumberStyles, provider: System.IFormatProvider) = System.Int32.Parse(self.raw, style, provider)

    member self.I64 () = System.Int64.Parse(self.raw, NumberStyles.Any)
    member self.I64 (style: NumberStyles) = System.Int64.Parse(self.raw, style)
    member self.I64 (provider: System.IFormatProvider) = System.Int64.Parse(self.raw, provider)
    member self.I64 (style: NumberStyles, provider: System.IFormatProvider) = System.Int64.Parse(self.raw, style, provider)

    member self.U8 () = System.Byte.Parse(self.raw, NumberStyles.Any)
    member self.U8 (style: NumberStyles) = System.Byte.Parse(self.raw, style)
    member self.U8 (provider: System.IFormatProvider) = System.Byte.Parse(self.raw, provider)
    member self.U8 (style: NumberStyles, provider: System.IFormatProvider) = System.Byte.Parse(self.raw, style, provider)

    member self.U16 () = System.UInt16.Parse(self.raw, NumberStyles.Any)
    member self.U16 (style: NumberStyles) = System.UInt16.Parse(self.raw, style)
    member self.U16 (provider: System.IFormatProvider) = System.UInt16.Parse(self.raw, provider)
    member self.U16 (style: NumberStyles, provider: System.IFormatProvider) = System.UInt16.Parse(self.raw, style, provider)

    member self.U32 () = System.UInt32.Parse(self.raw, NumberStyles.Any)
    member self.U32 (style: NumberStyles) = System.UInt32.Parse(self.raw, style)
    member self.U32 (provider: System.IFormatProvider) = System.UInt32.Parse(self.raw, provider)
    member self.U32 (style: NumberStyles, provider: System.IFormatProvider) = System.UInt32.Parse(self.raw, style, provider)

    member self.U64 () = System.UInt64.Parse(self.raw, NumberStyles.Any)
    member self.U64 (style: NumberStyles) = System.UInt64.Parse(self.raw, style)
    member self.U64 (provider: System.IFormatProvider) = System.UInt64.Parse(self.raw, provider)
    member self.U64 (style: NumberStyles, provider: System.IFormatProvider) = System.UInt64.Parse(self.raw, style, provider)

    member self.F32 () = System.Single.Parse(self.raw, NumberStyles.Any)
    member self.F32 (style: NumberStyles) = System.Single.Parse(self.raw, style)
    member self.F32 (provider: System.IFormatProvider) = System.Single.Parse(self.raw, provider)
    member self.F32 (style: NumberStyles, provider: System.IFormatProvider) = System.Single.Parse(self.raw, style, provider)

    member self.F64 () = System.Double.Parse(self.raw, NumberStyles.Any)
    member self.F64 (style: NumberStyles) = System.Double.Parse(self.raw, style)
    member self.F64 (provider: System.IFormatProvider) = System.Double.Parse(self.raw, provider)
    member self.F64 (style: NumberStyles, provider: System.IFormatProvider) = System.Double.Parse(self.raw, style, provider)

    member self.F128 () = System.Decimal.Parse(self.raw, NumberStyles.Any)
    member self.F128 (style: NumberStyles) = System.Decimal.Parse(self.raw, style)
    member self.F128 (provider: System.IFormatProvider) = System.Decimal.Parse(self.raw, provider)
    member self.F128 (style: NumberStyles, provider: System.IFormatProvider) = System.Decimal.Parse(self.raw, style, provider)

    member self.Try (result: sbyte outref) = System.SByte.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: sbyte outref) = System.SByte.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: sbyte outref) = System.SByte.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: sbyte outref) = System.SByte.TryParse(self.raw, style, provider, &result)

    member self.Try (result: int16 outref) = System.Int16.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: int16 outref) = System.Int16.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: int16 outref) = System.Int16.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: int16 outref) = System.Int16.TryParse(self.raw, style, provider, &result)

    member self.Try (result: int32 outref) = System.Int32.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: int32 outref) = System.Int32.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: int32 outref) = System.Int32.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: int32 outref) = System.Int32.TryParse(self.raw, style, provider, &result)

    member self.Try (result: int64 outref) = System.Int64.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: int64 outref) = System.Int64.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: int64 outref) = System.Int64.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: int64 outref) = System.Int64.TryParse(self.raw, style, provider, &result)

    member self.Try (result: byte outref) = System.Byte.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: byte outref) = System.Byte.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: byte outref) = System.Byte.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: byte outref) = System.Byte.TryParse(self.raw, style, provider, &result)

    member self.Try (result: uint16 outref) = System.UInt16.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: uint16 outref) = System.UInt16.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: uint16 outref) = System.UInt16.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: uint16 outref) = System.UInt16.TryParse(self.raw, style, provider, &result)

    member self.Try (result: uint32 outref) = System.UInt32.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: uint32 outref) = System.UInt32.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: uint32 outref) = System.UInt32.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: uint32 outref) = System.UInt32.TryParse(self.raw, style, provider, &result)

    member self.Try (result: uint64 outref) = System.UInt64.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: uint64 outref) = System.UInt64.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: uint64 outref) = System.UInt64.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: uint64 outref) = System.UInt64.TryParse(self.raw, style, provider, &result)

    member self.Try (result: float32 outref) = System.Single.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: float32 outref) = System.Single.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: float32 outref) = System.Single.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: float32 outref) = System.Single.TryParse(self.raw, style, provider, &result)

    member self.Try (result: float outref) = System.Double.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: float outref) = System.Double.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: float outref) = System.Double.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: float outref) = System.Double.TryParse(self.raw, style, provider, &result)

    member self.Try (result: decimal outref) = System.Decimal.TryParse(self.raw, NumberStyles.Any, null, &result)
    member self.Try (style: NumberStyles, result: decimal outref) = System.Decimal.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: decimal outref) = System.Decimal.TryParse(self.raw, NumberStyles.Any, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: decimal outref) = System.Decimal.TryParse(self.raw, style, provider, &result)

and [<Struct>] Hex =
    val raw: string
    new(raw) = { raw = raw }
    override self.ToString() = System.String.Format("Hex({0})", self.raw)

    member self.I8 () = System.SByte.Parse(self.raw, NumberStyles.HexNumber)
    member self.I8 (style: NumberStyles) = System.SByte.Parse(self.raw, style)
    member self.I8 (provider: System.IFormatProvider) = System.SByte.Parse(self.raw, provider)
    member self.I8 (style: NumberStyles, provider: System.IFormatProvider) = System.SByte.Parse(self.raw, style, provider)

    member self.I16 () = System.Int16.Parse(self.raw, NumberStyles.HexNumber)
    member self.I16 (style: NumberStyles) = System.Int16.Parse(self.raw, style)
    member self.I16 (provider: System.IFormatProvider) = System.Int16.Parse(self.raw, provider)
    member self.I16 (style: NumberStyles, provider: System.IFormatProvider) = System.Int16.Parse(self.raw, style, provider)

    member self.I32 () = System.Int32.Parse(self.raw, NumberStyles.HexNumber)
    member self.I32 (style: NumberStyles) = System.Int32.Parse(self.raw, style)
    member self.I32 (provider: System.IFormatProvider) = System.Int32.Parse(self.raw, provider)
    member self.I32 (style: NumberStyles, provider: System.IFormatProvider) = System.Int32.Parse(self.raw, style, provider)

    member self.I64 () = System.Int64.Parse(self.raw, NumberStyles.HexNumber)
    member self.I64 (style: NumberStyles) = System.Int64.Parse(self.raw, style)
    member self.I64 (provider: System.IFormatProvider) = System.Int64.Parse(self.raw, provider)
    member self.I64 (style: NumberStyles, provider: System.IFormatProvider) = System.Int64.Parse(self.raw, style, provider)

    member self.U8 () = System.Byte.Parse(self.raw, NumberStyles.HexNumber)
    member self.U8 (style: NumberStyles) = System.Byte.Parse(self.raw, style)
    member self.U8 (provider: System.IFormatProvider) = System.Byte.Parse(self.raw, provider)
    member self.U8 (style: NumberStyles, provider: System.IFormatProvider) = System.Byte.Parse(self.raw, style, provider)

    member self.U16 () = System.UInt16.Parse(self.raw, NumberStyles.HexNumber)
    member self.U16 (style: NumberStyles) = System.UInt16.Parse(self.raw, style)
    member self.U16 (provider: System.IFormatProvider) = System.UInt16.Parse(self.raw, provider)
    member self.U16 (style: NumberStyles, provider: System.IFormatProvider) = System.UInt16.Parse(self.raw, style, provider)

    member self.U32 () = System.UInt32.Parse(self.raw, NumberStyles.HexNumber)
    member self.U32 (style: NumberStyles) = System.UInt32.Parse(self.raw, style)
    member self.U32 (provider: System.IFormatProvider) = System.UInt32.Parse(self.raw, provider)
    member self.U32 (style: NumberStyles, provider: System.IFormatProvider) = System.UInt32.Parse(self.raw, style, provider)

    member self.U64 () = System.UInt64.Parse(self.raw, NumberStyles.HexNumber)
    member self.U64 (style: NumberStyles) = System.UInt64.Parse(self.raw, style)
    member self.U64 (provider: System.IFormatProvider) = System.UInt64.Parse(self.raw, provider)
    member self.U64 (style: NumberStyles, provider: System.IFormatProvider) = System.UInt64.Parse(self.raw, style, provider)

    member self.F32 () = System.Single.Parse(self.raw, NumberStyles.HexNumber)
    member self.F32 (style: NumberStyles) = System.Single.Parse(self.raw, style)
    member self.F32 (provider: System.IFormatProvider) = System.Single.Parse(self.raw, provider)
    member self.F32 (style: NumberStyles, provider: System.IFormatProvider) = System.Single.Parse(self.raw, style, provider)

    member self.F64 () = System.Double.Parse(self.raw, NumberStyles.HexNumber)
    member self.F64 (style: NumberStyles) = System.Double.Parse(self.raw, style)
    member self.F64 (provider: System.IFormatProvider) = System.Double.Parse(self.raw, provider)
    member self.F64 (style: NumberStyles, provider: System.IFormatProvider) = System.Double.Parse(self.raw, style, provider)

    member self.F128 () = System.Decimal.Parse(self.raw, NumberStyles.HexNumber)
    member self.F128 (style: NumberStyles) = System.Decimal.Parse(self.raw, style)
    member self.F128 (provider: System.IFormatProvider) = System.Decimal.Parse(self.raw, provider)
    member self.F128 (style: NumberStyles, provider: System.IFormatProvider) = System.Decimal.Parse(self.raw, style, provider)

    member self.Try (result: sbyte outref) = System.SByte.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: sbyte outref) = System.SByte.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: sbyte outref) = System.SByte.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: sbyte outref) = System.SByte.TryParse(self.raw, style, provider, &result)

    member self.Try (result: int16 outref) = System.Int16.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: int16 outref) = System.Int16.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: int16 outref) = System.Int16.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: int16 outref) = System.Int16.TryParse(self.raw, style, provider, &result)

    member self.Try (result: int32 outref) = System.Int32.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: int32 outref) = System.Int32.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: int32 outref) = System.Int32.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: int32 outref) = System.Int32.TryParse(self.raw, style, provider, &result)

    member self.Try (result: int64 outref) = System.Int64.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: int64 outref) = System.Int64.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: int64 outref) = System.Int64.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: int64 outref) = System.Int64.TryParse(self.raw, style, provider, &result)

    member self.Try (result: byte outref) = System.Byte.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: byte outref) = System.Byte.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: byte outref) = System.Byte.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: byte outref) = System.Byte.TryParse(self.raw, style, provider, &result)

    member self.Try (result: uint16 outref) = System.UInt16.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: uint16 outref) = System.UInt16.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: uint16 outref) = System.UInt16.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: uint16 outref) = System.UInt16.TryParse(self.raw, style, provider, &result)

    member self.Try (result: uint32 outref) = System.UInt32.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: uint32 outref) = System.UInt32.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: uint32 outref) = System.UInt32.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: uint32 outref) = System.UInt32.TryParse(self.raw, style, provider, &result)

    member self.Try (result: uint64 outref) = System.UInt64.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: uint64 outref) = System.UInt64.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: uint64 outref) = System.UInt64.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: uint64 outref) = System.UInt64.TryParse(self.raw, style, provider, &result)

    member self.Try (result: float32 outref) = System.Single.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: float32 outref) = System.Single.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: float32 outref) = System.Single.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: float32 outref) = System.Single.TryParse(self.raw, style, provider, &result)

    member self.Try (result: float outref) = System.Double.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: float outref) = System.Double.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: float outref) = System.Double.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: float outref) = System.Double.TryParse(self.raw, style, provider, &result)

    member self.Try (result: decimal outref) = System.Decimal.TryParse(self.raw, NumberStyles.HexNumber, null, &result)
    member self.Try (style: NumberStyles, result: decimal outref) = System.Decimal.TryParse(self.raw, style, null, &result)
    member self.Try (provider: System.IFormatProvider, result: decimal outref) = System.Decimal.TryParse(self.raw, NumberStyles.HexNumber, provider, &result)
    member self.Try (style: NumberStyles, provider: System.IFormatProvider, result: decimal outref) = System.Decimal.TryParse(self.raw, style, provider, &result)