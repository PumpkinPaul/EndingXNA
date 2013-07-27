using System;
using System.IO;
using Newtonsoft.Json;
using flash;
using flash.geom;

/// <summary>
/// Initialises levels and manages requests for them
/// 
/// @author Aaron Steed, robotacid.com
/// @conversion Paul Cunningham, pumpkin-games.net
/// </summary>
/// <remarks>
/// Created a concrete class to aid JSON deserialization (also a simple Point class as the JSON lib got a wee bit confused when Point properties were Number.
/// </remarks>
public class Library
{
    public class Point
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    /// <summary>Represents the JSON level data.</summary>
    public class LevelData
    {
        public Array<Array<int>> map { get; set; }
        public int playerDir { get; set; }
        public Point player { get; set; }
        public int food { get; set; }
        public int endingDist { get; set; }
        public int turns { get; set; }
    }

    //[Embed(source = "levels.json", mimeType = "application/octet-stream")] public static var LevelsData:Class;
		
	public static Array<LevelData> levels;
	public static Action loadUserLevelsCallback;
	public static Action saveUserLevelsCallback;
		
	public static Array<LevelData> PERMANENT_LEVELS;
	public static Array<LevelData> USER_LEVELS;
	public static int maxLevel;
		
	public const int TOTAL_LEVELS = 100;   
    
    public static void initLevels() {
        var data = File.ReadAllText("levels.json");
        
        var levels = JsonConvert.DeserializeObject<LevelData[]>(data);
        
        if (PERMANENT_LEVELS == null)
            PERMANENT_LEVELS = new Array<LevelData>(levels);

        if (USER_LEVELS == null)
            USER_LEVELS = new Array<LevelData>(levels);

        //for(int i = 0; i < TOTAL_LEVELS; i++){
        //    if(PERMANENT_LEVELS[i] != null) PERMANENT_LEVELS[i] = null;
        //    if(USER_LEVELS[i] != null) USER_LEVELS[i] = null;
        //}
        if(loadUserLevelsCallback != null) loadUserLevelsCallback();
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
