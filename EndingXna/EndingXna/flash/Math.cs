using System;

namespace flash
{
    public static class Math {
        
        private static readonly Random _random;

        static Math() {
            _random = new Random(DateTime.Now.TimeOfDay.Milliseconds);
        }

        public static double random() {
            return _random.NextDouble();
        }
    }
}
