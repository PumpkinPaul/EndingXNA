namespace flash.geom
{
    public class ColorTransform {

        Number redMultiplier;

        public int alphaMultiplier;

        public ColorTransform(double redMultiplier = 1.0, double greenMultiplier = 1.0, double blueMultiplier = 1.0, double alphaMultiplier = 1.0, double redOffset = 0.0, double greenOffset = 0.0, double blueOffset = 0.0, double alphaOffset = 0.0) {
            //TODO:
            this.redMultiplier = redMultiplier;

        }
    }
}
