namespace flash.system
{
    /// <summary>
    /// Basic implementation of the flash Capabilities class.
    /// </summary>
    public static class Capabilities {
       
        public static double screenResolutionX { get { return XnaGame.Instance.GameWindow.Width; } }
        public static double screenResolutionY { get { return XnaGame.Instance.GameWindow.Height; } }
        
    }
}
