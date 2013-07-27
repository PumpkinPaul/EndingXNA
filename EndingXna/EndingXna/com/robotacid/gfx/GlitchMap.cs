using flash;
using BitmapData = flash.display.BitmapData;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Manages the glitch line effects on the view port
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class GlitchMap {

        public Array<Point> rows;
		public Array<Point> cols;
		
		private int i;
		private Point p;
		private Rectangle rect;
		private Point g;
		private int glitchIndex;
		
		public readonly static Number TOLERANCE = 0.2;
		public readonly static Array<int> GLITCH_STEPS = new Array<int> {0, 1, 2, 1, 4, 0, 2, 4, 0, 2, 1, 0, 4, 2, 1, 0, 2, 1, 4, 0, 2, 0, 1, 1, 0, 4, 1};
		
		public GlitchMap() {
			rows = new Array<Point>();
			cols = new Array<Point>();
			p = new Point();
			rect = new Rectangle();
			glitchIndex = 0;
		}
		
		public void update() {
			for(i = cols.length - 1; i > -1; i--){
				g = cols[i];
				if(g.y > TOLERANCE) g.y -= Math.random();
				else if(g.y < -TOLERANCE) g.y += Math.random();
				else cols.splice(i, 1);
			}
			for(i = rows.length - 1; i > -1; i--){
				g = rows[i];
				if(g.x > TOLERANCE) g.x -= Math.random();
				else if(g.x < -TOLERANCE) g.x += Math.random();
				else rows.splice(i, 1);
			}
		}
		
		public void addGlitchRows(int ya, int yb, int dir) {
			do{
				glitchIndex++;
				if(glitchIndex >= GLITCH_STEPS.length) glitchIndex = 0;
				rows.push(new Point(dir * GLITCH_STEPS[glitchIndex], ya));
				if(ya < yb) ya++;
				else if(ya > yb) ya--;
			} while(ya != yb);
		}
		
		public void pushRows(int ya, int yb, int dir) {
			do{
				glitchIndex++;
				if(glitchIndex >= GLITCH_STEPS.length) glitchIndex = 0;
				rows.push(new Point(dir, ya));
				if(ya < yb) ya++;
				else if(ya > yb) ya--;
			} while(ya != yb);
		}
		
		public void addGlitchCols(int xa, int xb, int dir) {
			do{
				glitchIndex++;
				if(glitchIndex >= GLITCH_STEPS.length) glitchIndex = 0;
				cols.push(new Point(xa, dir * GLITCH_STEPS[glitchIndex]));
				if(xa < xb) xa++;
				else if(xa > xb) xa--;
			} while(xa != xb);
		}
		
		public void pushCols(int xa, int xb, int dir) {
			do{
				glitchIndex++;
				if(glitchIndex >= GLITCH_STEPS.length) glitchIndex = 0;
				cols.push(new Point(xa, dir));
				if(xa < xb) xa++;
				else if(xa > xb) xa--;
			} while(xa != xb);
		}
		
		public void reset() {
			cols.length = rows.length = 0;
		}
		
		public void apply(BitmapData target, int offsetX = 0, int offsetY = 0) {
			rect.y = 0;
			rect.width = 1;
			rect.height = target.height;
			for(i = cols.length - 1; i > -1; i--){
				g = cols[i];
				rect.x = p.x = g.x + offsetX;
				p.y = g.y;
				target.copyPixels(target, rect, p);
			}
			rect.x = 0;
			rect.width = target.width;
			rect.height = 1;
			for(i = rows.length - 1; i > -1; i--){
				g = rows[i];
				rect.y = p.y = g.y + offsetY;
				p.x = g.x;
				target.copyPixels(target, rect, p);
			}
		}
    }
}
