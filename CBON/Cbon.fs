namespace CbStyle.Cbon
open System.Collections.Generic
open System.Globalization

type CbVal =
    | Null
    | Bool of bool
    | Num of Num
    | Str of string
    | Arr of CbVal List
    | Obj of Dictionary<string, CbVal>

    static member inline fNull () = CbVal.Null
    static member inline fBool v = CbVal.Bool v
    static member inline fNum v = CbVal.Num v
    static member inline fStr v = CbVal.Str v
    static member inline fArr v = CbVal.Arr v
    static member inline fObj v = CbVal.Obj v

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
