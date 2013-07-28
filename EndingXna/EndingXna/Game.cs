using System;

using com.robotacid.engine;
using com.robotacid.gfx;
using flash.display;
using flash.events;
using flash.system;
using Microsoft.Xna.Framework.Graphics;
using Renderer = com.robotacid.gfx.Renderer;
using FX = com.robotacid.gfx.FX;
//using PNGEncoder = com.robotacid.gfx.PNGEncoder;
using SoundManager = com.robotacid.sound.SoundManager;
using SoundQueue = com.robotacid.sound.SoundQueue;
using com.robotacid.ui.editor;
//using FileManager = com.robotacid.ui.FileManager;
//using ProgressBar = com.robotacid.ui.ProgressBar;
using TextBox = com.robotacid.ui.TextBox;
//using Key = com.robotacid.ui.Key;
using TitleMenu = com.robotacid.ui.TitleMenu;
using Transition = com.robotacid.ui.Transition;
using UIManager = com.robotacid.ui.UIManager;
using XorRandom = com.robotacid.util.XorRandom;

//import flash.display.Bitmap;
//import flash.display.BitmapData;
//import flash.display.Graphics;
//import flash.display.MovieClip;
//import flash.display.Shape;
using Sprite = flash.display.Sprite;
//import flash.display.StageQuality;
//import flash.events.Event;
//import flash.events.KeyboardEvent;
//import flash.events.MouseEvent;
//import flash.events.TouchEvent;
//import flash.external.ExternalInterface;
//import flash.geom.ColorTransform;
//import flash.geom.Point;
//import flash.geom.Rectangle;
//import flash.media.Sound;
//import flash.net.SharedObject;
//import flash.system.Capabilities;
//import flash.text.TextField;
//import flash.ui.Keyboard;
//import flash.ui.Mouse;
//import flash.utils.getTimer;

using flash;
using Microsoft.Xna.Framework;
using Array = flash.Array;
using Math = flash.Math;

/// <summary>
/// Ending
/// 
/// A roguelike puzzle game
/// 
/// This is a god-object singleton. 
/// 
/// @author Aaron Steed, robotacid.com
/// @conversion Paul Cunningham, pumpkin-games.net
/// </summary>
public class Game : Sprite
{
    //render.main() is a NOP now so override here to do the main rendering
    protected internal override void OnDraw(RenderTarget2D sceneRenderTarget, GameTime gameTime) {
        renderer.main(sceneRenderTarget);           
    }

    public const Boolean TEST_BED_INIT = false;
	public static Boolean MOBILE = false;
		
	public static Game game;
	public static Renderer renderer;
		
	// core engine objects
	public TitleMenu titleMenu;
	public Level level;
	public RoomPainter roomPainter;
	public SoundQueue soundQueue;
	public Transition transition;

    // ui
	public Sprite focusPrompt;

    // states
	public int state;
	public int focusPreviousState;
	public int frameCount;
	public Boolean fireButtonPressed;
	public int fireButtonPressedCount;
	public int keyPressedCount;
	public Boolean mousePressed;
	public int mousePressedCount;
	public int mouseReleasedCount;
	public Number mouseVx;
	public Number mouseVy;
	public int mouseSwipeSent;
	public int mouseSwipeCount;
	public int mouseSwipeDelay;
	private Number lastMouseX;
	private Number lastMouseY;
	private Number mouseDownX;
	private Number mouseDownY;
	public Boolean paused;
	public int shakeDirX;
	public int shakeDirY;
	public Boolean forceFocus = false;
	public int mouseCorner;
	public Boolean regainedFocus = false;
	public int currentLevel;
	public int currentLevelType;
	public Library.LevelData currentLevelObj;
	public Boolean editing;
	public Boolean modifiedLevel;
	public Number scaleRatio;
	public Number defaultStageRatio;
	public Number stageRatio;
	public Number realStageWidth;
	public Number realStageHeight;
	public int hideMouseFrames;
		
	public const int MOUSE_SWIPE_DELAY_DEFAULT = 4;
		
