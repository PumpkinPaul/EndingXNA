using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using com.robotacid.gfx;
using flash;
using Microsoft.Xna.Framework.Storage;
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

    private const string SettingsFilename = "settings.dat";
    private const string GameStateFilename = "gameState.dat";
    

    public static void push(Boolean settingsOnly = false) {
        if(disabled) 
            return;
		
        XnaGame.Instance.StorageManager.Save(settings, SettingsFilename);

        if (!settingsOnly)
            XnaGame.Instance.StorageManager.Save(gameState, GameStateFilename);	
    }

    public static void pull(StorageContainer storageContainer) {
        if(disabled) 
            return;

        var settingsNew = XnaGame.Instance.StorageManager.Load<Settings>(storageContainer, SettingsFilename);
        var gameStateNew = XnaGame.Instance.StorageManager.Load<GameState>(storageContainer, GameStateFilename);
			
        if (settingsNew != null)
            settings = settingsNew;

        if (gameStateNew != null)
            gameState = gameStateNew;
    }

    public static void reset() {
		initSettings();
		initGameState();
		push();
    }

    /* Overwrites matching variable names with source to target */
	public static void overwrite(ref object target, ref object source) {
        //CONVERSION - just blat it - maybe look to reflect over fields (maybe)
        target = source;
	}
		
	/* This is populated on the fly by com.robotacid.level.Content */
	public static void initGameState() {
		
		gameState = new GameState {
			data = null,
			previousData = null
		};
	}
		
	public static void saveGameState() {
        if(game.level != null){
            gameState.data = game.level.data.saveData();
            push();
        }
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
        XnaGame.Instance.StorageManager.Save(settings, SettingsFilename);
	}
		
	public static void loadSettingsFile(StorageContainer storageContainer) {
        settings = XnaGame.Instance.StorageManager.Load<Settings>(storageContainer, SettingsFilename);
	}
}

