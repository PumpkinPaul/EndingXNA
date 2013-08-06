//#if WINDOWS_PHONE
//#define JSON 
//#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
#if JSON
using Newtonsoft.Json;
#endif
using flash;
using Point = flash.geom.Point;

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
    private const string _levelsFilename = "levels2.xml";
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

        //deserializedLevels = LoadXmlDoc();

        //var doc = LoadXmlDoc();

        //var sb = new StringBuilder();
        //using (var s = new StringWriter(sb)) {
        //    var x = new XmlSerializer(typeof(LevelData[]));
        //    x.Serialize(s, doc);
        //}





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

    private static LevelData[] LoadXmlDoc()
    {
        string data = null;
        using (var fs = TitleContainer.OpenStream("levels.xml")) {
            using (var sr = new StreamReader(fs))
                data = sr.ReadToEnd();
        }

        
        var lvls = new List<LevelData>();
        var doc = XDocument.Parse(data);

        foreach(var ld in  doc.Root.Elements("LevelData"))
        {
            if (ld.IsEmpty)
                break;

            var levelData = new LevelData();

            levelData.map = new Array<Array<int>>();

            var map = ld.Element("map");

            foreach(var aoi in map.Elements("ArrayOfInt")) 
            {
                var a = new Array<int>();
                foreach(var i in aoi.Elements("int")) 
                {
                    a.push(Convert.ToInt32(i.Value));
                }
                levelData.map.push(a);
            }

            levelData.playerDir = Convert.ToInt32(ld.Element("playerDir").Value);
            int x = Convert.ToInt32(ld.Element("player").Element("x").Value);
            int y = Convert.ToInt32(ld.Element("player").Element("y").Value);
            levelData.player = new Point(x, y);
            levelData.food = Convert.ToInt32(ld.Element("food").Value);
            levelData.endingDist = Convert.ToInt32(ld.Element("endingDist").Value);
            levelData.turns = Convert.ToInt32(ld.Element("turns").Value);

            lvls.Add(levelData);        
        }

        return lvls.ToArray();
    }   
}
