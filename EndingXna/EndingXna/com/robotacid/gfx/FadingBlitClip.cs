using flash;
using flash.display;
using flash.geom;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Dirty hack for fading out a snapshot
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class FadingBlitClip : BlitClip {

        public Array<BitmapData> spriteSheets;
		
		public FadingBlitClip(BitmapData spriteSheet, int fadeFrames) : base(spriteSheet, new Array<Rectangle>(fadeFrames)) {
			rect = new Rectangle(0, 0, spriteSheet.width, spriteSheet.height);
			frames.Add(rect);
			spriteSheets.Add(spriteSheet);
			ColorTransform colorTransform = new ColorTransform();
			Number alphaStep = 1.0 / fadeFrames;
			BitmapData bitmapData;
			for(int i = 1; i < fadeFrames; i++){
				frames.push(new Rectangle(0, 0, spriteSheet.width, spriteSheet.height));
				colorTransform.alphaMultiplier -= alphaStep;
				bitmapData = spriteSheet.clone();
				bitmapData.colorTransform(rect, colorTransform);
				spriteSheets.push(bitmapData);
			}
		}
		
		override public void render(BitmapData destination, int frame = 0) {
			spriteSheet = spriteSheets[frame];
			base.render(destination, frame);
		}
    }
}
