using System.Runtime.CompilerServices;

namespace IronSearch
{

    public static class UniformDouble
    {
        // Thread-local RNG state (no contention, no locks)
        private static readonly ThreadLocal<SplitMix64> _rng =
            new ThreadLocal<SplitMix64>(() =>
            {
                // Seed using time + thread id (simple but effective)
                ulong seed = (ulong)DateTime.UtcNow.Ticks
                           ^ (ulong)Thread.CurrentThread.ManagedThreadId * 0x9E3779B97F4A7C15UL;
                return new SplitMix64(seed);
            });

        // --- Public API ---
        public static double NextDouble(double A, double B)
        {
            if (!(A < B)) throw new ArgumentException("A must be < B");
            if (double.IsNaN(A) || double.IsNaN(B)) throw new ArgumentException();

            ulong a = ToOrdered(A);
            ulong b = ToOrdered(B);

            if (b - a <= 1)
                throw new ArgumentException("No representable doubles in (A, B)");

            var rng = _rng.Value!;

            ulong r = NextUInt64InRange(ref rng, a + 1, b - 1);

            _rng.Value = rng; // write back state

            return FromOrdered(r);
        }

        // --- RNG (SplitMix64) ---
        private struct SplitMix64
        {
            private ulong _state;

            public SplitMix64(ulong seed)
            {
                _state = seed;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ulong NextUInt64()
            {
                ulong z = (_state += 0x9E3779B97F4A7C15UL);
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        // --- Range sampling (unbiased) ---
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong NextUInt64InRange(ref SplitMix64 rng, ulong min, ulong max)
        {
            ulong range = max - min + 1;

            // Power-of-two fast path
            if ((range & (range - 1)) == 0)
                return min + (rng.NextUInt64() & (range - 1));

            ulong limit = ulong.MaxValue - (ulong.MaxValue % range);

            ulong r;
            do
            {
                r = rng.NextUInt64();
            } while (r >= limit);

            return min + (r % range);
        }

        // --- Double <-> ordered ulong mapping ---
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ToOrdered(double x)
        {
            ulong bits = (ulong)BitConverter.DoubleToInt64Bits(x);
            return (bits & (1UL << 63)) != 0
                ? ~bits
                : bits | (1UL << 63);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double FromOrdered(ulong o)
        {
            ulong bits = (o & (1UL << 63)) != 0
                ? o & ~(1UL << 63)
                : ~o;

            return BitConverter.Int64BitsToDouble((long)bits);
        }
    }
}
