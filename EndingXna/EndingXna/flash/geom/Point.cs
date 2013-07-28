﻿using Number = System.Double;

namespace flash.geom
{
    public class Point {
        public Number x;
        public Number y;

        public Point() {}

        public Point(Number x, Number y) {
            this.x = x;
            this.y = y;
        }

        public Point clone() {
            return new Point(x, y);
        }
    }
}