	// temp variables
	private int i;
	public static Point point = new Point();
		
	// CONSTANTS
		
	public readonly static Number SCALE = 8;
	public readonly static Number INV_SCALE = 1.0 / 8;
		
	public readonly static Number DEFAULT_SCREEN_RATIO = 480 / 320;
	public static Number SCREEN_RATIO;
		
	// states
	public const int GAME = 0;
	public const int MENU = 1;
	public const int DIALOG = 2;
	public const int SEGUE = 3;
	public const int UNFOCUSED = 4;
		
	public static Number WIDTH = 120;
	public static Number HEIGHT = 80;
		
	// game key properties
	public const int UP_KEY = 0;
	public const int DOWN_KEY = 1;
	public const int LEFT_KEY = 2;
	public const int RIGHT_KEY = 3;
		
	public const int MAX_LEVEL = 20;
		
	public const int TURN_FRAMES = 2;
	public const int HIDE_MOUSE_FRAMES = 60;
	public const int DEATH_FADE_COUNT = 10;
		
	public static Boolean fullscreenToggled;
	public static Boolean allowScriptAccess;
		
	public const int SOUND_DIST_MAX = 8;
	public readonly static Number INV_SOUND_DIST_MAX = 1.0 / SOUND_DIST_MAX;

    public Game() {
			
		visible = false;
			
		game = this;
		UserData.game = this;
		FX.game = this;
		Level.game = this;
		TitleMenu.game = this;
		RoomPainter.game = this;
		RoomPalette.game = this;
			
        //TODO
		// detect allowScriptAccess for tracking
        //allowScriptAccess = ExternalInterface.available;
        //if(allowScriptAccess){
        //    try{
        //        ExternalInterface.call("");
        //    } catch( Exception e){
        //        allowScriptAccess = false;
        //    }
        //}
			
		Library.initLevels();
			
		// init UserData
		UserData.initSettings();
		UserData.initGameState();
		UserData.pull();
		// check the game is alive
		if(UserData.gameState.dead) UserData.initGameState();
			
		state = TEST_BED_INIT ? GAME : MENU;
			
		// init sound
		SoundManager.init();
		soundQueue = new SoundQueue();
			
		if (stage != null) addedToStage();
		else addEventListener(Event.ADDED_TO_STAGE, addedToStage);
	}

    /* Determine scaling and extra rows/cols for different devices */
	private void setStageRatio() {
		if(MOBILE){
            
			realStageWidth = System.Math.Max(Capabilities.screenResolutionX, Capabilities.screenResolutionY);
			realStageHeight = System.Math.Min(Capabilities.screenResolutionX, Capabilities.screenResolutionY);
		} else {
			realStageWidth = stage.stageWidth;
			realStageHeight = stage.stageHeight;
		}
		stageRatio = realStageWidth / realStageHeight;
		defaultStageRatio = WIDTH / HEIGHT;
			
		// pad extra rows/cols to fill aspect ratio better
		if(stageRatio > defaultStageRatio){
			scaleRatio = realStageHeight / HEIGHT;
			while((WIDTH + SCALE) * scaleRatio < realStageWidth){
				WIDTH += SCALE;
			}
		} else {
			scaleRatio = realStageWidth / WIDTH;
			while((HEIGHT + SCALE) * scaleRatio < realStageHeight){
				HEIGHT += SCALE;
			}
		}
	}

    private void addedToStage(Event e = null) {
        removeEventListener(Event.ADDED_TO_STAGE, init);
			
        //TODO:
        //// KEYS INIT
        //if(!Key.initialized){
        //    Key.init(stage);
        //    Key.custom = UserData.settings.customKeys.slice();
        //    Key.hotKeyTotal = 10;
        //}
			
		// GRAPHICS INIT
			
		setStageRatio();
		language.trace("new width/height:", WIDTH + "/" + HEIGHT);
			
		renderer = new Renderer(this);
		renderer.init();
			
		transition = new Transition();
			
		x = (realStageWidth * 0.5 - WIDTH * scaleRatio * 0.5) >> 0;
		y = (realStageHeight * 0.5 - HEIGHT * scaleRatio * 0.5) >> 0;
			
		scaleX = scaleY = scaleRatio;
			
        //TODO:
		//stage.quality = StageQuality.LOW;
		visible = true;
			
		if(UserData.settings.orientation > 0){
			toggleOrientation();
		}
			
		// launch
		init();
	}

