using Renderer = com.robotacid.gfx.Renderer;

namespace com.robotacid.ui
{
    /// <summary>
    /// UIManager Button object
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class LevelButtonData {

        public static Game game;
		public static Renderer renderer;
		
		public int index;
		//public var roomData
		
		public LevelButtonData(int index) {
			this.index = index;
		}
    }
}
