namespace flash.display
{
    public class Stage : DisplayObjectContainer {
        public int stageWidth { get { return XnaGame.Instance.Window.ClientBounds.Width; } }
        public int stageHeight { get { return XnaGame.Instance.Window.ClientBounds.Height; } }

        public string displayState { get; set; }
        public string scaleMode { get; set; }
    }
}
