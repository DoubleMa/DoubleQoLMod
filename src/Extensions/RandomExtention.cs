using System;

namespace DoubleQoL.Extensions {

    public static class RandomExtention {

        public static int Between(this int x, int min, int max) => Math.Max(min, Math.Min(max, x));
    }
}