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
    public class FoodClockFX {

        public static Renderer renderer;
		
        public BlitSprite blit;
        public BitmapData spriteSheet;
        public int food;
        public int maxFood;
        public int nearDeath;
        public uint deadCol;
        public uint col;
        public int compassIndex;
		
        private Point p;
		
        public const double NEAR_DEATH  = 0.4;
        public static Array<Point> compassPoints = new Array<Point> { new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point( -1, 0) };
        // the pixels that make up the @
        public static Array<Point> pixels = new Array<Point>() { 
            new Point(4, 4), new Point(4, 4), new Point(5, 4), new Point(6, 4), new Point(6, 3), new Point(6, 2), new Point(6, 1),
            new Point(6, 0), new Point(5, 0), new Point(4, 0), new Point(3, 0), new Point(2, 0), new Point(1, 0),
            new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3), new Point(0, 4), new Point(0, 5),
            new Point(0, 6), new Point(1, 6), new Point(2, 6), new Point(3, 6), new Point(4, 6), new Point(5, 6), new Point(6, 6)
        };
        // I really did just type that out instead of wasting cpu on an automata or bothering to write a nifty bit
        // of code that would auto-generate it, figuring it out would have taken just as long
        // what makes this worse is that I realised I'd counted the pixels backwards and had to swap it around
        // for the sanity of the code. Oh, you want to change the player's graphics? Have fun.
		
        public FoodClockFX(BlitSprite blit, uint deadCol) {
            this.blit = blit;
            spriteSheet = blit.spriteSheet;
            this.deadCol = deadCol;
            food = maxFood = pixels.length - 1;
            col = spriteSheet.getPixel32((int)blit.rect.x, (int)blit.rect.y);
        }
		
        public void setFood(double n, double total, double x, double y) {
            var ratio = maxFood / total;
            var value = System.Math.Ceiling(ratio * n);
            while(food != value){
                p = pixels[food];
                if(food < value){
                    spriteSheet.setPixel32((int)(p.x + blit.rect.x), (int)(p.y + blit.rect.y), col);
                    food++;
                } else if(food > value){
                    if((1.0 / maxFood) * food <= NEAR_DEATH){
                        renderer.addFX(x + p.x, y + p.y, renderer.debrisBlit, compassPoints[compassIndex], 0, true, true);
                        if(food == 1){
                            renderer.bitmapDebris(blit, (int)(x * Renderer.INV_SCALE), (int)(y * Renderer.INV_SCALE), 0);
                        }
                    }
                    spriteSheet.setPixel32((int)(p.x + blit.rect.x), (int)(p.y + blit.rect.y), deadCol);
                    compassIndex++;
                    if(compassIndex >= compassPoints.length) compassIndex = 0;
                    food--;
                }
            }
        }
    }
}
