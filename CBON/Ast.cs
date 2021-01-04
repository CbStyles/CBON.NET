using System;
using System.Collections.Generic;

namespace CbStyles.Cbon
{
    [Serializable]
    public readonly struct CbVal : IEquatable<CbVal>
    {
        public CbType Type { get; }
        private readonly bool data;
        private readonly string? str;
        private readonly List<CbVal>? arr;
        private readonly Dictionary<string, CbVal>? obj;
        private readonly CbUnion? union;

        private CbVal(CbType tag)
        {
            Type = tag;
            data = default;
            str = null;
            arr = null;
            obj = null;
            union = null;
        }
        private CbVal(bool data)
        {
            Type = CbType.Bool;
            this.data = data;
            str = null;
            arr = null;
            obj = null;
            union = null;
        }
        private CbVal(CbType tag, string str)
        {
            Type = tag;
            data = default;
            this.str = str;
            arr = null;
            obj = null;
            union = null;
        }
        private CbVal(List<CbVal> arr)
        {
            Type = CbType.Arr;
            data = default;
            str = null;
            this.arr = arr;
            obj = null;
            union = null;
        }
        private CbVal(Dictionary<string, CbVal> obj)
        {
            Type = CbType.Obj;
            data = default;
            str = null;
            arr = null;
            this.obj = obj;
            union = null;
        }
        private CbVal(CbUnion union)
        {
            Type = CbType.Union;
            data = default;
            str = null;
            arr = null;
            obj = null;
            this.union = union;
        }

        public bool IsNull => Type == CbType.Null;
        public bool IsBool => Type == CbType.Bool;
        public bool IsNum => Type == CbType.Num;
        public bool IsHex => Type == CbType.Hex;
        public bool IsStr => Type == CbType.Str;
        public bool IsArr => Type == CbType.Arr;
        public bool IsObj => Type == CbType.Obj;
        public bool IsUnion => Type == CbType.Union;

        public bool Bool => IsBool ? data : throw new ArgumentException("union kind error");
        public string Num => IsNum ? str! : throw new ArgumentException("union kind error");
        public string Hex => IsHex ? str! : throw new ArgumentException("union kind error");
        public string Str => IsStr ? str! : throw new ArgumentException("union kind error");
        public List<CbVal> Arr => IsArr ? arr! : throw new ArgumentException("union kind error");
        public Dictionary<string, CbVal> Obj => IsObj ? obj! : throw new ArgumentException("union kind error");
        public CbUnion Union => IsUnion ? union! : throw new ArgumentException("union kind error");

        public bool? TryBool => IsBool ? data : null;
        public string? TryNum => IsNum ? str! : null;
        public string? TryHex => IsHex ? str! : null;
        public string? TryStr => IsStr ? str! : null;
        public List<CbVal>? TryArr => IsArr ? arr! : null;
        public Dictionary<string, CbVal>? TryObj => IsObj ? obj! : null;
        public CbUnion? TryUnion => IsUnion ? union! : null;

        public static CbVal NewNull() => new CbVal(CbType.Null);
        public static CbVal NewBool(bool val) => new CbVal(val);
        public static CbVal NewNum(string raw) => new CbVal(CbType.Num, raw.Replace("_", ""));
        public static CbVal NewHex(string raw) => new CbVal(CbType.Hex, raw);
        public static CbVal NewStr(string str) => new CbVal(CbType.Str, str);
        public static CbVal NewArr(List<CbVal> arr) => new CbVal(arr);
        public static CbVal NewObj(Dictionary<string, CbVal> obj) => new CbVal(obj);
        public static CbVal NewUnion(CbUnion union) => new CbVal(union);
        public static CbVal NewUnion(string tag, CbVal val) => new CbVal(new CbUnion(tag, val));

        public enum CbType
        {
            Null, Bool, Num, Hex, Str, Arr, Obj, Union
        }

        public override bool Equals(object? obj) => obj is CbVal val && Equals(val);

        public bool Equals(CbVal other) => Type == other.Type && Type switch
        {
            CbType.Null => true,
            CbType.Bool => data == other.data,
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
            CbType.Bool => HashCode.Combine(Type, data),
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
    }

    [Serializable]
    public record CbUnion
    {
        public readonly string Tag;
        public readonly CbVal Value;

        public CbUnion(string tag, CbVal value)
        {
            Tag = tag;
            Value = value;
        }
    }
}
