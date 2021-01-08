using System;
using System.Collections;
using System.Collections.Generic;

namespace CbStyles.Cbon.Unsafe
{
    internal unsafe readonly struct Sliced<T> : IEquatable<Sliced<T>>, IEnumerable<T> where T : unmanaged
    {
        private readonly T* ptr;
        private readonly nuint len;
        private readonly T* origin;

        public Sliced(T* ptr, nuint len) : this(ptr, len, ptr) { }
        private Sliced(T* ptr, nuint len, T* origin)
        {
            this.ptr = ptr;
            this.len = len;
            this.origin = origin;
        }

        public nuint Length => len;
        public bool IsEmpty => (void*)len == null;
        public T? this[nuint index] => index >= len ? null : ptr[index];
        public T? First => (void*)len == null ? null : ptr[0u];
        public T? Last => (void*)len == null ? null : ptr[len - (nuint)(void*)1u];
        public Sliced<T> Tail => Slice(1u);

        public T? Get(nuint index) => index >= len ? null : ptr[index];
        public T UncheckedGet(nuint index) => ptr[index];

        public nuint Offset => (nuint)(ptr - origin);
        public nuint RawIndex(nuint index) => Offset + index;

        public T* Ptr => ptr;
        public T* RawPtr => origin;

        /// <summary>
        /// <code>..</code>
        /// </summary>
        public Sliced<T> Slice() => this;
        /// <summary>
        /// <code>start..</code>
        /// </summary>
        public Sliced<T> Slice(nuint start) => start > len ? throw new ArgumentOutOfRangeException() 
            : new Sliced<T>(ptr + start, len - start, origin);
        /// <summary>
        /// <code>start..end</code>
        /// </summary>
        public Sliced<T> Slice(nuint start, nuint end) => start > end || end > len ? throw new ArgumentOutOfRangeException()
            : new Sliced<T>(ptr + start, end - start, origin);
        /// <summary>
        /// <code>..end</code>
        /// </summary>
        public Sliced<T> SliceTo(nuint end) => end > len ? throw new ArgumentOutOfRangeException()
            : new Sliced<T>(ptr, end, origin);

        public ReadOnlySpan<T> ToSpan() => new ReadOnlySpan<T>(ptr, (int)len);

        public T[] ToArray() => ToSpan().ToArray();

        public override string ToString() => ToSpan().ToString();

        #region Equals

        public override bool Equals(object? obj) => obj is Sliced<T> sliced && Equals(sliced);

        public bool Equals(Sliced<T> other) => ptr == other.ptr && len == other.len && origin == other.origin;

        public override int GetHashCode() => HashCode.Combine((nuint)ptr, len, (nuint)origin);

        public static bool operator ==(Sliced<T> left, Sliced<T> right) => left.ptr == right.ptr && left.len == right.len && left.origin == right.origin;

        public static bool operator !=(Sliced<T> left, Sliced<T> right) => !(left == right);

        #endregion

        #region Iterator

        public Iterator GetEnumerator() => new Iterator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Iterator : IEnumerator<T>
        {
            private readonly Sliced<T> slice;
            private nuint index;

            public Iterator(Sliced<T> slice)
            {
                this.slice = slice;
                index = 0u;
            }

            public T Current => slice.UncheckedGet(index);

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                var num = index + (nuint)(void*)1u;
                if (num < slice.len)
                {
                    index = num;
                    return true;
                }
                return false;
            }

            public void Reset() => index = (nuint)(void*)null;
        }

        #endregion
    }
}