    private void init(Event e = null) {
        init();
    }

    /*  */
	private void init() {
			
		// GAME GFX AND UI INIT
		if(state == GAME || state == MENU){
			renderer.createRenderLayers(this);
		}
			
		addChild(transition);
			
		if(focusPrompt == null){
			focusPrompt = new Sprite();
			focusPrompt.addChild(screenText("click"));
			stage.addEventListener(Event.DEACTIVATE, onFocusLost);
			stage.addEventListener(Event.ACTIVATE, onFocus);
		}
			
		if(state == GAME || state == MENU){
			frameCount = 1;
			currentLevel = 0;
			currentLevelType = TEST_BED_INIT ? Room.ADVENTURE : Room.PUZZLE;
			currentLevelObj = null;
			titleMenu = new TitleMenu();
			if(state == GAME) initLevel();
		}
			
		addListeners();
			
		// this is a hack to force clicking on the game when the browser first pulls in the swf
		if(forceFocus){
			onFocusLost();
			forceFocus = false;
		} else {
		}
	}
		
	/* Pedantically clear all memory and re-init the project */
	public void reset(Boolean newGame = true) {
		removeEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
		removeEventListener(MouseEvent.MOUSE_UP, mouseUp);
		removeEventListener(Event.ENTER_FRAME, main);
		stage.removeEventListener(KeyboardEvent.KEY_DOWN, keyPressed);
		stage.removeEventListener(Event.DEACTIVATE, onFocusLost);
		stage.removeEventListener(Event.ACTIVATE, onFocus);
		stage.removeEventListener(MouseEvent.MOUSE_MOVE, mouseMove);
		//removeEventListener(TouchEvent.TOUCH_BEGIN, touchBegin);
		while(numChildren > 0){
			removeChildAt(0);
		}
		level = null;
		if(newGame){
			UserData.initGameState();
			UserData.push();
		}
		init();
	}
		
	private void addListeners() {
		stage.addEventListener(Event.DEACTIVATE, onFocusLost);
		stage.addEventListener(Event.ACTIVATE, onFocus);
		stage.addEventListener(MouseEvent.MOUSE_MOVE, mouseMove);
		stage.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
		stage.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
		stage.addEventListener(KeyboardEvent.KEY_DOWN, keyPressed);
		addEventListener(Event.ENTER_FRAME, main);
	}
		
	// =================================================================================================
	// MAIN LOOP
	// =================================================================================================
		
	private void main(Event e) {
			
		// copy out when needed
		// var t:int = getTimer();
			
		if(transition.visible) transition.main();
			
		if(state == GAME) {
				
			if(roomPainter.active) roomPainter.main();
			if(level.active && transition.dir < 1) level.main();
			soundQueue.play();
			renderer.main();
				
		} else if(state == MENU){
				
			if(transition.dir < 1) titleMenu.main();
			renderer.main();
				
		}
			
		// hide the mouse when not in use
		if(hideMouseFrames < HIDE_MOUSE_FRAMES){
			hideMouseFrames++;
			if(hideMouseFrames >= HIDE_MOUSE_FRAMES){
				Mouse.hide();
				if(!MOBILE){
					if(!(roomPainter != null && roomPainter.active)){
						if(level != null && level.settingsButton != null){
							level.settingsButton.visible = false;
						}
					}
				}
			}
		}
			
		mouseVx = mouseX - lastMouseX;
		mouseVy = mouseY - lastMouseY;
		lastMouseX = mouseX;
		lastMouseY = mouseY;

	    frameCount++;
	}

