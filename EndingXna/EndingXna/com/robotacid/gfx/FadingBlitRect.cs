using flash;
using BitmapData = flash.display.BitmapData;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.gfx
{
    /// <summary>
    /// A hacky method for fading squares of colour
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class FadingBlitRect : BlitRect {

        public Array<BitmapData> frames;

        public FadingBlitRect(int dx = 0, int dy = 0, int width = 1, int height = 1, int totalFrames = 1, uint col = 0xFF000000) : base(dx, dy, width, height, col) {
			frames = new Array<BitmapData>(new BitmapData[totalFrames]);
			this.totalFrames = totalFrames;
			int step = 255 / totalFrames;
			for(int i = 0; i < totalFrames; i++) {
				frames[i] = new BitmapData(width, height, true, (uint)(col - 0x01000000 * i * step));
			}
		}
		
		override public void render(BitmapData destination, int frame = 0)  {
			p.x = x + dx;
			p.y = y + dy;
			destination.copyPixels(frames[frame], rect, p, null, null, true);
		}
    }
}
