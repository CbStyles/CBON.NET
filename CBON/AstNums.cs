using System;
using System.Globalization;
using System.Numerics;

namespace CbStyles.Cbon
{
    public partial struct CbVal 
    {

        public bool IsNumber => IsNum || IsHex;

        private class NumCache
        {
            public sbyte? i8 = null;
            public short? i16 = null;
            public int? i32 = null;
            public long? i64 = null;
            #if !NETSTANDARD
            public nint? isize = null;
            #endif
            public byte? u8 = null;
            public ushort? u16 = null;
            public uint? u32 = null;
            public ulong? u64 = null;
            #if !NETSTANDARD
            public nuint? usize = null;
            #endif
            public float? f32 = null;
            public double? f64 = null;

            #region Set

            public static sbyte Set(ref NumCache? self, sbyte v)
            {
                self ??= new NumCache();
                self.i8 = v;
                return v;
            }
            public static short Set(ref NumCache? self, short v)
            {
                self ??= new NumCache();
                self.i16 = v;
                return v;
            }
            public static int Set(ref NumCache? self, int v)
            {
                self ??= new NumCache();
                self.i32 = v;
                return v;
            }
            public static long Set(ref NumCache? self, long v)
            {
                self ??= new NumCache();
                self.i64 = v;
                return v;
            }
            #if !NETSTANDARD
            public static nint Set(ref NumCache? self, nint v)
            {
                self ??= new NumCache();
                self.isize = v;
                return v;
            }
            #endif
            public static byte Set(ref NumCache? self, byte v)
            {
                self ??= new NumCache();
                self.u8 = v;
                return v;
            }
            public static ushort Set(ref NumCache? self, ushort v)
            {
                self ??= new NumCache();
                self.u16 = v;
                return v;
            }
            public static uint Set(ref NumCache? self, uint v)
            {
                self ??= new NumCache();
                self.u32 = v;
                return v;
            }
            public static ulong Set(ref NumCache? self, ulong v)
            {
                self ??= new NumCache();
                self.u64 = v;
                return v;
            }
            #if !NETSTANDARD
            public static nuint Set(ref NumCache? self, nuint v)
            {
                self ??= new NumCache();
                self.usize = v;
                return v;
            }
            #endif
            public static float Set(ref NumCache? self, float v)
            {
                self ??= new NumCache();
                self.f32 = v;
                return v;
            }
            public static double Set(ref NumCache? self, double v)
            {
                self ??= new NumCache();
                self.f64 = v;
                return v;
            }

            #endregion
        }

        #region Parse Cached

        public sbyte I8Cached()
        {
            if (numCache?.i8 != null) return numCache!.i8.Value;
            return NumCache.Set(ref numCache, I8());
        }

        public short I16Cached()
        {
            if (numCache?.i16 != null) return numCache!.i16.Value;
            return NumCache.Set(ref numCache, I16());
        }

        public int I32Cached()
        {
            if (numCache?.i32 != null) return numCache!.i32.Value;
            return NumCache.Set(ref numCache, I32());
        }

        public long I64Cached()
        {
            if (numCache?.i64 != null) return numCache!.i64.Value;
            return NumCache.Set(ref numCache, I64());
        }

        #if !NETSTANDARD
        public nint ISizeCached()
        {
            if (numCache?.isize != null) return numCache!.isize.Value;
            return NumCache.Set(ref numCache, ISize());
        }
        #endif

        public byte U8Cached()
        {
            if (numCache?.u8 != null) return numCache!.u8.Value;
            return NumCache.Set(ref numCache, U8());
        }

        public ushort U16Cached()
        {
            if (numCache?.u16 != null) return numCache!.u16.Value;
            return NumCache.Set(ref numCache, U16());
        }

        public uint U32Cached()
        {
            if (numCache?.u32 != null) return numCache!.u32.Value;
            return NumCache.Set(ref numCache, U32());
        }

        public ulong U64Cached()
        {
            if (numCache?.u64 != null) return numCache!.u64.Value;
            return NumCache.Set(ref numCache, U64());
        }