	public void initLevel() {
		level = new Level(currentLevelType, currentLevelObj);
		level.checkView = level.checkButton.active = UserData.settings.checkView;
		level.tapControls = level.controlsButton.active = UserData.settings.tapControls;
		renderer.reset();
		roomPainter = new RoomPainter(level, level.data);
		//roomPainter.setActive(true);
		state = GAME;
	}
		
	public void resetGame() {
		if(!editing){
			if(level.data.room.type == Room.ADVENTURE){
				UserData.settings.adventureData = null;
				currentLevelObj = null;
			} else if(level.data.room.type == Room.PUZZLE){
				UserData.settings.puzzleData = null;
				currentLevelObj = Library.levels[currentLevel];;
			}
			UserData.push(true);
		}
		initLevel();
	}
		
	public void setNextGame(int type, int levelNum = 0) {
		modifiedLevel = false;
		currentLevel = levelNum;
		currentLevelType = type;
		if(type == Room.ADVENTURE){
			currentLevelObj = null;
			if(UserData.settings.adventureData != null){
				currentLevelObj = UserData.settings.adventureData;
			}
		} else if(type == Room.PUZZLE){
			if(UserData.settings.puzzleData != null && !editing){
				currentLevelObj = UserData.settings.puzzleData;
				currentLevel = UserData.settings.puzzleLevel;
			} else {
				currentLevelObj = Library.levels[levelNum];
			}
		}
	}
		
	public void saveProgress(Boolean appExit = false) {
		if(!editing){
			if(state == GAME){
				if(level.data.room.type == Room.ADVENTURE){
					if(!level.data.ended){
						UserData.settings.adventureData = level.data.saveData(true);
					} else {
						UserData.settings.adventureData = null;
					}
				} else if(level.data.room.type == Room.PUZZLE){
					if(!level.data.ended && appExit){
						UserData.settings.puzzleData = level.data.saveData(true);
						UserData.settings.puzzleLevel = currentLevel;
					} else {
						UserData.settings.puzzleData = null;
					}
				}
				UserData.push(true);
			}
		}
	}
		
	public void puzzleWin() {
		if(!editing || !modifiedLevel){
			currentLevel++;
			currentLevelObj = currentLevel < Library.TOTAL_LEVELS ? Library.levels[currentLevel] : null;
		}
		if(currentLevelObj != null){
			String str = (currentLevel < 10 ? "0" : "") + currentLevel;
			transition.begin(nextLevel, DEATH_FADE_COUNT, DEATH_FADE_COUNT, str, 15, null, 20);
		} else {
			transition.begin(quit, DEATH_FADE_COUNT, DEATH_FADE_COUNT, "\"poo-tee-weet?\"", 90, null, 30);
		}
	}
		
	public void adventureWin() {
		var str = "" + level.data.turns;
		transition.begin(quit, DEATH_FADE_COUNT, DEATH_FADE_COUNT, str, 30, null, 90);
	}
		
	public void blackOut() {
		renderer.reset();
		state = SEGUE;
	}
		
	public void nextLevel() {
		if(!editing) completePreviousLevel();
		initLevel();
	}
		
	private void completePreviousLevel() {
		if(!editing){
			if(level.data.room.type == Room.PUZZLE){
				UserData.settings.completed[currentLevel - 1] = true;
				UserData.settings.puzzleData = null;
				UserData.push(true);
			} else if(level.data.room.type == Room.ADVENTURE){
				if(UserData.settings.best != null || level.data.turns < (int)UserData.settings.best) {
					UserData.settings.best = level.data.turns;
					UserData.settings.adventureData = null;
					UserData.push(true);
					titleMenu.setScore(level.data.turns);
				}
			}
		}
	}
		
	public void death() {
		transition.begin(resetGame, DEATH_FADE_COUNT, DEATH_FADE_COUNT, "@", 5, null, 15);
	}
		
	public void quit() {
		if(level.active) completePreviousLevel();
		else saveProgress();
			
		if(roomPainter.active) roomPainter.setActive(false);
		renderer.reset();
		state = MENU;
	}
		
