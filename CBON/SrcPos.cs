using System;
using System.Collections;
using System.Diagnostics;

namespace CbStyles.Cbon.SrcPos
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct Pos : IEquatable<Pos>, IStructuralEquatable, IComparable<Pos>, IComparable, IStructuralComparable
    {
        public readonly nuint line;
        public readonly nuint column;

        public Pos(nuint line, nuint column)
        {
            this.line = line;
            this.column = column;
        }

        public override string ToString() => $"{line}:{column}";

        private string GetDebuggerDisplay() => $"{{ Pos {ToString()} }}";

        public Pos AddOne() => new Pos(line + 1, column + 1);

        #region Eq

        public override bool Equals(object? obj) => obj is Pos pos && Equals(pos);
        public bool Equals(Pos other) => line == other.line && column == other.column;

        public bool Equals(object? other, IEqualityComparer comparer) => other is Pos pos && Equals(pos);

        public override int GetHashCode() => HashCode.Combine(line, column);

        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        public static bool operator ==(Pos left, Pos right) => left.Equals(right);
        public static bool operator !=(Pos left, Pos right) => !(left == right);

        #endregion

        #region Cmp

        public int CompareTo(Pos other)
        {
            if (line > other.line) return 1;
            if (line < other.line) return -1;
            if (column > other.column) return 1;
            if (column < other.column) return -1;
            return 0;
        }

        public int CompareTo(object? obj) => obj is Pos pos ? CompareTo(pos) : -1;

        public int CompareTo(object? other, IComparer comparer) => other is Pos pos ? CompareTo(pos) : -1;

        public static bool operator >(Pos left, Pos right)
        {
            if (left.line > right.line) return true;
            if (left.line < right.line) return false;
            return left.column > right.column;
        }

        public static bool operator <(Pos left, Pos right)
        {
            if (left.line < right.line) return true;
            if (left.line > right.line) return false;
            return left.column < right.column;
        }

        public static bool operator >=(Pos left, Pos right)
        {
            if (left.line > right.line) return true;
            if (left.line < right.line) return false;
            return left.column >= right.column;
        }

        public static bool operator <=(Pos left, Pos right)
        {
            if (left.line < right.line) return true;
            if (left.line > right.line) return false;
            return left.column <= right.column;
        }

        #endregion
    }

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct Loc : IEquatable<Loc>, IStructuralEquatable, IComparable<Loc>, IComparable, IStructuralComparable
    {
        public readonly Pos from;
        public readonly Pos to;

        public Loc(Pos from, Pos to)
        {
            Debug.Assert(from <= to);
            this.from = from;
            this.to = to;
        }
        public Loc(Pos at) : this(at, at) { }

        public override string ToString() => $"{from.line}:{from.column} .. {to.line}:{to.column}";

        private string GetDebuggerDisplay() => $"{{ Loc {ToString()} }}";

        #region Eq

        public override bool Equals(object? obj) => obj is Loc loc && Equals(loc);

        public bool Equals(object? other, IEqualityComparer comparer) => other is Loc loc && Equals(loc);

        public bool Equals(Loc other) => from.Equals(other.from) && to.Equals(other.to);
        public override int GetHashCode() => HashCode.Combine(from, to);

        public int GetHashCode(IEqualityComparer comparer) => GetHashCode();
        public static bool operator ==(Loc left, Loc right) => left.Equals(right);
        public static bool operator !=(Loc left, Loc right) => !(left == right);

        #endregion

        #region Cmp

        public int CompareTo(Loc other)
        {
            if (from == to) return 0;
            if (from >= other.from && to >= other.to) return 1;
            if (from <= other.from && to <= other.to) return -1;
            if (from >= other.from && to <= other.to) return -1;
            if (from <= other.from && to >= other.to) return 1;
            return 0;
        }

        public int CompareTo(object? obj) => obj is Loc loc ? CompareTo(loc) : -1;

        public int CompareTo(object? other, IComparer comparer) => other is Loc loc ? CompareTo(loc) : -1;

        #endregion
    }
}