        #if !NETSTANDARD
        public nuint USizeCached()
        {
            if (numCache?.usize != null) return numCache!.usize.Value;
            return NumCache.Set(ref numCache, USize());
        }
        #endif
        public float F32Cached()
        {
            if (numCache?.f32 != null) return numCache!.f32.Value;
            return NumCache.Set(ref numCache, F32());
        }

        public double F64Cached()
        {
            if (numCache?.f64 != null) return numCache!.f64.Value;
            return NumCache.Set(ref numCache, F64());
        }

        #endregion

        #region Parse

        public sbyte I8() =>
            IsNum ? sbyte.Parse(str!, NumberStyles.Any) :
            IsHex ? sbyte.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public sbyte I8(NumberStyles styles) => IsNumber ? sbyte.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public sbyte I8(IFormatProvider provider) => IsNumber ? sbyte.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public sbyte I8(NumberStyles styles, IFormatProvider provider) => IsNumber ? sbyte.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        
        public short I16() =>
            IsNum ? short.Parse(str!, NumberStyles.Any) :
            IsHex ? short.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public short I16(NumberStyles styles) => IsNumber ? short.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public short I16(IFormatProvider provider) => IsNumber ? short.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public short I16(NumberStyles styles, IFormatProvider provider) => IsNumber ? short.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        
        public int I32() =>
            IsNum ? int.Parse(str!, NumberStyles.Any) :
            IsHex ? int.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public int I32(NumberStyles styles) => IsNumber ? int.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public int I32(IFormatProvider provider) => IsNumber ? int.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public int I32(NumberStyles styles, IFormatProvider provider) => IsNumber ? int.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        public long I64() =>
            IsNum ? long.Parse(str!, NumberStyles.Any) :
            IsHex ? long.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public long I64(NumberStyles styles) => IsNumber ? long.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public long I64(IFormatProvider provider) => IsNumber ? long.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public long I64(NumberStyles styles, IFormatProvider provider) => IsNumber ? long.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        #if !NETSTANDARD
        public nint ISize() =>
            IsNum ? nint.Parse(str!, NumberStyles.Any) :
            IsHex ? nint.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public nint ISize(NumberStyles styles) => IsNumber ? nint.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public nint ISize(IFormatProvider provider) => IsNumber ? nint.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public nint ISize(NumberStyles styles, IFormatProvider provider) => IsNumber ? nint.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        #endif

        public byte U8() =>
            IsNum ? byte.Parse(str!, NumberStyles.Any) :
            IsHex ? byte.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public byte U8(NumberStyles styles) => IsNumber ? byte.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public byte U8(IFormatProvider provider) => IsNumber ? byte.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public byte U8(NumberStyles styles, IFormatProvider provider) => IsNumber ? byte.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        public ushort U16() =>
            IsNum ? ushort.Parse(str!, NumberStyles.Any) :
            IsHex ? ushort.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public ushort U16(NumberStyles styles) => IsNumber ? ushort.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public ushort U16(IFormatProvider provider) => IsNumber ? ushort.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public ushort U16(NumberStyles styles, IFormatProvider provider) => IsNumber ? ushort.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        public uint U32() =>
            IsNum ? uint.Parse(str!, NumberStyles.Any) :
            IsHex ? uint.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public uint U32(NumberStyles styles) => IsNumber ? uint.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public uint U32(IFormatProvider provider) => IsNumber ? uint.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public uint U32(NumberStyles styles, IFormatProvider provider) => IsNumber ? uint.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        public ulong U64() =>
            IsNum ? ulong.Parse(str!, NumberStyles.Any) :
            IsHex ? ulong.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public ulong U64(NumberStyles styles) => IsNumber ? ulong.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public ulong U64(IFormatProvider provider) => IsNumber ? ulong.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public ulong U64(NumberStyles styles, IFormatProvider provider) => IsNumber ? ulong.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        #if !NETSTANDARD
        public nuint USize() =>
            IsNum ? nuint.Parse(str!, NumberStyles.Any) :
            IsHex ? nuint.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public nuint USize(NumberStyles styles) => IsNumber ? nuint.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public nuint USize(IFormatProvider provider) => IsNumber ? nuint.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public nuint USize(NumberStyles styles, IFormatProvider provider) => IsNumber ? nuint.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        #endif