	public TextBox screenText(String str) {
		var textBox = new TextBox(WIDTH, HEIGHT, 0xFF000000, 0xFF000000);
		textBox.align = "center";
		textBox.alignVert = "center";
		textBox.text = str;
		return textBox;
	}
		
	/* Play a sound at a volume based on the distance to the player */
	public void createDistSound(int mapX, int mapY, String name, Array<string> names = null, double volume = 1.0) {
		Number dist = Math.abs(level.data.player.x - mapX) + Math.abs(level.data.player.y - mapY);
		if(dist < SOUND_DIST_MAX){
			if(names != null) soundQueue.addRandom(name, names, (SOUND_DIST_MAX - dist) * INV_SOUND_DIST_MAX * volume);
			else if(name != null) soundQueue.add(name, (SOUND_DIST_MAX - dist) * INV_SOUND_DIST_MAX * volume);
		}
	}
		
	private void mouseDown(MouseEvent e = null) {
		// ignore the first click returning to the game
		if(!MOBILE){
			if(regainedFocus){
				regainedFocus = false;
				return;
			}
		}
		mouseSwipeSent = 0;
		mousePressed = true;
		mousePressedCount = frameCount;
		lastMouseX = mouseDownX = mouseX;
		lastMouseY = mouseDownY = mouseY;
		setMouseCorner();
	}
		
	private void mouseUp(MouseEvent e = null) {
		mousePressed = false;
		mouseReleasedCount = frameCount;
		// this is how you bypass security restrictions on method calls that require a MouseEvent to validate
		if(UIManager.mousePressedCallback != null){
			UIManager.mousePressedCallback();
			UIManager.mousePressedCallback = null;
		}
	}
		
	public int getMouseSwipe() {
		if(mouseSwipeSent == 0){
			Number vx = mouseX - mouseDownX;
			Number vy = mouseY - mouseDownY;
			Number len = System.Math.Sqrt(vx * vx + vy * vy);
			if(len > 3){
				mouseSwipeCount = mouseSwipeDelay = MOUSE_SWIPE_DELAY_DEFAULT;
				if(Math.abs(vy) > Math.abs(vx)){
					if(vy > 0){
						mouseSwipeSent = Room.DOWN;
					} else {
						mouseSwipeSent = Room.UP;
					}
				} else {
					if(vx > 0){
						mouseSwipeSent = Room.RIGHT;
					} else {
						mouseSwipeSent = Room.LEFT;
					}
				}
				return mouseSwipeSent;
			}
		} else {
			if(mouseSwipeCount > 0){
				mouseSwipeCount--;
			} else {
				if(mouseSwipeDelay > 0){
					mouseSwipeDelay--;
				}
				mouseSwipeCount = mouseSwipeDelay;
				return mouseSwipeSent;
			}
		}
		return 0;
	}
		
	private void mouseMove(MouseEvent e) {
		if(hideMouseFrames >= HIDE_MOUSE_FRAMES){
			Mouse.show();
			if(!MOBILE){
				if(level != null && level.settingsButton != null){
					level.settingsButton.visible = true;
				}
			}
		}
		hideMouseFrames = 0;
		setMouseCorner();
	}
		
	private void setMouseCorner() {
		//var xSlope:Number = (WIDTH / HEIGHT) * mouseY;
		//var ySlope:Number = HEIGHT - (HEIGHT / WIDTH) * mouseX;
		Number x = mouseX - (WIDTH - HEIGHT) * 0.5;
		Number y = mouseY;
		Number xSlope = y;
		Number ySlope = HEIGHT - x;
		if(x > xSlope && y > ySlope){
			mouseCorner = Room.RIGHT;
		} else if(x > xSlope && y < ySlope){
			mouseCorner = Room.UP;
		} else if(x < xSlope && y > ySlope){
			mouseCorner = Room.DOWN;
		} else if(x < xSlope && y < ySlope){
			mouseCorner = Room.LEFT;
		}
		if(Math.abs(mouseX - WIDTH * 0.5) < SCALE * 0.75 && Math.abs(mouseY - HEIGHT * 0.5) < SCALE * 0.75){
			mouseCorner = 0;
		}
	}
		
