using Number = System.Double;

namespace flash.geom
{
    public class Rectangle {
        public Number height;
        public Number width;
        public Number x;
        public Number y;

        public Rectangle() { }

        public Rectangle(Number x, Number y, Number width, Number height) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
         }

         public Rectangle clone() {
            return new Rectangle(x, y, width, height);
         }
    }
}
