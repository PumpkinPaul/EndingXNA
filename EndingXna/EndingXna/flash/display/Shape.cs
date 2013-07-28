using System;

namespace flash.display
{
    public class Shape : DisplayObject {
        public Graphics graphics { get ; private set; } 

        public Shape() {
            graphics = new Graphics();
        }
    }
}
