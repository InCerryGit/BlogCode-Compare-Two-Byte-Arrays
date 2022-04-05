using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace CompareByte;

public static class BytesCompare
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool ForCompare(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.Length != y.Length) return false;

        for (var index = 0; index < x.Length; index++)
        {
            if (x[index] != y[index]) return false;
        }

        return true;
    }

    public static bool SequenceCompare(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.Length != y.Length) return false;

        return x.SequenceEqual(y);
    }

    public static bool SequenceEqualInner<T>(ref T first, ref T second, int length) where T : IEquatable<T>
    {
        if (Unsafe.AreSame(ref first, ref second))
            goto Equal;

        nint index = 0; // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations
        T lookUp0;
        T lookUp1;
        while (length >= 8)
        {
            length -= 8;

            lookUp0 = Unsafe.Add(ref first, index);
            lookUp1 = Unsafe.Add(ref second, index);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 1);
            lookUp1 = Unsafe.Add(ref second, index + 1);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 2);
            lookUp1 = Unsafe.Add(ref second, index + 2);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 3);
            lookUp1 = Unsafe.Add(ref second, index + 3);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 4);
            lookUp1 = Unsafe.Add(ref second, index + 4);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 5);
            lookUp1 = Unsafe.Add(ref second, index + 5);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 6);
            lookUp1 = Unsafe.Add(ref second, index + 6);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 7);
            lookUp1 = Unsafe.Add(ref second, index + 7);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;

            index += 8;
        }

        if (length >= 4)
        {
            length -= 4;

            lookUp0 = Unsafe.Add(ref first, index);
            lookUp1 = Unsafe.Add(ref second, index);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 1);
            lookUp1 = Unsafe.Add(ref second, index + 1);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 2);
            lookUp1 = Unsafe.Add(ref second, index + 2);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            lookUp0 = Unsafe.Add(ref first, index + 3);
            lookUp1 = Unsafe.Add(ref second, index + 3);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;

            index += 4;
        }

        while (length > 0)
        {
            lookUp0 = Unsafe.Add(ref first, index);
            lookUp1 = Unsafe.Add(ref second, index);
            if (!(lookUp0?.Equals(lookUp1) ?? (object?) lookUp1 is null))
                goto NotEqual;
            index += 1;
            length--;
        }

        Equal:
        return true;

        NotEqual: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return false;
    }

    public static unsafe bool Sse2Compare(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.Length != y.Length) return false;

        fixed (byte* xPtr = x, yPtr = y)
        {
            return Sse2CompareInternal(xPtr, yPtr, x.Length);
        }
    }

    private static unsafe bool Sse2CompareInternal(byte* xPtr, byte* yPtr, int length)
    {
        byte* lastAddr = xPtr + length;
        byte* lastAddrMinus64 = lastAddr - 64;
        const int mask = 0xFFFF;
        while (xPtr < lastAddrMinus64) // unroll the loop so that we are comparing 64 bytes at a time.
        {
            if (Sse2.MoveMask(Sse2.CompareEqual(Sse2.LoadVector128(xPtr), Sse2.LoadVector128(yPtr))) != mask)
            {
                return false;
            }

            if (Sse2.MoveMask(Sse2.CompareEqual(Sse2.LoadVector128(xPtr + 16), Sse2.LoadVector128(yPtr + 16))) != mask)
            {
                return false;
            }

            if (Sse2.MoveMask(Sse2.CompareEqual(Sse2.LoadVector128(xPtr + 32), Sse2.LoadVector128(yPtr + 32))) != mask)
            {
                return false;
            }

            if (Sse2.MoveMask(Sse2.CompareEqual(Sse2.LoadVector128(xPtr + 48), Sse2.LoadVector128(yPtr + 48))) != mask)
            {
                return false;
            }

            xPtr += 64;
            yPtr += 64;
        }

        while (xPtr < lastAddr)
        {
            if (*xPtr != *yPtr) return false;
            xPtr++;
            yPtr++;
        }

        return true;
    }

    public static unsafe bool Avx2Compare(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.Length != y.Length) return false;

        fixed (byte* xPtr = x, yPtr = y)
        {
            return Avx2CompareInternal(xPtr, yPtr, x.Length);
        }
    }

    private static unsafe bool Avx2CompareInternal(byte* xPtr, byte* yPtr, int length)
    {
        byte* lastAddr = xPtr + length;
        byte* lastAddrMinus128 = lastAddr - 128;
        const int mask = -1;
        while (xPtr < lastAddrMinus128) // unroll the loop so that we are comparing 128 bytes at a time.
        {
            if (Avx2.MoveMask(Avx2.CompareEqual(Avx.LoadVector256(xPtr), Avx.LoadVector256(yPtr))) != mask)
            {
                return false;
            }

            if (Avx2.MoveMask(Avx2.CompareEqual(Avx.LoadVector256(xPtr + 32), Avx.LoadVector256(yPtr + 32))) != mask)
            {
                return false;
            }

            if (Avx2.MoveMask(Avx2.CompareEqual(Avx.LoadVector256(xPtr + 64), Avx.LoadVector256(yPtr + 64))) != mask)
            {
                return false;
            }

            if (Avx2.MoveMask(Avx2.CompareEqual(Avx.LoadVector256(xPtr + 96), Avx.LoadVector256(yPtr + 96))) != mask)
            {
                return false;
            }

            xPtr += 128;
            yPtr += 128;
        }

        while (xPtr < lastAddr)
        {
            if (*xPtr != *yPtr) return false;
            xPtr++;
            yPtr++;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static unsafe bool UlongCompare(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.Length != y.Length) return false;

        fixed (byte* xPtr = x, yPtr = y)
        {
            return UlongCompareInternal(xPtr, yPtr, x.Length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static unsafe bool UlongCompareInternal(byte* xPtr, byte* yPtr, int length)
    {
        byte* lastAddr = xPtr + length;
        byte* lastAddrMinus32 = lastAddr - 32;
        while (xPtr < lastAddrMinus32) // unroll the loop so that we are comparing 32 bytes at a time.
        {
            if (*(ulong*) xPtr != *(ulong*) yPtr) return false;
            if (*(ulong*) (xPtr + 8) != *(ulong*) (yPtr + 8)) return false;
            if (*(ulong*) (xPtr + 16) != *(ulong*) (yPtr + 16)) return false;
            if (*(ulong*) (xPtr + 24) != *(ulong*) (yPtr + 24)) return false;
            xPtr += 32;
            yPtr += 32;
        }

        while (xPtr < lastAddr)
        {
            if (*xPtr != *yPtr) return false;
            xPtr++;
            yPtr++;
        }

        return true;
    }

    [DllImport("msvcrt.dll")]
    private static extern unsafe int memcmp(byte* b1, byte* b2, int count);

    public static unsafe bool MemcmpCompare(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        if (x.Length != y.Length) return false;

        fixed (byte* xPtr = x, yPtr = y)
        {
            return memcmp(xPtr, yPtr, x.Length) == 0;
        }
    }
}