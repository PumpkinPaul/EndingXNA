using flash;
using flash.display;
using flash.geom;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Renders an animation to a BitmapData using a series of references to animation frames
	/// on a sprite sheet
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class BlitClip : BlitSprite {

        public int frame;
		public Array<Rectangle> frames;
		
		public BlitClip(BitmapData spriteSheet, Array<Rectangle> frames, int dx = 0, int dy = 0) : base(spriteSheet, frames[0], dx, dy){

			this.frames = frames;
			totalFrames = frames.length;
			
			// collapse multiple references
			int i, j;
            Rectangle a, b;
			for(i = 0; i < frames.length; i++){
				for(j = i + 1; j < frames.length; j++){
					a = frames[i];
					b = frames[j];
					if(a != null && b != null && a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height){
						frames[j] = frames[i];
					}
				}
			}
		}
		
		/* Returns a a copy of this object, must be cast into a BlitClip */
		override public BlitRect clone() {
			var blit= new BlitClip(spriteSheet, frames.slice(), dx, dy);
			return blit;
		}
		override public void render(BitmapData destination, int frame = 0) {
			if(frames[frame] != null){
				p.x = x + dx;
				p.y = y + dy;
				destination.copyPixels(spriteSheet, frames[frame], p, null, null, true);
			}
		}
    }
}
