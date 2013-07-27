using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using com.robotacid.gfx;
using flash;
using Array = System.Array;

/// <summary>
/// Provides an interface for storing game data in a shared object and restoring the game from
/// the shared object
/// 
/// @author Aaron Steed, robotacid.com
/// @conversion Paul Cunningham, pumpkin-games.net
/// </summary>
public class UserData
{
    public class Settings {
        public Array<Keys> customKeys;
        public Boolean sfx;
        public Boolean music;
        public Boolean ascended;
        public Boolean checkView;
        public Boolean tapControls;
        public List<Boolean> completed;
        public int orientation;
        public object best;

        public Library.LevelData adventureData;
        public Library.LevelData puzzleData;
        public int puzzleLevel;

        public bool paletteLeft;
        public bool paletteRight;
    }

    public class GameState {
        public Object data;
        public Object previousData;
        public bool dead;
    }

    public static Game game;
    public static Renderer renderer;
		
    public static Settings settings;
    public static GameState gameState;
		
    public static byte[] settingsBytes;
    public static byte[] gameStateBytes;
		
    private static Action loadSettingsCallback;
		
    public static Boolean disabled = false;
		
    private static int i;

    public static void push(Boolean settingsOnly = false) {
        //if(disabled) return;
			
        //var sharedObject:SharedObject = SharedObject.getLocal("ending");
        //// SharedObject.data has a nasty habit of writing direct to the file
        //// even when you're not asking it to. So we offload into a ByteArray instead.
        //settingsBytes = new ByteArray();
        //settingsBytes.writeObject(settings);
        //sharedObject.data.settingsBytes = settingsBytes;
        //if(!settingsOnly){
        //    gameStateBytes = new ByteArray();
        //    gameStateBytes.writeObject(gameState);
        //    sharedObject.data.gameStateBytes = gameStateBytes;
        //}
        //settingsBytes = null;
        //gameStateBytes = null;
        //// wrapper to send users to manage their shared object settings if blocked
        //try{
        //    sharedObject.flush();
        //    sharedObject.close();
        //} catch(e:Error){
        //    navigateToURL(new URLRequest("http://www.macromedia.com/support/documentation/en/flashplayer/help/settings_manager03.html"));
        //}
    }

    public static void pull() {
        //TODO
    }

    public static void reset() {
        //TODO
		initSettings();
		initGameState();
		push();
    }

    /* Overwrites matching variable names with source to target */
	public static void overwrite(Array target, Array source) {
        //TODO
        //foreach(var key in source){
        //    target[key] = source[key];
        //}
	}
		
	/* This is populated on the fly by com.robotacid.level.Content */
	public static void initGameState() {
		
		gameState = new GameState {
			data = null,
			previousData = null
		};
			
	}
		
	public static void saveGameState() {
        //TODO
        //if(game.level != null){
        //    gameState.data = game.level.data.saveData();
        //    push();
        //}
	}
		
	/* Create the default settings object to initialise the game from */
	public static void initSettings() {
        settings = new Settings {
            customKeys = new Array<Keys> {Keys.W, Keys.S, Keys.A, Keys.D },
            sfx = true,
            music = true,
            ascended = false,
            checkView = false,
            tapControls = !Game.MOBILE,
            completed = new Array<Boolean>(new Boolean[Library.TOTAL_LEVELS]),
            orientation = 0,
            best = null
        };
        for(int i = 0; i < Library.TOTAL_LEVELS; i++){
            settings.completed[i] = false;
        }
	}
		
	/* Push settings data to the shared object */
	public static void saveSettings() {
	}
		
	/* Saves the settings to a file */
	public static void saveSettingsFile() {
		//settingsBytes = new ByteArray();
		//settingsBytes.writeObject(settings);
		//FileManager.save(settingsBytes, "settings.dat");
		//settingsBytes = null;
	}
		
	/* Loads settings and executes a callback when finished */
	public static void loadSettingsFile(Action callback = null) {
		//loadSettingsCallback = callback;
		//FileManager.load(loadSettingsFileComplete, null, [FileManager.DAT_FILTER]);
	}
	private static void loadSettingsFileComplete() {
		//settingsBytes = FileManager.data;
		//overwrite(settings, settingsBytes.readObject());
		//if(Boolean(loadSettingsCallback)) loadSettingsCallback();
		//loadSettingsCallback = null;
	}
}

