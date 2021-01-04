using System;
using System.Runtime.CompilerServices;

namespace CbStyles.Cbon
{
    internal unsafe static class Utils
    {
        public const nuint one = 1u;
        public const nuint zero = 0u;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static (A?, B?) Split<A, B>(this (A, B)? v) where A : struct where B : struct => v switch
        {
            (A a, B b) => (a, b),
            _ => (null, null),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static nuint Min(this nuint a, nuint b) => b >= a ? a : b;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static T? Then<T>(this T? v, Func<T, T?> f) where T : struct => v == null ? v : f(v.Value);
    }
}
