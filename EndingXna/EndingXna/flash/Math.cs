using System;

namespace flash
{
    /// <summary>
    /// Basic implementation of the Flash Math class.
    /// </summary>
    /// <remarks>Use .Net Math library for core functions.</remarks>
    public static class Math {
        
        private static readonly Random _random;

        static Math() {
            _random = new Random(DateTime.Now.TimeOfDay.Milliseconds);
        }

        /// <summary>
        /// Returns a pseudo-random number n, where 0 <= n < 1. The number returned is calculated in an undisclosed manner, and is "pseudo-random" because the calculation inevitably contains some element of non-randomness.
        /// </summary>
        /// <returns>A pseudo-random number.</returns>
        public static double random() {
            return _random.NextDouble();
        }
    }
}
