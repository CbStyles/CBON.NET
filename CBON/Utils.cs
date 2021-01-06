using System.Runtime.CompilerServices;

namespace CbStyles.Cbon
{
    internal unsafe static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Min(this nuint a, nuint b) => b >= a ? a : b;
    }
}
