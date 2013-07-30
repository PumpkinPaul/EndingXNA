using Microsoft.Xna.Framework.Graphics;
using BitmapData = flash.display.BitmapData;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Renders a section of a sprite sheet BitmapData to another BitmapData
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class BlitSprite : BlitRect {

        public BitmapData spriteSheet;
		
		public BlitSprite(BitmapData spriteSheet, Rectangle rect, int dx = 0, int dy = 0) : base(dx, dy, (int)rect.width, (int)rect.height) {
			this.spriteSheet = spriteSheet;
			this.rect = rect;
		}
		
		/* Returns a a copy of this object, must be cast into a BlitSprite */
		override public BlitRect clone() {
			var blit = new BlitSprite(spriteSheet, rect, dx, dy);
			return blit;
		}
		
		override public void render(BitmapData destination, int frame = 0) {
			p.x = x + dx;
			p.y = y + dy;
			destination.copyPixels(spriteSheet, rect, p, null, null, true);
		}
    }
}
