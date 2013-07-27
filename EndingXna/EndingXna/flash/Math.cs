using System;

namespace flash
{
    public static class Math {
        
        private static Random _random;

        static Math() {
            _random = new Random(DateTime.Now.TimeOfDay.Milliseconds);
        }

        public static Number random() {
            return new Number(_random.NextDouble());
        }

        public static Number abs(Number value) {
            return System.Math.Abs((double)value);
        }
    }
}
