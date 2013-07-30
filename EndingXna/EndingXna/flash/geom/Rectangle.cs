namespace flash.geom
{
    public class Rectangle {
        public double height;
        public double width;
        public double x;
        public double y;

        public Rectangle() { }

        public Rectangle(double x, double y, double width, double height) {
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
