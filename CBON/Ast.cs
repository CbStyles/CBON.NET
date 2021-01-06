using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CbStyles.Cbon
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial struct CbVal : IEquatable<CbVal>
    {
        public enum CbType
        {
            Null, Bool, Num, Hex, Date, UUID, Str, Arr, Obj, Union
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UnionStruct
        {
            [FieldOffset(0)]
            public bool boolean;
            [FieldOffset(0)]
            public DateTime date;
            [FieldOffset(0)]
            public Guid uuid;
        }

        public CbType Type { get; }
        private readonly UnionStruct data;
        private readonly string? str;
        private readonly List<CbVal>? arr;
        private readonly Dictionary<string, CbVal>? obj;
        private readonly CbUnion? union;

        private NumCache? numCache;

        #region Constructor

        private CbVal(CbType tag)
        {
            Type = tag;
            data = default;
            str = null;
            arr = null;
            obj = null;
            union = null;
            numCache = null;
        }
        private CbVal(bool boolean)
        {
            Type = CbType.Bool;
            data = new UnionStruct { boolean = boolean };
            str = null;
            arr = null;
            obj = null;
            union = null;
            numCache = null;
        }
        private CbVal(DateTime date)
        {
            Type = CbType.Date;
            data = new UnionStruct { date = date };
            str = null;
            arr = null;
            obj = null;
            union = null;
            numCache = null;
        }
        private CbVal(Guid uuid)
        {
            Type = CbType.UUID;
            data = new UnionStruct { uuid = uuid };
            str = null;
            arr = null;
            obj = null;
            union = null;
            numCache = null;
        }
        private CbVal(CbType tag, string str)
        {
            Type = tag;
            data = default;
            this.str = str;
            arr = null;
            obj = null;
            union = null;
            numCache = null;
        }
        private CbVal(List<CbVal> arr)
        {
            Type = CbType.Arr;
            data = default;
            str = null;
            this.arr = arr;
            obj = null;
            union = null;
            numCache = null;
        }
        private CbVal(Dictionary<string, CbVal> obj)
        {
            Type = CbType.Obj;
            data = default;
            str = null;
            arr = null;
            this.obj = obj;
            union = null;
            numCache = null;
        }
        private CbVal(CbUnion union)
        {
            Type = CbType.Union;
            data = default;
            str = null;
            arr = null;
            obj = null;
            this.union = union;
            numCache = null;
        }

        #endregion

        #region IsX

        public bool IsNull => Type == CbType.Null;
        public bool IsBool => Type == CbType.Bool;
        public bool IsDate => Type == CbType.Date;
        public bool IsUUID => Type == CbType.UUID;
        public bool IsNum => Type == CbType.Num;
        public bool IsHex => Type == CbType.Hex;
        public bool IsStr => Type == CbType.Str;
        public bool IsArr => Type == CbType.Arr;
        public bool IsObj => Type == CbType.Obj;
        public bool IsUnion => Type == CbType.Union;

        #endregion

        #region Get

        public bool Bool => IsBool ? data.boolean : throw KindErr(nameof(CbType.Bool));
        public DateTime Date => IsDate ? data.date : throw KindErr(nameof(CbType.Date));
        public Guid UUID => IsUUID ? data.uuid : throw KindErr(nameof(CbType.UUID));
        public string Num => IsNum ? str! : throw KindErr(nameof(CbType.Num));
        public string Hex => IsHex ? str! : throw KindErr(nameof(CbType.Hex));
        public string Str => IsStr ? str! : throw KindErr(nameof(CbType.Str));
        public List<CbVal> Arr => IsArr ? arr! : throw KindErr(nameof(CbType.Arr));
        public Dictionary<string, CbVal> Obj => IsObj ? obj! : throw KindErr(nameof(CbType.Obj));
        public CbUnion Union => IsUnion ? union! : throw KindErr(nameof(CbType.Union));

        #endregion

        #region TryGet

        public bool? TryBool => IsBool ? data.boolean : null;
        public DateTime? TryDate => IsDate ? data.date : null;
        public Guid? TryUUID => IsUUID ? data.uuid : null;
        public string? TryNum => IsNum ? str! : null;
        public string? TryHex => IsHex ? str! : null;
        public string? TryStr => IsStr ? str! : null;
        public List<CbVal>? TryArr => IsArr ? arr! : null;
        public Dictionary<string, CbVal>? TryObj => IsObj ? obj! : null;
        public CbUnion? TryUnion => IsUnion ? union! : null;

        #endregion

        #region Char

        public bool IsChar => IsStr && str!.Length == 1;

        public char? Char() => IsChar ? str![0] : null;

        public bool Char(out char result)
        {
            if (IsChar)
            {
                result = str![0];
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        #endregion

        #region New

        public static CbVal NewNull() => new CbVal(CbType.Null);
        public static CbVal NewBool(bool val) => new CbVal(val);
        public static CbVal NewDate(string date) => new CbVal(DateTime.Parse(date));
        public static CbVal NewDate(DateTime date) => new CbVal(date);
        public static CbVal NewUUID(string uuid) => new CbVal(new Guid(uuid));
        public static CbVal NewUUID(Guid uuid) => new CbVal(uuid);
        public static CbVal NewNum(string raw) => new CbVal(CbType.Num, raw.Replace("_", ""));
        public static CbVal NewHex(string raw) => new CbVal(CbType.Hex, raw);
        public static CbVal NewStr(string str) => new CbVal(CbType.Str, str);
        public static CbVal NewArr(List<CbVal> arr) => new CbVal(arr);
        public static CbVal NewObj(Dictionary<string, CbVal> obj) => new CbVal(obj);
        public static CbVal NewUnion(CbUnion union) => new CbVal(union);
        public static CbVal NewUnion(string tag, CbVal val) => new CbVal(new CbUnion(tag, val));

        #endregion

        #region Equals

        public override bool Equals(object? obj) => obj is CbVal val && Equals(val);

        public bool Equals(CbVal other) => Type == other.Type && Type switch
        {
            CbType.Null => true,
            CbType.Bool => data.boolean == other.data.boolean,
            CbType.Date => data.date == other.data.date,
            CbType.UUID => data.uuid == other.data.uuid,
            CbType.Num => str! == other.str!,
            CbType.Hex => str! == other.str!,
            CbType.Str => str! == other.str!,
            CbType.Arr => arr! == other.arr!,
            CbType.Obj => obj! == other.obj!,
            CbType.Union => union! == other.union!,
            _ => throw new NotImplementedException(),
        };

        public override int GetHashCode() => Type switch
        {
            CbType.Null => HashCode.Combine(Type),
            CbType.Bool => HashCode.Combine(Type, data.boolean),
            CbType.Date => HashCode.Combine(Type, data.date),
            CbType.UUID => HashCode.Combine(Type, data.uuid),
            CbType.Num => HashCode.Combine(Type, str),
            CbType.Hex => HashCode.Combine(Type, str),
            CbType.Str => HashCode.Combine(Type, str),
            CbType.Arr => HashCode.Combine(Type, arr!),
            CbType.Obj => HashCode.Combine(Type, obj!),
            CbType.Union => HashCode.Combine(Type, union!),
            _ => throw new NotImplementedException(),
        };

        public static bool operator ==(CbVal left, CbVal right) => left.Equals(right);

        public static bool operator !=(CbVal left, CbVal right) => !(left == right);

        #endregion

        private static ArgumentException KindErr(string type) => new ArgumentException($"Type Error, This {nameof(CbVal)} type is not ${type}");

        private string GetDebuggerDisplay() => $"{nameof(CbVal)}:{TypeString()}: {ToString()}";

        public string TypeString() => Type switch
        {
            CbType.Null => "Null",
            CbType.Bool => "Bool",
            CbType.Num => "Num",
            CbType.Hex => "Hex",
            CbType.Date => "Date",
            CbType.UUID => "UUID",
            CbType.Str => "Str",
            CbType.Arr => "Arr",
            CbType.Obj => "Obj",
            CbType.Union => "Union",
            _ => throw new NotImplementedException("never"),
        };

        public override string ToString() => Type switch
        {
            CbType.Null => "null",
            CbType.Bool => data.boolean.ToString(),
            CbType.Num => str!,
            CbType.Hex => $"0x{str!}",
            CbType.Date => data.date.ToString(),
            CbType.UUID => data.uuid.ToString(),
            CbType.Str => $"\"{str!}\"",// todo escape quote
            CbType.Arr => "todo",
            CbType.Obj => "todo",
            CbType.Union => union!.ToString(),
            _ => throw new NotImplementedException("never"),
        };
    }

    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public record CbUnion
    {
        public readonly string Tag;
        public readonly CbVal Value;

        public CbUnion(string tag, CbVal value)
        {
            Tag = tag;
            Value = value;
        }

        private string GetDebuggerDisplay() => $"({Tag}){Value}";
        public override string ToString() => $"({Tag}){Value}";
    }
}
