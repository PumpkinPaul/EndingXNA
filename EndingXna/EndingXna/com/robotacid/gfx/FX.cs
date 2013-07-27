using System;
using flash;
using flash.display;
using flash.geom;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Self managing BlitClip wrapper
	/// Accounts for Blit being projected onto tracking the viewport and
	/// self terminates after animation is complete
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class FX : Point {

        public static Game game;
		public static Renderer renderer;
		
		public BlitRect blit;
		public int frame;
		public Boolean active;
		public Point canvasPoint;
		public BitmapData bitmapData;
		public Point dir;
		public Boolean looped;
		public Boolean killOffScreen;
		
		public FX(Number x, Number y, BlitRect blit, BitmapData bitmapData, Point canvasPoint, Point dir = null, int delay = 0, Boolean looped = false, Boolean killOffScreen = true) : base(x, y) {
			
			this.blit = blit;
			this.bitmapData = bitmapData;
			this.canvasPoint = canvasPoint;
			this.dir = dir;
			this.looped = looped;
			this.killOffScreen = killOffScreen;
			frame = 0 - delay;
			active = true;
		}
		
		public void main() {
			if(frame > -1){
				blit.x = (canvasPoint.x) + x;
				blit.y = (canvasPoint.y) + y;
				// just trying to ease the collosal rendering requirements going on
				if(blit.x + blit.dx + blit.width >= 0 &&
					blit.y + blit.dy + blit.height >= 0 &&
					blit.x + blit.dx <= Game.WIDTH &&
					blit.y + blit.dy <= Game.HEIGHT){
					blit.render(bitmapData, frame++);
				} else {
					frame++;
				}
				if(frame == blit.totalFrames){
					if(looped) frame = 0;
					else active = false;
				}
			} else {
				frame++;
			}
			if(dir != null){
				x += dir.x;
				y += dir.y;
			}
			if(!renderer.onScreen(x, y, 5) && killOffScreen) active = false;
		}
    }
}
