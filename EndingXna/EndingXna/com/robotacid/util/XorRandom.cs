using System;

using flash;

namespace com.robotacid.util
{
    /// <summary>
    /// XorShift random number generator
    ///
	/// Adapted from: http://www.calypso88.com/?p=524
	///
	/// I've inlined the algorithm repeatedly because it actually runs faster than Math.random()
	/// so we might as well keep the speed boost on all calls to this object
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class XorRandom
    {
        public const double MAX_RATIO = 1 / uint.MaxValue;
		public uint r;
		public uint seed;
		
		public XorRandom(uint seed = 0u) {
			
			//seed = 1195675104;
			
			if(seed != 0){
				r = seed;
			} else {
				r = seedFromDate();
			}
			this.seed = r;
		}
		
		/* Get a seed using a Date object */
		public static uint seedFromDate() {
			uint r = ((uint)(new DateTime().TimeOfDay.TotalMilliseconds) % uint.MaxValue);
			// once in a blue moon we can roll a zero from sourcing the seed from the Date
			if(r == 0) r = (uint)(new Random().Next() * MAX_RATIO);
			return r;
		}
		
		/* Returns a number from 0 - 1 */
		public double value() {
			r ^= r << 21;
			r ^= r >> 35; // >>> logical
			r ^= r << 4;
			return r * MAX_RATIO;
		}
		
		public double range(double n) {
			r ^= r << 21;
			r ^= r >> 35; // >>> logical
			r ^= r << 4;
			return r * MAX_RATIO * n;
		}
		
		public int rangeInt(double n) {
			r ^= r << 21;
			r ^= r >> 35; // >>> logical
			r ^= r << 4;
			return (int)(r * MAX_RATIO * n);
		}
		
		public Boolean coinFlip() {
			r ^= r << 21;
			r ^= r >> 35; // >>> logical
			r ^= r << 4;
			return r * MAX_RATIO < 0.5;
		}
		
		/* Advance the sequence by n steps */
		public void step(int n = 1) {
			while(n-- > 0){
				r ^= r << 21;
				r ^= r >> 35; // >>> logical
				r ^= r << 4;
			}
		}
    }
}
