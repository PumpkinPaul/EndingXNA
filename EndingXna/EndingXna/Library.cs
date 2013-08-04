using System;
using System.IO;
using Microsoft.Xna.Framework;
#if JSON
using Newtonsoft.Json;
#endif
using flash;

/// <summary>
/// Initialises levels and manages requests for them
/// 
/// @author Aaron Steed, robotacid.com
/// @conversion Paul Cunningham, pumpkin-games.net
/// </summary>
/// <remarks>
/// Created a concrete class to aid JSON deserialization.
/// </remarks>
public class Library
{
    /// <summary>Represents the JSON level data.</summary>
    public class LevelData
    {
        public Array<Array<int>> map { get; set; }
        public int playerDir { get; set; }
        public flash.geom.Point player { get; set; }
        public int food { get; set; }
        public int endingDist { get; set; }
        public int turns { get; set; }
    }
		
	public static Array<LevelData> levels;
	public static Action loadUserLevelsCallback;
	public static Action saveUserLevelsCallback;
		
	public static Array<LevelData> PERMANENT_LEVELS;
	public static Array<LevelData> USER_LEVELS;
	public static int maxLevel;
		
	public const int TOTAL_LEVELS = 100;  
    
    #if JSON
    private const string _levelsFilename = "levels.json";
    #else
    private const string _levelsFilename = "levels.xml";
    #endif
    
    public static void initLevels() {
        string data = null;
        using (var fs = TitleContainer.OpenStream(_levelsFilename)) {
            using (var sr = new StreamReader(fs))
                data = sr.ReadToEnd();
        }
        
        #if JSON
        var deserializedLevels = JsonConvert.DeserializeObject<LevelData[]>(data);
        #else
        var deserializedLevels = XnaGame.Instance.StorageManager.DeserializeObject<LevelData[]>(data);
        #endif

        if (PERMANENT_LEVELS == null)
            PERMANENT_LEVELS = new Array<LevelData>(deserializedLevels);

        if (USER_LEVELS == null)
            USER_LEVELS = new Array<LevelData>(deserializedLevels);

        if(loadUserLevelsCallback != null) 
            loadUserLevelsCallback();
	} 

    public static void setLevels(Boolean permanent = true) {
		if(permanent){
			levels = PERMANENT_LEVELS;
			maxLevel = getMaxLevel();
		} else {
			levels = USER_LEVELS;
			maxLevel = levels.length - 1;
		}
	}
		
	private static int getMaxLevel() {
		for(int i = 0; i < levels.length; i++){
			if(levels[i] == null) return i - 1;
		}
		return levels.length - 1;
	}
}
