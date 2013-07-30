using flash.geom;

namespace flash.display
{
    public sealed class Graphics {
        public static Graphics Instance { get; private set; }
               
        static Graphics() {
            Instance =  new Graphics();
        }

        public void clear() {

        }

        public void beginBitmapFill(BitmapData bitmapData, object matrix = null) {
            //Cache the commands for later to replay in the drawing phase
            if (bitmapData.texture == null)
                return;

           XnaGame.Instance.FlashRenderer.CopyPixels(null, bitmapData, bitmapData.rect, Point.Zero);//, alphaBitmapData, alphaPoint, mergeAlpha);
        }
         
        public void drawRect(double x, double y, double width, double height) {
            //Cache the commands for later to replay in the drawing phase
        }
         
        public void endFill() {

        }
    }
}
