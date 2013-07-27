using System;

namespace flash
{
    public struct Number : IEquatable<Number> {
        const int one = 1;
        public static readonly Number One = new Number(1);
        public static readonly Number Zero = new Number(1);

        static Number() {

        }

        private double _value;

        public Number(double value)
        {
            _value = value;
        }

        public Number(float value)
        {
            _value = value;
        }

        public Number(int value)
        {
            _value = value;
        }

        public static implicit operator Number(double value)
        {
            return new Number(value);
        }

        public static implicit operator Number(float value)
        {
            return new Number(value);
        }

        public static implicit operator Number(int value) {
            return new Number(value);    
        }

        public static implicit operator double(Number n) {
            return n._value;    
        }

        public static implicit operator float(Number n) {
            return (float)n._value;    
        }

        public static implicit operator int(Number n) {
            return (int)n._value;    
        }

        public static implicit operator uint(Number n) {
            return (uint)n._value;    
        }

        public static bool operator >(Number first, Number second)
        {
            return first._value > second._value;
        }

        public static bool operator <(Number first, Number second)
        {
            return first._value < second._value;
        }

        public static bool operator >(Number first, int second)
        {
            return first._value > second;
        }

        public static bool operator <(Number first, int second)
        {
            return first._value < second;
        }

        public static bool operator >(int first, Number second)
        {
            return first > second._value;
        }

        public static bool operator <(int first, Number second)
        {
            return first < second._value;
        }

        public static Number operator +(Number first, Number second)
        {
            return new Number(first._value + second._value);
        }

        public static Number operator *(Number first, double second)
        {
            return new Number(first._value * second);
        }

        public static Number operator /(Number first, double second)
        {
            return new Number(first._value / second);
        }

        public static Number operator -(Number first, Number second)
        {
            return new Number(first._value - second._value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override bool Equals(object obj)
        {        
            if (!(obj is Number))
                return false;

            var other = (Number)obj;
            return _value.Equals(other._value);
        }

        public bool Equals(Number other)
        {
            return _value.Equals(other._value);
        }

        public static bool operator==(Number lhs, Number rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(Number lhs, Number rhs)
        {
            return !(lhs == rhs);
        }
    }
}