        #if !NETSTANDARD
        public Half F16() =>
            IsNum ? Half.Parse(str!, NumberStyles.Any) :
            IsHex ? Half.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public Half F16(NumberStyles styles) => IsNumber ? Half.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public Half F16(IFormatProvider provider) => IsNumber ? Half.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public Half F16(NumberStyles styles, IFormatProvider provider) => IsNumber ? Half.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        #endif

        public float F32() =>
            IsNum ? float.Parse(str!, NumberStyles.Any) :
            IsHex ? float.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public float F32(NumberStyles styles) => IsNumber ? float.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public float F32(IFormatProvider provider) => IsNumber ? float.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public float F32(NumberStyles styles, IFormatProvider provider) => IsNumber ? float.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        public double F64() =>
            IsNum ? double.Parse(str!, NumberStyles.Any) :
            IsHex ? double.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public double F64(NumberStyles styles) => IsNumber ? double.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public double F64(IFormatProvider provider) => IsNumber ? double.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public double F64(NumberStyles styles, IFormatProvider provider) => IsNumber ? double.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        public decimal F128() =>
            IsNum ? decimal.Parse(str!, NumberStyles.Any) :
            IsHex ? decimal.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public decimal F128(NumberStyles styles) => IsNumber ? decimal.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public decimal F128(IFormatProvider provider) => IsNumber ? decimal.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public decimal F128(NumberStyles styles, IFormatProvider provider) => IsNumber ? decimal.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        public BigInteger BigInt() =>
            IsNum ? BigInteger.Parse(str!, NumberStyles.Any) :
            IsHex ? BigInteger.Parse(str!, NumberStyles.HexNumber) :
            throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public BigInteger BigInt(NumberStyles styles) => IsNumber ? BigInteger.Parse(str!, styles) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public BigInteger BigInt(IFormatProvider provider) => IsNumber ? BigInteger.Parse(str!, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");
        public BigInteger BigInt(NumberStyles styles, IFormatProvider provider) => IsNumber ? BigInteger.Parse(str!, styles, provider) : throw KindErr($"{nameof(CbType.Num)} or {nameof(CbType.Hex)}");

        #endregion

        #region TryParse out Cached

        public bool TryI8Cached(out sbyte result)
        {
            if (numCache?.i8 != null)
            {
                result = numCache!.i8.Value;
                return true;
            }
            if (TryI8(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        public bool TryI16Cached(out short result)
        {
            if (numCache?.i16 != null)
            {
                result = numCache!.i16.Value;
                return true;
            }
            if (TryI16(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        public bool TryI32Cached(out int result)
        {
            if (numCache?.i32 != null)
            {
                result = numCache!.i32.Value;
                return true;
            }
            if (TryI32(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        public bool TryI64Cached(out long result)
        {
            if (numCache?.i64 != null)
            {
                result = numCache!.i64.Value;
                return true;
            }
            if (TryI64(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        #if !NETSTANDARD
        public bool TryISizeCached(out nint result)
        {
            if (numCache?.isize != null)
            {
                result = numCache!.isize.Value;
                return true;
            }
            if (TryISize(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }
        #endif

        public bool TryU8Cached(out byte result)
        {
            if (numCache?.u8 != null)
            {
                result = numCache!.u8.Value;
                return true;
            }
            if (TryU8(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        public bool TryU16Cached(out ushort result)
        {
            if (numCache?.u16 != null)
            {
                result = numCache!.u16.Value;
                return true;
            }
            if (TryU16(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        public bool TryU32Cached(out uint result)
        {
            if (numCache?.u32 != null)
            {
                result = numCache!.u32.Value;
                return true;
            }
            if (TryU32(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        public bool TryU64Cached(out ulong result)
        {
            if (numCache?.u64 != null)
            {
                result = numCache!.u64.Value;
                return true;
            }
            if (TryU64(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }
        #if !NETSTANDARD
        public bool TryUSizeCached(out nuint result)
        {
            if (numCache?.usize != null)
            {
                result = numCache!.usize.Value;
                return true;
            }
            if (TryUSize(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }
        #endif

        public bool TryF32Cached(out float result)
        {
            if (numCache?.f32 != null)
            {
                result = numCache!.f32.Value;
                return true;
            }
            if (TryF32(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        public bool TryF64Cached(out double result)
        {
            if (numCache?.f64 != null)
            {
                result = numCache!.f64.Value;
                return true;
            }
            if (TryF64(out result))
            {
                NumCache.Set(ref numCache, result);
                return true;
            }
            return false;
        }

        #endregion

        #region TryParse out

        private static bool OutDefault<T>(out T result) where T : unmanaged
        {
            result = default;
            return false;
        }

        public bool TryI8(out sbyte result) =>
            IsNum ? sbyte.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? sbyte.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryI8(NumberStyles styles, out sbyte result) => IsNumber ? sbyte.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryI8(IFormatProvider provider, out sbyte result) =>
            IsNum ? sbyte.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? sbyte.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryI8(NumberStyles styles, IFormatProvider provider, out sbyte result) => IsNumber ? sbyte.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        public bool TryI16(out short result) =>
            IsNum ? short.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? short.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryI16(NumberStyles styles, out short result) => IsNumber ? short.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryI16(IFormatProvider provider, out short result) =>
            IsNum ? short.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? short.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryI16(NumberStyles styles, IFormatProvider provider, out short result) => IsNumber ? short.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        public bool TryI32(out int result) =>
            IsNum ? int.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? int.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryI32(NumberStyles styles, out int result) => IsNumber ? int.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryI32(IFormatProvider provider, out int result) =>
            IsNum ? int.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? int.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryI32(NumberStyles styles, IFormatProvider provider, out int result) => IsNumber ? int.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        public bool TryI64(out long result) =>
            IsNum ? long.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? long.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryI64(NumberStyles styles, out long result) => IsNumber ? long.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryI64(IFormatProvider provider, out long result) =>
            IsNum ? long.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? long.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryI64(NumberStyles styles, IFormatProvider provider, out long result) => IsNumber ? long.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        #if !NETSTANDARD
        public bool TryISize(out nint result) =>
            IsNum ? nint.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? nint.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryISize(NumberStyles styles, out nint result) => IsNumber ? nint.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryISize(IFormatProvider provider, out nint result) =>
            IsNum ? nint.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? nint.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryISize(NumberStyles styles, IFormatProvider provider, out nint result) => IsNumber ? nint.TryParse(str!, styles, provider, out result) : OutDefault(out result);
        #endif
        public bool TryU8(out byte result) =>
            IsNum ? byte.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? byte.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryU8(NumberStyles styles, out byte result) => IsNumber ? byte.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryU8(IFormatProvider provider, out byte result) =>
            IsNum ? byte.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? byte.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryU8(NumberStyles styles, IFormatProvider provider, out byte result) => IsNumber ? byte.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        public bool TryU16(out ushort result) =>
            IsNum ? ushort.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? ushort.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryU16(NumberStyles styles, out ushort result) => IsNumber ? ushort.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryU16(IFormatProvider provider, out ushort result) =>
            IsNum ? ushort.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? ushort.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryU16(NumberStyles styles, IFormatProvider provider, out ushort result) => IsNumber ? ushort.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        public bool TryU32(out uint result) =>
            IsNum ? uint.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? uint.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryU32(NumberStyles styles, out uint result) => IsNumber ? uint.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryU32(IFormatProvider provider, out uint result) =>
            IsNum ? uint.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? uint.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryU32(NumberStyles styles, IFormatProvider provider, out uint result) => IsNumber ? uint.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        public bool TryU64(out ulong result) =>
            IsNum ? ulong.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? ulong.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryU64(NumberStyles styles, out ulong result) => IsNumber ? ulong.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryU64(IFormatProvider provider, out ulong result) =>
            IsNum ? ulong.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? ulong.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryU64(NumberStyles styles, IFormatProvider provider, out ulong result) => IsNumber ? ulong.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        #if !NETSTANDARD
        public bool TryUSize(out nuint result) =>
            IsNum ? nuint.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? nuint.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryUSize(NumberStyles styles, out nuint result) => IsNumber ? nuint.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryUSize(IFormatProvider provider, out nuint result) =>
            IsNum ? nuint.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? nuint.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryUSize(NumberStyles styles, IFormatProvider provider, out nuint result) => IsNumber ? nuint.TryParse(str!, styles, provider, out result) : OutDefault(out result);
        #endif

        public bool TryF32(out float result) =>
            IsNum ? float.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? float.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryF32(NumberStyles styles, out float result) => IsNumber ? float.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryF32(IFormatProvider provider, out float result) =>
            IsNum ? float.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? float.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryF32(NumberStyles styles, IFormatProvider provider, out float result) => IsNumber ? float.TryParse(str!, styles, provider, out result) : OutDefault(out result);

        public bool TryF64(out double result) =>
            IsNum ? double.TryParse(str!, NumberStyles.Any, null, out result) :
            IsHex ? double.TryParse(str!, NumberStyles.HexNumber, null, out result) :
            OutDefault(out result);
        public bool TryF64(NumberStyles styles, out double result) => IsNumber ? double.TryParse(str!, styles, null, out result) : OutDefault(out result);
        public bool TryF64(IFormatProvider provider, out double result) =>
            IsNum ? double.TryParse(str!, NumberStyles.Any, provider, out result) :
            IsHex ? double.TryParse(str!, NumberStyles.HexNumber, provider, out result) :
            OutDefault(out result);
        public bool TryF64(NumberStyles styles, IFormatProvider provider, out double result) => IsNumber ? double.TryParse(str!, styles, provider, out result) : OutDefault(out result);

#endregion

        #region Parse Cached

        public sbyte? TryI8Cached()
        {
            if (numCache?.i8 != null) return numCache!.i8.Value;
            var v = TryI8();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        public short? TryI16Cached()
        {
            if (numCache?.i16 != null) return numCache!.i16.Value;
            var v = TryI16();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        public int? TryI32Cached()
        {
            if (numCache?.i32 != null) return numCache!.i32.Value;
            var v = TryI32();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        public long? TryI64Cached()
        {
            if (numCache?.i64 != null) return numCache!.i64.Value;
            var v = TryI64();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        #if !NETSTANDARD
        public nint? TryISizeCached()
        {
            if (numCache?.isize != null) return numCache!.isize.Value;
            var v = TryISize();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }
        #endif

        public byte? TryU8Cached()
        {
            if (numCache?.u8 != null) return numCache!.u8.Value;
            var v = TryU8();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        public ushort? TryU16Cached()
        {
            if (numCache?.u16 != null) return numCache!.u16.Value;
            var v = TryU16();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        public uint? TryU32Cached()
        {
            if (numCache?.u32 != null) return numCache!.u32.Value;
            var v = TryU32();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        public ulong? TryU64Cached()
        {
            if (numCache?.u64 != null) return numCache!.u64.Value;
            var v = TryU64();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        #if !NETSTANDARD
        public nuint? TryUSizeCached()
        {
            if (numCache?.usize != null) return numCache!.usize.Value;
            var v = TryUSize();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }
        #endif

        public float? TryF32Cached()
        {
            if (numCache?.f32 != null) return numCache!.f32.Value;
            var v = TryF32();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        public double? TryF64Cached()
        {
            if (numCache?.f64 != null) return numCache!.f64.Value;
            var v = TryF64();
            if (v != null) NumCache.Set(ref numCache, v.Value);
            return v;
        }

        #endregion

        #region TryParse Nullable

        public sbyte? TryI8() => TryI8(out var result) ? result : null;
        public sbyte? TryI8(NumberStyles styles) => TryI8(styles, out var result) ? result : null;
        public sbyte? TryI8(IFormatProvider provider) => TryI8(provider, out var result) ? result : null;
        public sbyte? TryI8(NumberStyles styles, IFormatProvider provider) => TryI8(styles, provider, out var result) ? result : null;

        public short? TryI16() => TryI16(out var result) ? result : null;
        public short? TryI16(NumberStyles styles) => TryI16(styles, out var result) ? result : null;
        public short? TryI16(IFormatProvider provider) => TryI16(provider, out var result) ? result : null;
        public short? TryI16(NumberStyles styles, IFormatProvider provider) => TryI16(styles, provider, out var result) ? result : null;

        public int? TryI32() => TryI32(out var result) ? result : null;
        public int? TryI32(NumberStyles styles) => TryI32(styles, out var result) ? result : null;
        public int? TryI32(IFormatProvider provider) => TryI32(provider, out var result) ? result : null;
        public int? TryI32(NumberStyles styles, IFormatProvider provider) => TryI32(styles, provider, out var result) ? result : null;

        public long? TryI64() => TryI64(out var result) ? result : null;
        public long? TryI64(NumberStyles styles) => TryI64(styles, out var result) ? result : null;
        public long? TryI64(IFormatProvider provider) => TryI64(provider, out var result) ? result : null;
        public long? TryI64(NumberStyles styles, IFormatProvider provider) => TryI64(styles, provider, out var result) ? result : null;

        #if !NETSTANDARD
        public nint? TryISize() => TryISize(out var result) ? result : null;
        public nint? TryISize(NumberStyles styles) => TryISize(styles, out var result) ? result : null;
        public nint? TryISize(IFormatProvider provider) => TryISize(provider, out var result) ? result : null;
        public nint? TryISize(NumberStyles styles, IFormatProvider provider) => TryISize(styles, provider, out var result) ? result : null;
        #endif

        public byte? TryU8() => TryU8(out var result) ? result : null;
        public byte? TryU8(NumberStyles styles) => TryU8(styles, out var result) ? result : null;
        public byte? TryU8(IFormatProvider provider) => TryU8(provider, out var result) ? result : null;
        public byte? TryU8(NumberStyles styles, IFormatProvider provider) => TryU8(styles, provider, out var result) ? result : null;

        public ushort? TryU16() => TryU16(out var result) ? result : null;
        public ushort? TryU16(NumberStyles styles) => TryU16(styles, out var result) ? result : null;
        public ushort? TryU16(IFormatProvider provider) => TryU16(provider, out var result) ? result : null;
        public ushort? TryU16(NumberStyles styles, IFormatProvider provider) => TryU16(styles, provider, out var result) ? result : null;

        public uint? TryU32() => TryU32(out var result) ? result : null;
        public uint? TryU32(NumberStyles styles) => TryU32(styles, out var result) ? result : null;
        public uint? TryU32(IFormatProvider provider) => TryU32(provider, out var result) ? result : null;
        public uint? TryU32(NumberStyles styles, IFormatProvider provider) => TryU32(styles, provider, out var result) ? result : null;

        public ulong? TryU64() => TryU64(out var result) ? result : null;
        public ulong? TryU64(NumberStyles styles) => TryU64(styles, out var result) ? result : null;
        public ulong? TryU64(IFormatProvider provider) => TryU64(provider, out var result) ? result : null;
        public ulong? TryU64(NumberStyles styles, IFormatProvider provider) => TryU64(styles, provider, out var result) ? result : null;

        #if !NETSTANDARD
        public nuint? TryUSize() => TryUSize(out var result) ? result : null;
        public nuint? TryUSize(NumberStyles styles) => TryUSize(styles, out var result) ? result : null;
        public nuint? TryUSize(IFormatProvider provider) => TryUSize(provider, out var result) ? result : null;
        public nuint? TryUSize(NumberStyles styles, IFormatProvider provider) => TryUSize(styles, provider, out var result) ? result : null;
        #endif

        public float? TryF32() => TryF32(out var result) ? result : null;
        public float? TryF32(NumberStyles styles) => TryF32(styles, out var result) ? result : null;
        public float? TryF32(IFormatProvider provider) => TryF32(provider, out var result) ? result : null;
        public float? TryF32(NumberStyles styles, IFormatProvider provider) => TryF32(styles, provider, out var result) ? result : null;

        public double? TryF64() => TryF64(out var result) ? result : null;
        public double? TryF64(NumberStyles styles) => TryF64(styles, out var result) ? result : null;
        public double? TryF64(IFormatProvider provider) => TryF64(provider, out var result) ? result : null;
        public double? TryF64(NumberStyles styles, IFormatProvider provider) => TryF64(styles, provider, out var result) ? result : null;

        #endregion
    }
}
