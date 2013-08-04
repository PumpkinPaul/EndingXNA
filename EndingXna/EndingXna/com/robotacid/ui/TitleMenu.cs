using System;
using com.robotacid.gfx;
using flash;
#if JSON
using Newtonsoft.Json;
#endif
using Array = flash.Array;
using Level = com.robotacid.engine.Level;
using LevelData = com.robotacid.engine.LevelData;
using Room = com.robotacid.engine.Room;
using Renderer = com.robotacid.gfx.Renderer;
using BitmapData = flash.display.BitmapData;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.ui
{
    /// <summary>
    /// The entry point into the game
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class TitleMenu {

        public static Game game;
		public static Renderer renderer;
		
		public UIManager uiManager;
		public TextBox scoreTextBox;
		
		private int i, r, c;
		private BlitButton puzzleButton;
		private BlitButton adventureButton;
		private BlitButton editorButton;
		private BlitButton leftButton;
		private BlitButton rightButton;
		private BlitButton backButton;
		private BlitButton saveToDesktopButton;
		private BlitButton loadFromDesktopButton;
		private Array<BlitButton> levelButtons/*BlitButton*/;
		private Rectangle levelButtonRect;
		private int firstLevel;
		private BlitButton buttonInHand;
		private double prevButtonX;
		private double prevButtonY;
		private Library.LevelData sourceLevel;
		private Library.LevelData targetLevel;
		private int sourceId;
		private int targetId;
		private Array<Array<int>> mapCopyBuffer;
		
		public const int ROOT = 0;
		public const int PUZZLE_LEVELS = 1;
		public const int ADVENTURE = 2;
		public const int EDITOR_LEVELS = 3;
		
		public readonly static double BUTTON_HEIGHT = 12;
		
		public const int LEVEL_BUTTONS_WIDE = 5;
		public const int  LEVEL_BUTTONS_HIGH = 4;
		public const int  PAGE_LEVELS = LEVEL_BUTTONS_WIDE * LEVEL_BUTTONS_HIGH;
		public const int  LEVEL_BUTTON_GAP = 2;

        #if JSON
        private const string _userLevelsFilename = "user_levels.json";
        #else
        private const string _userLevelsFilename = "user_levels.xml";
        #endif

        public TitleMenu() {
			uiManager = new UIManager();
			uiManager.selectSoundCallback = game.selectSound;
			levelButtonRect = new Rectangle(0, 0, renderer.numberButtonBlit.width - 1, renderer.numberButtonBlit.height - 1);
			
			mapCopyBuffer = Room.create2DArray(Level.ROOM_WIDTH, Level.ROOM_HEIGHT);
			
			// puzzle, adventure, editor
			puzzleButton = uiManager.addButton(Game.WIDTH * 0.5 - renderer.puzzleButtonBlit.width * 2, Game.HEIGHT * 0.5 - renderer.puzzleButtonBlit.height * 0.5, renderer.puzzleButtonBlit, puzzlePressed);
			adventureButton = uiManager.addButton(Game.WIDTH * 0.5 - renderer.adventureButtonBlit.width * 0.5, Game.HEIGHT * 0.5 - renderer.adventureButtonBlit.height * 0.5, renderer.adventureButtonBlit, adventurePressed);
			editorButton = uiManager.addButton(Game.WIDTH * 0.5 + renderer.editorButtonBlit.width, Game.HEIGHT * 0.5 - renderer.editorButtonBlit.height * 0.5, renderer.editorButtonBlit, puzzlePressed);
			
			// level button grid
			uiManager.addGroup();
			uiManager.changeGroup(1);
			levelButtons = new Array<BlitButton>();
			double buttonX = Game.WIDTH * 0.5 - ((LEVEL_BUTTONS_WIDE - 1) * LEVEL_BUTTON_GAP + levelButtonRect.width * LEVEL_BUTTONS_WIDE) * 0.5;
			double buttonY = Game.HEIGHT * 0.5 - ((LEVEL_BUTTONS_HIGH - 1) * LEVEL_BUTTON_GAP + levelButtonRect.height * LEVEL_BUTTONS_HIGH) * 0.5;
			BlitButton levelButton;
			for(r = 0; r < LEVEL_BUTTONS_HIGH; r++){
				for(c = 0; c < LEVEL_BUTTONS_WIDE; c++){
					levelButton = uiManager.addButton(
						buttonX + c * (levelButtonRect.width + LEVEL_BUTTON_GAP),
						buttonY + r * (levelButtonRect.height + LEVEL_BUTTON_GAP),
						renderer.numberButtonBlit, levelPressed, levelButtonRect
					);
					levelButton.releaseCallback = levelReleased;
					levelButton.id = c + r * LEVEL_BUTTONS_WIDE;
					levelButton.targetId = 1;
					levelButtons.push(levelButton);
				}
			}
			int border = 2;
			var buttonRect = new Rectangle(0, 0, renderer.cancelButtonBlit.width, renderer.cancelButtonBlit.height);
			leftButton = uiManager.addButton(Game.WIDTH * 0.5 - 3 * Game.SCALE + border, Game.HEIGHT - Game.SCALE * 2 + border, renderer.leftButtonBlit, directionPressed, buttonRect);
			leftButton.visible = false;
			backButton = uiManager.addButton(Game.WIDTH * 0.5 - 1 * Game.SCALE + border, Game.HEIGHT - Game.SCALE * 2 + border, renderer.cancelButtonBlit, backPressed, buttonRect);
			rightButton = uiManager.addButton(Game.WIDTH * 0.5 + 1 * Game.SCALE + border, Game.HEIGHT - Game.SCALE * 2 + border, renderer.rightButtonBlit, directionPressed, buttonRect);
			if(!Game.MOBILE){
				loadFromDesktopButton = uiManager.addButton(Game.WIDTH * 0.5 - 5 * Game.SCALE + border, Game.HEIGHT - Game.SCALE * 2 + border, renderer.loadButtonBlit, loadFromDesktop, buttonRect);
				loadFromDesktopButton.feedCallbackToEvent = true;
				saveToDesktopButton = uiManager.addButton(Game.WIDTH * 0.5 + 3 * Game.SCALE + border, Game.HEIGHT - Game.SCALE * 2 + border, renderer.saveButtonBlit, saveToDesktop, buttonRect);
				saveToDesktopButton.feedCallbackToEvent = true;
			}
			scoreTextBox = new TextBox(Game.WIDTH, 8, 0x0, 0x0);
			scoreTextBox.align = "center";
			if(UserData.settings.best != null){
				setScore((int)UserData.settings.best);
			}
			uiManager.changeGroup(0);
		}

        public void main() {
			if(UIManager.dialog != null){
				UIManager.dialog.update(
					game.mouseX,
					game.mouseY,
					game.mousePressed,
					game.mousePressedCount == game.frameCount,
					game.mouseReleasedCount == game.frameCount
				);
			} else {
				uiManager.update(
					game.mouseX,
					game.mouseY,
					game.mousePressed,
					game.mousePressedCount == game.frameCount,
					game.mouseReleasedCount == game.frameCount
				);
				if(game.editing){
					if(buttonInHand != null && buttonInHand.heldCount == 0){
						buttonInHand.x = game.mouseX - buttonInHand.blit.width * 0.5;
						buttonInHand.y = game.mouseY - buttonInHand.blit.height * 0.5;
					}
				}
			}
		}

        public void setScore(int n) {
			scoreTextBox.text = "" + n;
			scoreTextBox.bitmapData.colorTransform(scoreTextBox.bitmapData.rect, Renderer.UI_COL_TRANSFORM);
		}

        private void adventurePressed() {
			game.editing = false;
			game.setNextGame(Room.ADVENTURE);
			game.transition.begin(game.initLevel, Transition.DEFAULT_TRANSITION_TICKS, Transition.DEFAULT_TRANSITION_TICKS, "@", 15);
		}
		
		private void puzzlePressed() {
			game.editing = uiManager.lastButton == editorButton;
			if(game.editing){
				Library.setLevels(false);
				if(loadFromDesktopButton != null) loadFromDesktopButton.visible = true;
				if(saveToDesktopButton != null) saveToDesktopButton.visible = true;
			} else {
				Library.setLevels(true);
				if(loadFromDesktopButton != null) loadFromDesktopButton.visible = false;
				if(saveToDesktopButton != null) saveToDesktopButton.visible = false;
			}
			// restore puzzle progress from app exit if available
			if(!game.editing && UserData.settings.puzzleData != null){
				int n = UserData.settings.puzzleLevel;
				String str = (n < 10 ? "0" : "") + n;
				game.setNextGame(Room.PUZZLE, n);
				game.transition.begin(game.initLevel, Transition.DEFAULT_TRANSITION_TICKS, Transition.DEFAULT_TRANSITION_TICKS, str, 15);
			} else {
				if(firstLevel > Library.maxLevel) firstLevel = 0;
				if(firstLevel == 0){
					leftButton.visible = false;
				} else {
					leftButton.visible = true;
				}
				if(firstLevel < Library.maxLevel - PAGE_LEVELS){
					rightButton.visible = true;
				} else {
					rightButton.visible = false;
				}
				uiManager.changeGroup(PUZZLE_LEVELS);
			}
		}
		
		private void backPressed() {
			uiManager.changeGroup(ROOT);
		}
		
		private void levelPressed() {
			if(!game.editing){
				int n = uiManager.lastButton.id + firstLevel;
				String str = (n < 10 ? "0" : "") + n;
				game.setNextGame(Room.PUZZLE, n);
				game.transition.begin(game.initLevel, Transition.DEFAULT_TRANSITION_TICKS, Transition.DEFAULT_TRANSITION_TICKS, str, 15);
			} else {
				buttonInHand = uiManager.lastButton;
				sourceId = buttonInHand.id + firstLevel;
				sourceLevel = Library.levels[sourceId];
				targetLevel = null;
				prevButtonX = uiManager.lastButton.x;
				prevButtonY = uiManager.lastButton.y;
				
			}
		}
		
		private void levelReleased() {
			if(buttonInHand != null){
				buttonInHand.x = prevButtonX;
				buttonInHand.y = prevButtonY;
				int i;
                BlitButton overButton;
				int x = (int)(Game.WIDTH * 0.5 - renderer.levelMovePanelBlit.width * 0.5);
				int y = (int)(Game.HEIGHT * 0.5 - renderer.levelMovePanelBlit.height * 0.5);
				for(i = 0; i < uiManager.buttonsOver.length; i++){
					overButton = uiManager.buttonsOver[i];
					if(overButton != buttonInHand){
						if(overButton.targetId > 0){
							int step = renderer.cancelButtonBlit.width + 1;
							UIManager.openDialog(
								new Array<Point> { 
									new Point(x, y),
									new Point(x + 3, y + 3),
									new Point(x + 3 + (step * 1), y + 3),
									new Point(x + 3 + (step * 2), y + 3),
									new Point(x + 3 + (step * 3), y + 3),
									new Point(x + 3 + (step * 4), y + 3)
								},
								new Array<BlitRect> { 
									renderer.levelMovePanelBlit,
									renderer.cancelButtonBlit,
									renderer.insertBeforeButtonBlit,
									renderer.insertAfterButtonBlit,
									renderer.swapButtonBlit,
									renderer.saveButtonBlit
								},
								new Array<Action> { 
									null,
									UIManager.closeDialog,
									insertBefore,
									insertAfter,
									swapLevels,
									writeSourceToTarget
								},
								game.selectSound
							);
							// swap init
							targetId = firstLevel + overButton.id;
							targetLevel = Library.levels[targetId];
							overButton.over = false;
							buttonInHand.over = false;
							buttonInHand = null;
							uiManager.buttonsOver.length = 0;
							return;
						} else {
							if(overButton == backButton){
								confirmDialog(deleteLevelDialog);
							} else if(overButton == rightButton){
								targetId = sourceId + PAGE_LEVELS;
								insertAfter();
							} else if(overButton == leftButton){
								targetId = sourceId - PAGE_LEVELS;
								insertBefore();
							}
						}
					}
				}
				buttonInHand = null;
			}
			if(game.editing && uiManager.lastButton.heldCount > 0){
				int n = uiManager.lastButton.id + firstLevel;
				String str = (n < 10 ? "0" : "") + n;
				game.setNextGame(Room.PUZZLE, n);
				game.transition.begin(game.initLevel, Transition.DEFAULT_TRANSITION_TICKS, Transition.DEFAULT_TRANSITION_TICKS, str, 30);
			}
		}
		
		public void deleteLevelDialog() {
			Library.levels[sourceId] = null;
			sourceLevel = null;
			if(Library.saveUserLevelsCallback != null) Library.saveUserLevelsCallback();
			UIManager.closeDialog();
		}
		
		public void swapLevels() {
			if(targetLevel == null) return;
			Library.levels[sourceId] = (Library.LevelData)targetLevel;
			Library.levels[targetId] = (Library.LevelData)sourceLevel;
			sourceLevel = Library.levels[sourceId];
			targetLevel = Library.levels[targetId];
			sourceId ^= targetId;
			targetId ^= sourceId;
			sourceId ^= targetId;
			if(Library.saveUserLevelsCallback != null) Library.saveUserLevelsCallback();
		}
		
		public void writeSourceToTarget() {
			if(targetLevel == null){
				targetLevel = LevelData.saveObject(Level.ROOM_WIDTH, Level.ROOM_HEIGHT);
			}
			confirmDialog(confirmWrite);
		}
		
		private void confirmWrite() {
            //CONVERSION - converted this ok but I don't think it's called
			LevelData.writeToObject(sourceLevel, targetLevel, Level.ROOM_WIDTH, Level.ROOM_HEIGHT);
			Library.levels[targetId] = (Library.LevelData)targetLevel;
			if(Library.saveUserLevelsCallback != null) Library.saveUserLevelsCallback();
			UIManager.closeDialog();
		}
		
		private void confirmDialog(Action callback) {
			if(UIManager.dialog != null) UIManager.closeDialog();
			UIManager.confirmDialog(
				Game.WIDTH * 0.5 - renderer.confirmPanelBlit.width * 0.5,
				Game.HEIGHT * 0.5 - renderer.confirmPanelBlit.height * 0.5,
				renderer.confirmPanelBlit,
				callback,
				renderer.confirmButtonBlit,
				renderer.cancelButtonBlit,
				game.selectSound
			);
		}
		
		private void insertAfter() {
			Library.levels.splice(sourceId, 1);
			if(targetId < sourceId) targetId++;
			Library.levels.splice(targetId, 0, (Library.LevelData)sourceLevel);
			if(Library.saveUserLevelsCallback != null) Library.saveUserLevelsCallback();
			UIManager.closeDialog();
		}
		
		private void insertBefore() {
			Library.levels.splice(sourceId, 1);
			if(targetId > sourceId) targetId--;
			Library.levels.splice(targetId, 0, (Library.LevelData)sourceLevel);
			if(Library.saveUserLevelsCallback != null) Library.saveUserLevelsCallback();
			UIManager.closeDialog();
		}
		
		private void directionPressed() {
			if(uiManager.lastButton == leftButton){
				if(firstLevel > 0){
					firstLevel -= PAGE_LEVELS;
					if(firstLevel == 0){
						leftButton.visible = false;
					}
					rightButton.visible = true;
				}
			} else if(uiManager.lastButton == rightButton){
				if(firstLevel <= Library.maxLevel - PAGE_LEVELS){
					firstLevel += PAGE_LEVELS;
					if(firstLevel > Library.maxLevel - PAGE_LEVELS){
						rightButton.visible = false;
					}
					leftButton.visible = true;
				}
			}
		}
		
		private void loadFromDesktop() {
            levelsLoaded();
		}
		
		private void levelsLoaded() {
            
            #if JSON
            var data = XnaGame.Instance.StorageManager.Load<string>(_userLevelsFilename);
            #else
            var data = XnaGame.Instance.StorageManager.Load<Library.LevelData[]>(_userLevelsFilename);
            #endif

            if (data == null)
                return;

            #if JSON
            var deserializedLevels = JsonConvert.DeserializeObject<Library.LevelData[]>(data);
            #else
            var deserializedLevels = data;//XnaGame.Instance.StorageManager.DeserializeObject<Library.LevelData[]>(data);
            #endif
            Library.USER_LEVELS = new Array<Library.LevelData>(deserializedLevels);
            Library.levels = Library.USER_LEVELS;
		}
		
		private void saveToDesktop() {
            
            #if JSON
            var data = JsonConvert.SerializeObject(Library.USER_LEVELS);
            #else
            var data = Library.USER_LEVELS;
            #endif
            
            XnaGame.Instance.StorageManager.Save(data, _userLevelsFilename);
		}
		
		public void renderPreview(Library.LevelData obj, double x, double y, BitmapData target) {
            BitmapData bitmapData = Level.getLevelBitmapData(obj.map, Level.ROOM_WIDTH, Level.ROOM_HEIGHT);
			var p = new Point(x, y);
			target.copyPixels(bitmapData, bitmapData.rect, p);
		}
		
		public void render() {
			uiManager.render(renderer.guiBitmapData);
			
			if(uiManager.currentGroup == ROOT){
                //CONVERSION
                //renderer.guiBitmapData.copyPixels(scoreTextBox.bitmapData, scoreTextBox.bitmapData.rect, new Point(0, adventureButton.y + adventureButton.blit.height), null, null, true);
				renderer.guiBitmapData.copyPixels(scoreTextBox.text, scoreTextBox.bitmapData.rect, new Point(scoreTextBox._position.x, adventureButton.y + adventureButton.blit.height), 1.0f);
			} else if(uiManager.currentGroup == PUZZLE_LEVELS){
				int i;
                BlitButton button;
				for(r = 0; r < LEVEL_BUTTONS_HIGH; r++){
					for(c = 0; c < LEVEL_BUTTONS_WIDE; c++){
						i = c + r * LEVEL_BUTTONS_WIDE;
						button = levelButtons[i];
						button.visible = true;
						if(!game.editing){
							if(firstLevel + i > Library.maxLevel){
								button.visible = false;
								continue;
							}
						}
						if(Library.levels[firstLevel + i] != null){
							if(!game.editing){
								if(!UserData.settings.completed[firstLevel + i]){
									renderer.notCompletedBlit.x = (int)button.x;
									renderer.notCompletedBlit.y = (int)button.y;
									renderer.notCompletedBlit.render(renderer.guiBitmapData);
								}
							}
							renderer.numberBlit.setValue(firstLevel + i);
							renderer.numberBlit.x = (int)button.x + 2;
							renderer.numberBlit.y = (int)button.y + 2;
							renderer.numberBlit.renderNumbers(renderer.guiBitmapData);
							
						}
					}
				}
				if(game.editing && game.transition.dir <= 0){
					if(UIManager.dialog == null){
						if(!(Game.MOBILE && game.mouseReleasedCount == game.frameCount)){
							for(i = uiManager.buttonsOver.length - 1; i > -1; i--){
								button = uiManager.buttonsOver[i];
								if(button.targetId > 0 && Library.levels[firstLevel + button.id] != null){
									button.render(renderer.guiBitmapData);
									
                                    renderer.numberBlit.setValue(firstLevel + button.id);
									renderer.numberBlit.x = (int)button.x + 2;
									renderer.numberBlit.y = (int)button.y + 2;
									renderer.numberBlit.renderNumbers(renderer.guiBitmapData);
									renderer.levelPreviewPanelBlit.x = (int)button.x;
									renderer.levelPreviewPanelBlit.y = (int)(
										button == buttonInHand ? button.y - (Level.ROOM_HEIGHT + 3) : button.y + button.blit.height
									);
									renderer.levelPreviewPanelBlit.render(renderer.guiBitmapData);
									renderPreview(Library.levels[firstLevel + button.id], renderer.levelPreviewPanelBlit.x + 1, renderer.levelPreviewPanelBlit.y + 1, renderer.guiBitmapData);
								}
							}
						}
					} else {
						if(sourceLevel != null || targetLevel != null){
							button = UIManager.dialog.buttons[0];
							renderer.levelPreviewPanelBlit.x = (int)(button.x + button.blit.width * 0.5 - renderer.levelPreviewPanelBlit.width * 0.5);
							if(sourceLevel != null){
								renderer.levelPreviewPanelBlit.y = (int)button.y - (Level.ROOM_HEIGHT + 3);
								renderer.levelPreviewPanelBlit.render(renderer.guiBitmapData);
								renderPreview(sourceLevel, renderer.levelPreviewPanelBlit.x + 1, renderer.levelPreviewPanelBlit.y + 1, renderer.guiBitmapData);
							}
							if(targetLevel != null){
								renderer.levelPreviewPanelBlit.y = (int)button.y + button.blit.height;
								renderer.levelPreviewPanelBlit.render(renderer.guiBitmapData);
								renderPreview(targetLevel, renderer.levelPreviewPanelBlit.x + 1, renderer.levelPreviewPanelBlit.y + 1, renderer.guiBitmapData);
							}
						}
					}
				}
				if(UIManager.dialog != null) UIManager.dialog.render(renderer.guiBitmapData);
				
			}
		}
    }
}
