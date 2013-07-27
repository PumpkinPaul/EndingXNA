using System;
using flash;
using flash.display;
using flash.geom;
using Math = System.Math;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Controls the reference point of the canvas
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class CanvasCamera {

        public Renderer renderer;
		
		public Boolean dragScroll;
		public Boolean uiStopDrag;
		public Point canvas;
		public Number canvasX, canvasY;
		public Number lastCanvasX, lastCanvasY;
		public Rectangle mapRect;
		public Point targetPos;
		public Number vx, vy;
		public int count;
		public Point delayedTargetPos;
		public Number interpolation;
		
		private Number viewWidth;
		private Number viewHeight;
		
		public readonly static Number DEFAULT_INTERPOLATION = 0.2;
		public readonly static Number DRAG_SCROLL_INTERPOLATION = 0.9;
		
		public const int UP = 1;
		public const int RIGHT = 2;
		public const int DOWN = 4;
		public  const int LEFT = 8;
		
		public CanvasCamera(Point canvas, Renderer renderer) {
			this.canvas = canvas;
			this.renderer = renderer;
			targetPos = new Point();
			dragScroll = false;
			count = 0;
			vx = vy = 0;
			interpolation = DEFAULT_INTERPOLATION;
			viewWidth = Game.WIDTH;
			viewHeight = Game.HEIGHT;
			mapRect = new Rectangle(0, 0, 1000, 1000);
		}
		
		/* This sets where the screen will focus on - the coords are a point on the canvas you want centered on the map */
		public void setTarget(Number x, Number y) {
			targetPos.x = (int)(-x + viewWidth * 0.5);
			targetPos.y = (int)(-y + viewHeight * 0.5);
		}
		
		/* This sets where the screen will focus on - the coords are a point on the canvas you want centered on the map */
		public void displace(Number x, Number y) {
			targetPos.x -= x;
			targetPos.y -= y;
			canvasX -= x;
			canvasY -= y;
			lastCanvasX -= x;
			lastCanvasY -= y;
		}
		
		/* Set a target to scroll to after a given delay */
		public void setDelayedTarget(Number x, Number y, int delay) {
			delayedTargetPos = new Point(x, y);
			count = delay;
		}
		
		/* No interpolation - jump straight to the target */
		public void skipPan() {
			canvas.x = (int)targetPos.x;
			canvas.y = (int)targetPos.y;
			canvasX = lastCanvasX = canvas.x;
			canvasY = lastCanvasY = canvas.y;
			//back.y = canvas.y;
		}
		
		/* Get a target position to feed back to the Camera later */
		public Point getTarget(){
			return new Point( -targetPos.x + viewWidth * 0.5, -targetPos.y + viewHeight * 0.5);
		}
		
		public void main() {
			
			lastCanvasX = canvasX;
			lastCanvasY = canvasY;
			
			if(count > 0){
				count--;
				if(count <= 0) setTarget(delayedTargetPos.x, delayedTargetPos.y);
			}
			
			// update the canvas position
			if(dragScroll){
				if(renderer.game.mousePressed){
					if(!uiStopDrag){
						vx = renderer.game.mouseVx;
						vy = renderer.game.mouseVy;
					}
				} else {
					vx *= interpolation;
					vy *= interpolation;
				}
				if(canvasX + vx > Game.WIDTH * 0.5){
					canvasX = Game.WIDTH * 0.5;
					vx = 0;
				}
				if(canvasY + vy > Game.HEIGHT * 0.5){
					canvasY = Game.HEIGHT * 0.5;
					vy = 0;
				}
				if(canvasX + vx < -(mapRect.x + mapRect.width - Game.WIDTH * 0.5)){
					canvasX = -(mapRect.x + mapRect.width - Game.WIDTH * 0.5);
					vx = 0;
				}
				if(canvasY + vy < -(mapRect.y + mapRect.height - Game.HEIGHT * 0.5)){
					canvasY = -(mapRect.y + mapRect.height - Game.HEIGHT * 0.5);
					vy = 0;
				}
				uiStopDrag = false;
			} else {
				vx = (targetPos.x - canvasX) * interpolation;
				vy = (targetPos.y - canvasY) * interpolation;
			}
			canvasX += vx;
			canvasY += vy;
			//back.move(vx, vy);
			
			canvas.x = Math.Round((double)canvasX) - renderer.shakeOffset.x;
			canvas.y = Math.Round((double)canvasY) - renderer.shakeOffset.y;
		}
		
		public void toggleDragScroll() {
			dragScroll = !dragScroll;
			interpolation = dragScroll ? DRAG_SCROLL_INTERPOLATION : DEFAULT_INTERPOLATION;
			vx = vy = 0;
		}
    }
}
