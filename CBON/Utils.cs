using System;
using System.Runtime.CompilerServices;

namespace CbStyles.Cbon
{
    internal unsafe static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static nuint Min(this nuint a, nuint b) => b >= a ? a : b;
    }
}
