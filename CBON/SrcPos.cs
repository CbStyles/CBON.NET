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

}
