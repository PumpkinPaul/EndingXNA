using System;
using flash;

using BitmapData = flash.display.BitmapData;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.gfx
{
    /// <summary>
    /// UIManager Button object
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class BlitButton {

        public int id;
		public int targetId;
		
		public double x;
		public double y;
		public BlitRect blit;
		public Boolean over;
		public Boolean active;
		public Rectangle area;
		public Action callback;
		public Boolean visible;
		public int frame;
		public Action heldCallback;
		public int heldCount;
		public Boolean held;
		public Action releaseCallback;
		public Boolean focusLock;
		public Boolean feedCallbackToEvent;
		public Boolean silent;
		
		private Boolean states;
		
		public const int HELD_DELAY= 15;
		
		public BlitButton(double x, double y, BlitRect blit, Action callback, Rectangle area = null, Boolean states = true) {
			this.x = x;
			this.y = y;
			this.blit = blit;
			this.states = states;
			this.callback = callback;
			if(area == null) area = new Rectangle(0, 0, blit.width, blit.height);
			this.area = area;
			visible = true;
			active = false;
			focusLock = true;
		}
		
		public void render(BitmapData bitmapData) {
			blit.x = (int)x;
			blit.y = (int)y;
			if(states){
				frame = over ? 1 : 0;
				frame += active ? 2 : 0;
			}
			blit.render(bitmapData, frame);
		}
    }
}
