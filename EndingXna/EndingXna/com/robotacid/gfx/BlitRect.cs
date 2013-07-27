using BitmapData = flash.display.BitmapData;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Renders a solid rect of colour to a BitmapData
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class BlitRect {

        public int x, y, width, height;
		public int dx, dy;
		public Rectangle rect;
		public uint col;
		public int totalFrames;
		
		public static Point p = new Point();
		
		public BlitRect(int dx = 0, int dy = 0, int width = 1, int height = 1, uint col = 0xFF000000) {
			x = y = 0;
			this.dx = dx;
			this.dy = dy;
			this.width = width;
			this.height = height;
			this.col = col;
			totalFrames = 1;
			rect = new Rectangle(0, 0, width, height);
		}
		
		public virtual void render(BitmapData destination, int frame = 0) {
			rect.x = x + dx;
			rect.y = y + dy;
			destination.fillRect(rect, col);
		}
		
		/* Returns a a copy of this object */
		public virtual BlitRect clone() {
			var blit = new BlitRect();
			blit.x = x;
			blit.y = y;
			blit.dx = dx;
			blit.dy = dy;
			blit.width = width;
			blit.height = height;
			blit.rect = new Rectangle(0, 0, width, height);
			blit.col = col;
			return blit;
		}
    }
}
