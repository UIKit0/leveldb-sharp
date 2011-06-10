using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace leveldb
{
    public class Random
    {
        const UInt32 M = 2147483647U;    // 2^31-1
        const UInt64 A = 16807;  // bits 14, 8, 7, 5, 2, 1, 0

        UInt32 seed_;

        public Random(UInt32 s)
        {
            seed_ = s & 0x7fffffffu;
        }

        public UInt32 Next()
        {
            // We are computing
            //       seed_ = (seed_ * A) % M,    where M = 2^31-1
            //
            // seed_ must not be zero or M, or else all subsequent computed values
            // will be zero or M respectively.  For all other values, seed_ will end
            // up cycling through every number in [1,M-1]
            UInt64 product = seed_ * A;

            // Compute (product % M) using the fact that ((x << 31) % M) == x.
            seed_ = (UInt32)((product >> 31) + (product & M));
            // The first reduction may overflow by 1 bit, so we may need to
            // repeat.  mod == M is not possible; using > allows the faster
            // sign-bit-based test.
            if (seed_ > M) {
              seed_ -= M;
            }
            return seed_;
        }

        // Returns a uniformly distributed value in the range [0..n-1]
        // REQUIRES: n > 0
        public UInt32 Uniform(int n) { return Next() % (UInt32)n; }

        // Randomly returns true ~"1/n" of the time, and false otherwise.
        // REQUIRES: n > 0
        public bool OneIn(int n) { return (Next() % n) == 0; }

        // Skewed: pick "base" uniformly from range [0,max_log] and then
        // return "base" random bits.  The effect is to pick a number in the
        // range [0,2^max_log-1] with exponential bias towards smaller numbers.
        public UInt32 Skewed(int max_log)
        {
            return Uniform(1 << (int)Uniform(max_log + 1));
        }

    }
}
