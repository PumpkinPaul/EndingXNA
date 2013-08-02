using System;
using flash;
using Microsoft.Xna.Framework;
using Array = flash.Array;
using BitmapData = flash.display.BitmapData;
using Math = System.Math;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Renders a string of numbers from the current x,y position
	/// 
    /// Using setTargetValue and update rolls the numbers towards a target value
    /// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class NumberBlit : BlitSprite {

        public int spacing;
		public Array<double > drums;
		public int digits;
		public double  value;
		public double  target;
		public double  step;
		
		private int sourceY;
		private int stepY;
		
		// temps
		private int i;
		private String str;
		private double  remainder;
		private double  digit;
		private int valueI;
		
		public NumberBlit(BitmapData spriteSheet, Rectangle rect, int digits = 1, double step = 0.25, int dx = 0, int dy = 0, int spacing = 0, int stepY = 0) : base (spriteSheet, rect, dx, dy) {
			this.height = height;
			this.digits = digits;
			this.step = step;
			if(stepY > 0) this.stepY = stepY;
			else this.stepY = (int)rect.width;
			if(spacing > 0) this.spacing = spacing;
			else this.spacing = (int)rect.width;
			sourceY = (int)rect.y;
			drums = new Array<double >(new double [digits]);
			this.rect = new Rectangle(rect.x, rect.y, spacing, spacing + 1);
		}
		
		/* Rolls the number drums towards our target digits */
		public void update() {
			if(value != target){
				if(value < target) value += step;
				if(value > target) value -= step;
				if(Math.Abs((value - target)) < step){
					setValue((int)target);
					return;
				}
				int valueI = (int)value;
				remainder = value - valueI;
				str = valueI + "";
				if(digits > 0){
					while(str.Length < digits) str = "0" + str;
				}
				for(i = digits - 1; i > -1; i--){
					digit = (int)str[i] - '0';
					drums[i] = digit + remainder;
					if(digit != 9) remainder = 0;
				}
			}
		}
		
		public void setTargetValue(int n) {
			target = n;
		}
		
		public void setValue(int n) {
			value = target = n;
			str = n + "";
			if(digits > 0){
				while(str.Length < digits) str = "0" + str;
			}

			for(i = 0; i < digits; i++){
				drums[i] = (int)(str[i]) - (int)'0';
			}
		}
		
		public void renderNumbers(BitmapData bitmapData) {
			double  yTemp = y;
			Rectangle rectTemp = rect.clone();
			for(i = 0; i < digits; i++){
				rect.y = sourceY + drums[i] * stepY;
				render(bitmapData);
				x += spacing;
			}
			rect = rectTemp;
		}
    }
}
