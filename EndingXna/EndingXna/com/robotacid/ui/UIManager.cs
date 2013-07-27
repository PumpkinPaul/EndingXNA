using System;
using com.robotacid.gfx;
using BlitClip = com.robotacid.gfx.BlitClip;
using BlitRect = com.robotacid.gfx.BlitRect;
using BitmapData = flash.display.BitmapData;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;
using flash;
using Array = flash.Array;
using Sprite = flash.display.Sprite;

namespace com.robotacid.ui
{
    /// <summary>
    /// Oversees our custom rendered buttons
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class UIManager {

        public Boolean active;
		public Array<BlitButton> buttons;
		public Array<Array<BlitButton>> buttonGroups;
		public int currentGroup;
		public Boolean clicked;
		public Boolean mouseLock;
		public Boolean mouseOver;
		public BlitButton lastButton;
		public Array<BlitButton> buttonsOver;
		public Boolean ignore;
		public Action selectSoundCallback;
		
		public static UIManager dialog;
		public static Action mousePressedCallback;
		
		private int i;
        BlitButton button;
		
		public UIManager(Boolean active = true) {
			this.active = active;
			buttons = new Array<BlitButton>();
			buttonsOver = new Array<BlitButton>();
			buttonGroups = new Array<Array<BlitButton>> { buttons };
			currentGroup = 0;
		}
		
		public static UIManager openDialog(Array<Point> buttonPoints, Array<BlitRect> buttonBlits, Array<Action> buttonCallbacks, Action selectSoundCallback) {
			dialog = new UIManager();
			dialog.selectSoundCallback = selectSoundCallback;
			int i; 
            Point point; 
            BlitRect blit; 
            Action f;
			for(i = 0; i < buttonPoints.length; i++){
				point = buttonPoints[i];
				blit = buttonBlits[i];
				f = buttonCallbacks[i];
				dialog.addButton(point.x, point.y, blit, f, null, f != null);
			}
			
			return dialog;
		}
		
		public static UIManager confirmDialog(Number x, Number y, BlitRect panel, Action okayCallback, BlitRect okayBlit, BlitRect cancelBlit, Action cancelCallback = null, Action selectSoundCallback = null) {
			dialog = new UIManager();
			dialog.selectSoundCallback = selectSoundCallback;
			cancelCallback = cancelCallback ?? closeDialog;
			int i;
            Point point;
            BlitRect blit;
            Action f;
			var buttonPoints = new Array<Point> { new Point(x, y), new Point(x + 3, y + 3), new Point(x + okayBlit.width + 4, y + 3) };
			var buttonBlits = new Array<BlitRect> { panel, okayBlit, cancelBlit };
			var buttonCallbacks = new Array<Action> { null, okayCallback, cancelCallback };
			for(i = 0; i < buttonPoints.length; i++){
				point = buttonPoints[i];
				blit = buttonBlits[i];
				f = buttonCallbacks[i];
				dialog.addButton(point.x, point.y, blit, f, null, f != null);
			}
			return dialog;
		}
		
		public static void closeDialog() {
			dialog = null;
		}
		
		public BlitButton addButton(Number x, Number y, BlitRect blit, Action callback = null, Rectangle area = null, Boolean states = true) {
			var button = new BlitButton(x, y, blit, callback, area, states);
			buttons.push(button);
			return button;
		}
		
		public void addExistingButton(BlitButton button) {
			buttons.push(button);
		}
		
		public void addGroup() {
			buttonGroups.push(new Array<BlitButton>());
		}
		
		public void changeGroup(int n) {
			currentGroup = n;
			buttons = buttonGroups[currentGroup];
			mouseLock = true;
		}
		
		public void update(Number mouseX, Number mouseY, Boolean mousePressed, Boolean mouseClick, Boolean mouseReleased = false) {
			buttonsOver.length = 0;
			if(!mousePressed){
				mouseLock = false;
				ignore = false;
			} else {
				if(ignore) return;
			}
			mouseOver = false;
			button = buttons[0];
			for(i = buttons.length - 1; i > -1; i--){
				button = buttons[i];
				if(
					button.visible &&
					mouseX >= button.x + button.area.x &&
					mouseY >= button.y + button.area.y &&
					mouseX < button.x + button.area.x + button.area.width &&
					mouseY < button.y + button.area.y + button.area.height
				){
					button.over = Game.MOBILE ? (mousePressed || mouseReleased) : true;
					if(button.over){
						mouseOver = true;
						if(button == lastButton) buttonsOver.unshift(button);
						else buttonsOver.push(button);
					}
					if(mouseClick){
						lastButton = button;
						if(button.callback != null){
							if(button.feedCallbackToEvent){
								mousePressedCallback = button.callback;
							} else {
								button.callback();
								if((selectSoundCallback != null) && !button.silent) selectSoundCallback();
							}
						}
						mouseLock = true;
						button.heldCount = BlitButton.HELD_DELAY;
						break;
					} else if(mousePressed){
						if(button == lastButton){
							if(button.heldCount > 0) button.heldCount--;
							else {
								button.held = true;
								if(button.heldCallback != null){
									button.heldCount = BlitButton.HELD_DELAY;
									button.heldCallback();
								}
							}
						} else {
							if(!button.focusLock){
								lastButton = button;
								if(button.callback != null){
									button.callback();
									if(selectSoundCallback != null && !button.silent) selectSoundCallback();
								}
							}
						}
					}
				} else {
					if(button.over){
						button.over = false;
						button.held = false;
					}
				}
				
			}
			if(mouseReleased){
				if(lastButton != null && lastButton.over && lastButton.releaseCallback != null){
					lastButton.releaseCallback();
					lastButton.over = false;
				}
			}
			if(mouseClick && !mouseOver) ignore = true;
		}
		
		public void setActive(Boolean value) {
			active = value;
			mouseLock = !active;
		}
		
		public void render(BitmapData bitmapData) {
			for(i = 0; i < buttons.length; i++){
				button = buttons[i];
				if(button.visible) button.render(bitmapData);
			}
		}
		
		/* Returns a list of x positions for given widths and gaps spread out from cx */
		public static Array<Number> distributeRects(Number cx, Number width, Number gap, int total) {
			var list = new Array<Number>();
			Number x = cx - (width * total + gap * (total - 1)) * 0.5;
			for(; total > 0; total--){
				list.push(x);
				x += width + gap;
			}
			return list;
		}
    }
}
