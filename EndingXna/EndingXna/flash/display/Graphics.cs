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
        }
         
        public void drawRect(Number x, Number y, Number width, Number height) {
            //Cache the commands for later to replay in the drawing phase
        }
         
        public void endFill() {

        }
    }
}
