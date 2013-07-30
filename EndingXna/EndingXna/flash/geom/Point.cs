using Number = System.Double;

namespace flash.geom
{
    public class Point {
        public static readonly Point Zero = new Point(0, 0);
        public static readonly Point One = new Point(1, 1);

        public double x;
        public double y;

        public Point() {}

        public Point(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public Point clone() {
            return new Point(x, y);
        }
    }
}
