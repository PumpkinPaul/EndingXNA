using System;
using flash;
using Sprite = flash.display.Sprite;

namespace com.robotacid.ui
{
    /// <summary>
    /// A simple fade to segue between scenes with optional text inbetween
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class Transition : Sprite {

        public Action changeOverCallback;
		public Action completeCallback;
		public int dir;
		public int fadeIn;
		public int fadeOut;
		public int delayCount;
		
		private TextBox textBox;
		private int textCount;
		private double fadeInAlpha;
		private double fadeOutAlpha;
		private Boolean changeOverFrame;

        public readonly static double FADE_STEP = 1.0 / 10;
		public const int TEXT_DELAY = 60;
		
		public Transition() {
			dir = 0;
			alpha = 0;
			textBox = new TextBox(Game.WIDTH, Game.HEIGHT, 0xFF000000, 0xFF000000);
			textBox.alignVert = "center";
			textBox.align = "center";
			textBox.offsetY = -1;
			addChild(textBox);
			visible = false;
			changeOverFrame = false;
		}
		
		public void main() {
			if(delayCount > 0){
				delayCount--;
				return;
			}
			if(changeOverFrame){
				changeOverCallback();
				changeOverFrame = false;
			}
			// fade in text and delay
			if(alpha == 1 && textCount > 0){
				textCount--;
				if(textCount == 0) dir = -1;
			// fade in, callback, fade out, callback
			} else {
				if(dir > 0){
					alpha += fadeInAlpha;
					fadeIn--;
					if(alpha >= 1 || fadeIn <= 0){
						alpha = 1;
						if(textCount == 0) dir = -1;
						if(changeOverCallback != null) changeOverFrame = true;
					}
				} else if(dir < 0){
					alpha -= fadeOutAlpha;
					fadeOut--;
					if(alpha <= 0.0f || fadeOut <= 0.0f){
						dir = 0;
						alpha = 0;
						visible = false;
						if(completeCallback != null) completeCallback();
					}
				}
			}
		}
		
		/* Initiate a transition */
		public void begin(Action changeOverCallback, int fadeIn = 10, int fadeOut = 10, String text = "", int textDelay = 0, Action completeCallback = null, int delayCount = 0) {
		
            this.changeOverCallback = changeOverCallback;
			this.fadeIn = fadeIn;
			this.fadeOut = fadeOut;
			this.delayCount = delayCount;
			this.completeCallback = completeCallback;
			fadeInAlpha = fadeIn > 0 ? 1.0 / fadeIn : 1.0;
			fadeOutAlpha = fadeOut > 0 ? 1.0 / fadeOut : 1.0;
			textBox.text = text;
			textCount = textDelay;
			visible = true;
			dir = 1;
		}
    }
}