	public void selectSound() {
		SoundManager.playSound("chirup");
	}
		
	public void toggleOrientation() {
		if(scaleY > 0){
			scaleY = -scaleY;
			scaleX = -scaleX;
			UserData.settings.orientation = -1;
			x = (realStageWidth * 0.5 + WIDTH * scaleRatio * 0.5) >> 0;
			y = (realStageHeight * 0.5 + HEIGHT * scaleRatio * 0.5) >> 0;
		} else {
			scaleY = -scaleY;
			scaleX = -scaleX;
			UserData.settings.orientation = 0;
			x = (realStageWidth * 0.5 - WIDTH * scaleRatio * 0.5) >> 0;
			y = (realStageHeight * 0.5 - HEIGHT * scaleRatio * 0.5) >> 0;
		}
		UserData.push(true);
	}
		
	private void keyPressed(KeyboardEvent e) {
		keyPressedCount = frameCount;
		if(Key.lockOut) return;
		if(level != null){
			if(Key.isDown(Key.R)){
				initLevel();
			}
			//if(Key.isDown(Key.T)){
				//LevelData.printPathMap();
			//}
			if(
				Key.isDown(Keyboard.CONTROL) ||
				Key.isDown(Keyboard.SHIFT) ||
				Key.isDown(Keyboard.CONTROL)
			){
				level.toggleCheckView();
			}
		}

        //TODO:
        //if(Key.isDown(Key.P)){
        //    //var tempBitmap:BitmapData = new BitmapData(WIDTH * scaleX, HEIGHT * scaleY, true, 0x0);
        //    var tempBitmap = new BitmapData(stage.stageWidth, stage.stageHeight, true, 0x0);
        //    tempBitmap.draw(this, transform.matrix);
        //    FileManager.save(PNGEncoder.encode(tempBitmap), "screenshot.png");
        //}

		/**/
		/*
		if(Key.isDown(Key.NUMBER_1)){
			SoundManager.playSound("door1");
		}
		if(Key.isDown(Key.NUMBER_2)){
			SoundManager.playSound("door2");
		}
		if(Key.isDown(Key.NUMBER_3)){
			SoundManager.playSound("door3");
		}
		if(Key.isDown(Key.NUMBER_4)){
			SoundManager.playSound("door4");
		}*/
	}
		
	/* Must be called through a mouse event to fire in a browser */
	public void toggleFullscreen() {
		if(stage.displayState == "normal"){
			try{
				stage.displayState = "fullScreen";
				stage.scaleMode = "showAll";
			} catch(Exception e){
			}
		} else {
			stage.displayState = "normal";
		}
		fullscreenToggled = true;
	}
		
	public void toggleSound() {
		SoundManager.active = !SoundManager.active;
		UserData.settings.sfx = SoundManager.active;
		UserData.push(true);
	}
		
	/* When the flash object loses focus we put up a splash screen to encourage players to click to play */
	private void onFocusLost(Event e = null) {
		if(MOBILE || state == UNFOCUSED || state == MENU) return;
		focusPreviousState = state;
		state = UNFOCUSED;
		Key.clearKeys();
		addChild(focusPrompt);
	}
		
	/* When focus returns we remove the splash screen -
		* 
		* WARNING: Activating fullscreen mode causes this method to be fired twice by the Flash Player
		* for some unknown reason.
		* 
		* Any modification to this method should take this into account and protect against repeat calls
		*/
	private void onFocus(Event e = null) {
		if(focusPrompt.parent != null) focusPrompt.parent.removeChild(focusPrompt);
		if(state == UNFOCUSED) state = focusPreviousState;
		if(state != MENU && !fullscreenToggled) regainedFocus = true;
		if(fullscreenToggled) fullscreenToggled = false;
	}
}
