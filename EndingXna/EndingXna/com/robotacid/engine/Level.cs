using System;
using com.robotacid.gfx;
using com.robotacid.sound;
using com.robotacid.ui;
using flash.display;
using flash;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Array = flash.Array;
using Math = flash.Math;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.engine
{
    /// <summary>
    /// Buffer between the game logic and the architecture
    ///
	/// Input and rendering is handled at this tier.
	///
	/// This class requires heavy butchering to port to new languages. Show no mercy.
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class Level {

        public static Renderer renderer;
        public static Game game;

        public LevelData data;
		public Room room;
		public Array<Array<int>> blackOutMap;
		public Array<Point> endingKillList = new Array<Point>();
		
		public UIManager uiManager;
        public FoodClockFX foodClockGfx;
		public BlitButton checkButton;
		public BlitButton settingsButton;
		public BlitButton controlsButton;
		public BlitButton orientationButton;
		public BlitButton soundButton;
		public BlitButton fullscreenButton;
		public TextBox scoreTextBox;
		
		public Boolean active;
		public int state;
		public int phase;
		public int animCount;
		public Boolean keyDown;
		public Boolean checkView;
		public Boolean tapControls;
		public int animAccCount;
		public int animDelay;
		public double moveStep;
		public double restStep;
		public int endingKillCount;
		public int endingDir;
		
		private int blinkCount;
		public Boolean blink;
		
		// temp
		private Point p;
		
		// states
		public const int IDLE = 0;
		public const int ANIMATE = 1;
		public const int ENDING_ANIM = 2;
		
		// phases
		public const int PLAYER_PHASE = 0;
		public const int ENEMY_PHASE = 1;
		
		
		public static readonly double SCALE = Game.SCALE;
		public static readonly double INV_SCALE = 1 / Game.SCALE;
		public const int ANIM_FRAMES_MAX = 3;
		public const int ANIM_FRAMES_MIN = 2;
		public const int ANIM_ACC_DELAY = 10;
		public const int BLINK_DELAY = 30;
		public const int BLINK_DELAY_VISIBLE = 10;
		public const int ENDING_KILL_DELAY = 6;
		public const int SEGUE_SPEED = 2;
		// turner rotation frames
		public const int TURNER_N = 0;
		public const int TURNER_NE = 1;
		public const int TURNER_E = 2;
		public const int TURNER_SE = 3;
		public const int TURNER_S = 4;
		public const int TURNER_SW = 5;
		public const int TURNER_W = 6;
		public const int TURNER_NW = 7;
		
		public const int MAP_WIDTH = 25;
		public const int MAP_HEIGHT = 25;
		public const int ROOM_WIDTH = 13;
		public const int ROOM_HEIGHT = 13;

        readonly Array<string> bassyKillSounds = new Array<string> { "bassyKill1", "bassyKill2", "bassyKill3", "bassyKill4" };
        readonly Array<string> doorSounds = new Array<string> { "door1", "door2", "door3", "door4" };
        readonly Array<string> stepSounds = new Array<string> { "step1", "step2", "step3" };
        readonly Array<string> deathSounds = new Array<string> { "death1", "death2", "death3", "death4" };
		readonly Array<string> pushSounds = new Array<string> { "push1", "push2", "push3", "push4" };
        readonly Array<string> blockedSounds = new Array<string> { "blocked1", "blocked2", "blocked3", "blocked4" };
        readonly Array<string> generatedSounds = new Array<string> { "generator1", "generator2", "generator3", "generator4" };
        readonly Array<string> adventureSounds = new Array<string> { "adventure1", "adventure2", "adventure3", "adventure4" };
        readonly Array<string> endingSounds = new Array<string> { "ending1", "ending2", "ending3", "ending4" };
        readonly Array<string> virusSounds = new Array<string> { "punchRattle", "hitRattle" };
        readonly Array<string> rattleSounds = new Array<string> { "rattle1", "rattle2" };
        readonly Array<string> allySounds = new Array<string> { "ally1", "ally2", "ally3", "ally4" };
        readonly Array<string> moverKillSounds = new Array<string> { "moverKill1", "moverKill2", "moverKill3", "moverKill4" };
        readonly Array<string> meatyKillSounds = new Array<string> { "meatyKill1", "meatyKill2", "meatyKill3", "meatyKill4" };
        readonly Array<string> trapTwangSounds = new Array<string> { "trapTwang1", "trapTwang2", "trapTwang3", "trapTwang4" };
        readonly Array<string> rattleKillSounds = new Array<string> { "rattleKill1", "rattleKill2", "rattleKill3", "rattleKill4" };
	
        public Level(int type = Room.PUZZLE, Library.LevelData dataObj = null) {
            active = true;
            room = new Room(type, ROOM_WIDTH, ROOM_HEIGHT);
            room.init();
            if(!LevelData.initialised) LevelData.init();
            int dataWidth = type == Room.PUZZLE ? ROOM_WIDTH : MAP_WIDTH;
            int dataHeight = type == Room.PUZZLE ? ROOM_HEIGHT : MAP_HEIGHT;
            data = new LevelData(room, dataWidth, dataHeight);
            if(dataObj != null){
                data.loadData(dataObj);
                if(type == Room.PUZZLE){
                    blackOutMap = data.getBlackOutMap();
                }
            }
            state = IDLE;
            phase = PLAYER_PHASE;
            animDelay = ANIM_FRAMES_MAX;
            animAccCount = ANIM_ACC_DELAY;
            data.killCallback = kill;
            data.displaceCallback = displaceCamera;
            data.pushCallback = push;
            data.swapCallback = swap;
            data.generateCallback = generate;
            data.endingCallback = ending;
            data.blockedCallback = blocked;
            keyDown = false;
            blink = true;
            // copy player gfx buffer
            //TODO - Foodclock 
            //renderer.gameSpriteSheet.copyPixels(renderer.gameSpriteSheet, renderer.playerBuffer.rect, new Point(renderer.playerBlit.rect.x, renderer.playerBlit.rect.y));
            foodClockGfx = new FoodClockFX(renderer.playerBlit, Renderer.WALL_COL);
            renderer.numberBlit.setValue(data.food);
			
            uiManager = new UIManager();
            uiManager.selectSoundCallback = game.selectSound;
            settingsButton = uiManager.addButton(Game.WIDTH - (renderer.settingsButtonBlit.width), 0, renderer.settingsButtonBlit, openSettings);
            settingsButton.visible = game.hideMouseFrames < Game.HIDE_MOUSE_FRAMES;
			
            uiManager.addGroup();
            uiManager.changeGroup(1);
            uiManager.addButton(Game.WIDTH - (renderer.settingsButtonBlit.width), 0, renderer.settingsButtonBlit, resume);
            int border = 2;
            Rectangle buttonRect = new Rectangle(0, 0, renderer.propertyButtonBlit.width, renderer.propertyButtonBlit.height);
            var buttonXs = UIManager.distributeRects(Game.WIDTH * 0.5, buttonRect.width, 4, 3);
            var buttonYs = UIManager.distributeRects(Game.HEIGHT * 0.5, buttonRect.height, 4, 2);
            double buttonX;// = Game.HEIGHT * 0.5 - Game.SCALE + border;
            double buttonY;// = Game.HEIGHT * 0.5 - Game.SCALE + border;
            buttonY = buttonYs[0];
            uiManager.addButton(buttonXs[0], buttonY, renderer.cancelButtonBlit, quit, buttonRect);
            uiManager.addButton(buttonXs[1], buttonY, renderer.resetButtonBlit, reset, buttonRect);
            checkButton = uiManager.addButton(buttonXs[2], buttonY, renderer.checkButtonBlit, toggleCheckView, buttonRect);
            checkButton.active = checkView;
            buttonXs = UIManager.distributeRects(Game.WIDTH * 0.5, buttonRect.width, 4, 2);
            // mobile buttons
            buttonY = buttonYs[1];
            controlsButton = uiManager.addButton(buttonXs[0], buttonY, renderer.controlsButtonBlit, toggleControls, buttonRect);
            controlsButton.active = tapControls;
            orientationButton = uiManager.addButton(buttonXs[1], buttonY, renderer.orientationButtonBlit, game.toggleOrientation, buttonRect);
            // desktop buttons
            soundButton = uiManager.addButton(buttonXs[0], buttonY, renderer.soundButtonBlit, toggleSound, buttonRect);
            soundButton.active = !SoundManager.active;
            fullscreenButton = uiManager.addButton(buttonXs[1], buttonY, renderer.fullscreenButtonBlit, toggleFullscreen, buttonRect);
            fullscreenButton.active = game.stage.displayState != "normal";;
            fullscreenButton.feedCallbackToEvent = true;
            if(!Game.MOBILE){
                controlsButton.visible = false;
                orientationButton.visible = false;
            } else {
                soundButton.visible = false;
                fullscreenButton.visible = false;
            }
			
            uiManager.changeGroup(0);
			
            scoreTextBox = new TextBox(Game.WIDTH, 8, 0x0, 0x0);
            scoreTextBox.align = "center";
		}
		
		private void resume() { 
			uiManager.changeGroup(0);
		}
		
		private void quit() {
			active = false;
			game.transition.begin(game.quit, 10, 10);
		}
		
		private void reset() {
			String str;
            int time;
			if(room.type == Room.PUZZLE){
				str = (game.currentLevel < 10 ? "0" : "") + game.currentLevel;
				time = 30;
			} else {
				str = "everything was\nbeautiful\nand nothing hurt";
				time = 45;
			}
			game.transition.begin(game.resetGame, Game.DEATH_FADE_COUNT, Game.DEATH_FADE_COUNT, str, time);
		}
		
		public void toggleControls() {
            tapControls = !tapControls;
            controlsButton.active = tapControls;
            game.roomPainter.palette.controlsButton.active = tapControls;
            UserData.settings.tapControls = tapControls;
            UserData.push(true);
		}
		
		public void toggleSound() {
            game.toggleSound();
            soundButton.active = !SoundManager.active;
            game.roomPainter.palette.soundButton.active = !SoundManager.active;
		}
		
		public void toggleFullscreen() {
            game.toggleFullscreen();
            fullscreenButton.active = game.roomPainter.palette.fullscreenButton.active = game.stage.displayState != "normal";
		}
		
        public void main() {
            int dir = 0;
			
			if(uiManager.active) uiManager.update(
				game.mouseX,
				game.mouseY,
				game.mousePressed,
				game.mousePressedCount == game.frameCount
			);
			
			if(state == IDLE){
				if(phase == PLAYER_PHASE){
					
					if(!uiManager.mouseLock && uiManager.currentGroup == 0) dir = getInput();
					
					var playerProperty = data.map[(int)data.player.y][(int)data.player.x];
					// don't repeat a blocked move - no dry humping the walls
					if(dir > 0 && !(((playerProperty & Room.BLOCKED) > 0) && ((playerProperty & dir) == dir))){
						// accelerate animation length
						if(animAccCount > 0){
							if(animDelay > ANIM_FRAMES_MIN) animDelay--;
						}
						animAccCount = ANIM_ACC_DELAY;
						data.playerTurn(dir);
						if((data.map[(int)data.player.y][(int)data.player.x] & Room.BLOCKED) > 0){
							game.soundQueue.addRandom("blocked", blockedSounds);
						} else {
							game.soundQueue.addRandom("step", stepSounds);
						}
						initAnimate();
					} else {
						if(animAccCount > 0){
							animAccCount--;
							if(animAccCount == 0) animDelay = ANIM_FRAMES_MAX;
						}
					}
				} else if(phase == ENEMY_PHASE){
					data.enemyTurn();
					initAnimate();
					if(!data.alive()) game.soundQueue.addRandom("death", deathSounds);
				}
			} else if(state == ANIMATE){
				animCount--;
				if(animCount == 0){
					state = IDLE;
					//if(phase == PLAYER_PHASE && !((data.map[data.player.y][data.player.x] & Room.BLOCKED) == Room.BLOCKED)){
                    if(phase == PLAYER_PHASE && !((data.map[(int)data.player.y][(int)data.player.x] & Room.BLOCKED) > 0)){
						if(data.ended){
							if(room.type == Room.PUZZLE){
								game.puzzleWin();
							} else if(room.type == Room.ADVENTURE){
								state = ENDING_ANIM;
								endingKillList.Clear();
								data.fillPathMap((int)data.player.x, (int)data.player.y, null, Room.ENEMY | Room.WALL, endingKillList, Room.DOOR);
								endingKillCount = ENDING_KILL_DELAY;
							}
						} else {
							phase = ENEMY_PHASE;
							blinkCount = BLINK_DELAY_VISIBLE;
						}
						
					} else if(phase == ENEMY_PHASE){
						foodClockGfx.setFood(data.food, LevelData.FOOD_MAX, data.player.x * SCALE, data.player.y * SCALE);
						if(!data.alive()){
							active = false;
							game.death();
						}
						phase = PLAYER_PHASE;
					}
				}
			} else if(state == ENDING_ANIM){
				if(endingKillCount > 0){
					endingKillCount--;
					if(endingKillCount == 0){
						if(endingKillList.length > 0){
							var p = endingKillList.pop();
							data.kill((int)p.x, (int)p.y, 0);
							endingKillCount = (int)(ENDING_KILL_DELAY + Math.random() * ENDING_KILL_DELAY);
						} else {
							if((endingDir & Room.UP) > 0){
								renderer.setSlide(0, SEGUE_SPEED);
							} else if((endingDir & Room.RIGHT) > 0){
								renderer.setSlide(-SEGUE_SPEED, 0);
							} else if((endingDir & Room.DOWN) > 0){
								renderer.setSlide(0, -SEGUE_SPEED);
							} else if((endingDir & Room.LEFT) > 0){
								renderer.setSlide(SEGUE_SPEED, 0);
							}
							game.soundQueue.addRandom("adventure", adventureSounds);
							game.soundQueue.addRandom("adventureEnding", endingSounds);
							game.adventureWin();
						}
					}
				}
			}
			blinkCount--;
			if(blinkCount <= 0) blinkCount = BLINK_DELAY;
			blink = blinkCount >= BLINK_DELAY_VISIBLE;
		}
		
		public void initAnimate() {
			state = ANIMATE;
			animCount = animDelay;
		}
		
		public void toggleCheckView() {
			checkView = !checkView;
			checkButton.active = checkView;
			game.roomPainter.palette.checkButton.active = checkView;
			UserData.settings.checkView = checkView;
			UserData.push(true);
		}
		
		public void openSettings() {
			if(game.editing){
				game.modifiedLevel = true;
				game.level.data.loadData(game.currentLevelObj);
				game.roomPainter.setActive(true);
			} else {
				uiManager.changeGroup(1);
				if(room.type == Room.ADVENTURE){
					scoreTextBox.text = "" + data.turns;
				}
			}
			fullscreenButton.active = game.roomPainter.palette.fullscreenButton.active = game.stage.displayState != "normal";
		}
		
		public int getInput() {
			var dir = 0;
			//if(Key.keysPressed == 1){
				if(Key.isDown(Keyboard.UP) || Key.isDown(Key.K) || Key.customDown(Game.UP_KEY) || Key.isDown(Key.NUMBER_8)) dir |= Room.UP;
				if(Key.isDown(Keyboard.LEFT) || Key.isDown(Key.H) || Key.customDown(Game.LEFT_KEY) || Key.isDown(Key.NUMBER_4)) dir |= Room.LEFT;
				if(Key.isDown(Keyboard.RIGHT) || Key.isDown(Key.L) || Key.customDown(Game.RIGHT_KEY) || Key.isDown(Key.NUMBER_6)) dir |= Room.RIGHT;
				if(Key.isDown(Keyboard.DOWN) || Key.isDown(Key.J) || Key.customDown(Game.DOWN_KEY) || Key.isDown(Key.NUMBER_2)) dir |= Room.DOWN;
			//}
			if(game.mousePressed){
				if(tapControls){
					if((game.mouseCorner & Room.UP) > 0) dir |= Room.UP;
					else if((game.mouseCorner & Room.RIGHT) > 0) dir |= Room.RIGHT;
					else if((game.mouseCorner & Room.DOWN) > 0) dir |= Room.DOWN;
					else if((game.mouseCorner & Room.LEFT) > 0) dir |= Room.LEFT;
				} else {
					dir = game.getMouseSwipe();
				}
			}
			return dir;
		}
		
		public void push(int x, int y, int dir) {
			if((dir & Room.UP) > 0){
				renderer.glitchMap.pushCols((int)(x * SCALE), (int)((x + 1) * SCALE), -3);
			} else if((dir & Room.RIGHT) > 0){
				renderer.glitchMap.pushRows((int)(y * SCALE), (int)((y + 1) * SCALE), 3);
			} else if((dir & Room.DOWN) > 0){
				renderer.glitchMap.pushCols((int)(x * SCALE), (int)((x + 1) * SCALE), 3);
			} else if((dir & Room.LEFT) > 0){
				renderer.glitchMap.pushRows((int)(y * SCALE), (int)((y + 1) * SCALE), -3);
			}
			game.createDistSound(x, y, "push", pushSounds);
		}
		
		public void swap(int x, int y, int target) {
			if((target & Room.ALLY) > 0){
				game.soundQueue.add("slide");
			} else if((target & Room.SWAP) > 0){
				game.soundQueue.add("strangeExchange");
			}
		}
		
		private void blocked(int x, int y, int dir) {
			game.createDistSound(x, y, "blocked", blockedSounds, 0.5);
		}
		
		public void generate(int x, int y) {
			game.createDistSound(x, y, "generator", generatedSounds);
		}
		
		public void kill(int x, int y, int dir, int property = 0, int explosion = 0) {
			if(explosion != 0){
				renderer.addFX(x * SCALE, y * SCALE, renderer.explosionBlit, null, explosion);
			}
			if(property != 0){
				var blit = getPropertyBlit(property) as BlitSprite;
				var dirs = (~Room.rotateBits(dir, 2, Room.UP_DOWN_LEFT_RIGHT, 4)) & Room.UP_DOWN_LEFT_RIGHT;
				if((property & Room.ENDING) != 0){
					dirs = dir;
				}
				renderer.bitmapDebris(blit, x, y, dirs);
				if((property & Room.GENERATOR) > 0) renderer.bitmapDebris(renderer.generatorBlit, x, y, dirs);
				
				var soundStep = 1 + (data.turns % 4);
				
				if((property & Room.TURNER) > 0){
					game.createDistSound(x, y, moverKillSounds[soundStep - 1]);
				} else if((property & Room.VIRUS) != 0){
					game.createDistSound(x, y, "virus", virusSounds);
				} else if(((property & Room.ALLY) > 0) && !((property & Room.PLAYER) > 0)){
					game.createDistSound(x, y, allySounds[soundStep - 1]);
				} else if((property & Room.TRAP) > 0) {
					game.createDistSound(x, y, "rattle", rattleSounds);
				} else if((property & Room.ENDING) != 0) {
					if(room.type == Room.PUZZLE) 
                        game.createDistSound(x, y, endingSounds[soundStep - 1]);
					else {
						game.soundQueue.add("twangKill");
					}
				} else if((property & Room.DOOR) > 0){
					game.soundQueue.add(doorSounds[soundStep - 1]);
					if((property & Room.INCREMENT) > 0){
						game.soundQueue.add("twangMove");
						var ex = x;
						var ey = y;
						if((property & Room.M_UP) > 0) ey--;
						else if((property & Room.M_RIGHT) > 0) ex++;
						else if((property & Room.M_DOWN) > 0) ey++;
						else if((property & Room.M_LEFT) > 0) ex--;
						renderer.bitmapDebris(renderer.whiteBlit, ex, ey, dirs);
					}
				} else if((property & Room.MOVER) > 0){
					game.createDistSound(x, y, rattleKillSounds[soundStep - 1]);
					if((property & Room.M_UP_DOWN_LEFT_RIGHT) == Room.M_UP_DOWN_LEFT_RIGHT){
						game.createDistSound(x, y, trapTwangSounds[soundStep - 1]);
					}
				} else if((property & Room.WALL) > 0){
					game.createDistSound(x, y, "wall", bassyKillSounds);
				}
				if((property & Room.GENERATOR) > 0){
					game.createDistSound(x, y, meatyKillSounds[soundStep - 1]);
				}
			}
			if(explosion > 0){
				game.createDistSound(x, y, "bitBlastRattle");
			}
			var shakeX = 0;
            int shakeY = 0;
			if((dir & Room.UP) > 0){
				renderer.glitchMap.addGlitchCols((int)(x * SCALE), (int)((x + 1)  * SCALE), -1);
				shakeY = -2;
			} else if((dir & Room.RIGHT) > 0){
				renderer.glitchMap.addGlitchRows((int)(y * SCALE), (int)((y + 1) * SCALE), 1);
				shakeX = 2;
			} else if((dir & Room.DOWN) > 0){
				renderer.glitchMap.addGlitchCols((int)(x * SCALE), (int)((x + 1) * SCALE), 1);
				shakeY = 2;
			} else if((dir & Room.LEFT) > 0){
				renderer.glitchMap.addGlitchRows((int)(y * SCALE), (int)((y + 1) * SCALE), -1);
				shakeX = -2;
			}
			renderer.shake(shakeX, shakeY);
		}
		
		public void ending(int x, int y, int dir) {
			renderer.refresh = false;
			renderer.trackPlayer = false;
			if(room.type == Room.PUZZLE){
				if((dir & Room.UP) > 0) {
					renderer.setSlide(0, SEGUE_SPEED);
				} else if((dir & Room.RIGHT) > 0) {
					renderer.setSlide(-SEGUE_SPEED, 0);
				} else if((dir & Room.DOWN) > 0) {
					renderer.setSlide(0, -SEGUE_SPEED);
				} else if((dir & Room.LEFT) > 0) {
					renderer.setSlide(SEGUE_SPEED, 0);
				}
			} else {
				renderer.setSlide(0, 0);
				endingDir = dir;
			}
			
			renderer.bitmapDebris(renderer.playerBlit, x, y, dir | Room.rotateBits(dir, 2, Room.UP_DOWN_LEFT_RIGHT, 4));
		}
		
		public void displaceCamera(int x, int y, int revealDir, int eraseDir) {
            
            renderer.displace(x * SCALE, y * SCALE);
            renderer.addFX(0, 0, renderer.mapFadeBlits[revealDir], null, 0, false, false, false, true);
            // create render old room contents
            //CONVERSION - Not going to bother fading the new sections in - sorry (but feel free to implement)
            //Bitmap bitmap;
            //double bx = 0, by = 0;
            //if(eraseDir == Room.NORTH){
            //    bitmap = new Bitmap(renderMapSection(0, 0, data.width, room.height - 1));
            //} else if(eraseDir == Room.EAST){
            //    bitmap = new Bitmap(renderMapSection(room.width, 0, room.width - 1, data.height));
            //    bx = room.width * SCALE;
            //} else if(eraseDir == Room.SOUTH){
            //    bitmap = new Bitmap(renderMapSection(0, room.height, data.width, room.height - 1));
            //    by = room.height * SCALE;
            //} else if(eraseDir == Room.WEST){
            //    bitmap = new Bitmap(renderMapSection(0, 0, room.width - 1, data.height));
            //}
            //var blit = new FadingBlitClip(bitmap.bitmapData, 15);
            //renderer.addFX(bx + x * SCALE, by + y * SCALE, blit, null, 0, true, false, false, true);
		}
		
		public BitmapData renderMapSection(int x, int y, int width, int height) {
            //CONVERSION - Not going to bother fading the new sections in - sorry (but feel free to implement)
            return null;
            //var bitmapData = new BitmapData(width * SCALE, height * SCALE, true, 0x0);
            //var background = new Shape();
            //var matrix:Matrix = new Matrix();
            //matrix.tx = -x * SCALE;
            //matrix.ty = -y * SCALE;
            //background.graphics.lineStyle(0, 0, 0);
            //background.graphics.beginBitmapFill(renderer.backgroundBitmapData, matrix);
            //background.graphics.drawRect(0, 0, width * SCALE + 1, height * SCALE + 1);
            //bitmapData.draw(background);
            //int fromX = x;
            //int fromY = y;
            //int toX = x + width;
            //int toY = y + height;
            //int r, c;
            //var renderMap = data.map;
            //for(r = fromY; r < toY; r++){
            //    for(c = fromX; c < toX; c++){
            //        if(
            //            (c >= 0 && r >= 0 && c < data.width && r < data.height)
            //        ){
            //            if(renderMap[r][c] > 0){
            //                renderProperty((c - x) * SCALE, (r - y) * SCALE, renderMap[r][c], bitmapData);
            //            }
            //        }
            //    }
            //}
            //return bitmapData;
		}
		
		public BlitRect getPropertyBlit(int property, Boolean rotate = false) {
			BlitRect blit = null;
            BlitClip blitClip;
			if((property & Room.PLAYER) != 0) {
				blit = renderer.playerBlit;
			} else if((property & Room.ENEMY) != 0) {
				if((property & Room.MOVER) != 0) {
					if((property & Room.M_UP_DOWN_LEFT_RIGHT) == Room.M_UP_DOWN_LEFT_RIGHT){
						blit = renderer.moverBlit;
					} else if((property & Room.M_UP_DOWN_LEFT_RIGHT) == (Room.M_LEFT | Room.M_RIGHT)){
						blit = renderer.horizMoverBlit;
					} else if((property & Room.M_UP_DOWN_LEFT_RIGHT) == (Room.M_UP | Room.M_DOWN)){
						blit = renderer.vertMoverBlit;
					}
				} else if((property & Room.TRAP) != 0) {
					blitClip = renderer.trapBlit as BlitClip;
					// all 15 trap direction combinations map to the bitwise direction properties
					blitClip.frame = -1 + ((property & Room.M_UP_DOWN_LEFT_RIGHT) >> Room.M_DIR_SHIFT);
					blit = renderer.trapBlit;
				} else if((property & Room.TURNER) != 0) {
					blit = renderer.turnerBlit;
					blitClip = renderer.turnerBlit as BlitClip;
					blitClip.frame = getTurnerFrame(property, rotate);
				} else if((property & Room.DOOR) != 0) {
					blit = renderer.doorBlit;
					blitClip = renderer.doorBlit as BlitClip;
					if((property & Room.ENDING) != 0) {
						blitClip.frame = blitClip.totalFrames - 1;
					} else {
						blitClip.frame = blitClip.totalFrames - room.endingDist;
						if((property & Room.INCREMENT) != 0) {
							blitClip.frame++;
						}
						if(blitClip.frame < 0) blitClip.frame = 0;
						if(blitClip.frame > blitClip.totalFrames - 1) blitClip.frame = blitClip.totalFrames - 1;
					}
				} else if((property & Room.VIRUS) != 0) {
					blit = renderer.virusBlit;
				}
			} else if((property & Room.ALLY) != 0) {
				blit = renderer.allyBlit;
			} else if((property & Room.WALL) != 0) {
				if((property & Room.SWAP) != 0) {
					blit = renderer.swapBlit;
				} else if((property & Room.INDESTRUCTIBLE) != 0) {
					blit = renderer.indestructibleWallBlit;
				} else {
					blit = renderer.wallBlit;
				}
			} else {
				if(property == Room.BOMB){
					return renderer.bombBlit;
				} else if((property & Room.GENERATOR) != 0) {
					return renderer.generatorBlit;
				}
				blit = renderer.errorBlit;
			}
			return blit;
		}
		
		public void renderProperty(double x, double y, int property, BitmapData bitmapData, Boolean renderCheck = true) {
			BlitRect blit;
            Boolean displace = false;
            int frame = 0;
			if((property & Room.PLAYER) > 0){
				displace = phase == PLAYER_PHASE || ((property & Room.PUSHED) > 0);
				blit = renderer.playerBlit;
			} else if((property & Room.ENEMY) > 0){
				displace = phase == ENEMY_PHASE;
				if((property & Room.TURNER) > 0){
					// check we aren't a turner that's rotating
					displace = (
						phase == ENEMY_PHASE &&
						(
							((property & Room.UP) > 0) && ((property & Room.M_UP) > 0) ||
							((property & Room.RIGHT) > 0) && ((property & Room.M_RIGHT) > 0) ||
							((property & Room.DOWN) > 0) && ((property & Room.M_DOWN) > 0) ||
							((property & Room.LEFT) > 0) && ((property & Room.M_LEFT) > 0) ||
							((property & Room.GENERATOR) > 0) && ((property & Room.ATTACK) > 0)
						)
					);
				} else if((property & Room.GENERATOR) > 0){
					if(!((property & Room.ATTACK) > 0)) displace = false;
				}
				blit = getPropertyBlit(
					property,
					((property & Room.TURNER) > 0) && state == ANIMATE && phase == ENEMY_PHASE && !displace
				);
			} else if((property & Room.ALLY) > 0) {
				displace = phase == PLAYER_PHASE || ((property & Room.PUSHED) > 0);
				blit = renderer.allyBlit;
			} else if((property & Room.WALL) > 0) {
				if((property & Room.SWAP) > 0) {
					blit = renderer.swapBlit;
					displace = phase == PLAYER_PHASE;
				} else if((property & Room.INDESTRUCTIBLE) > 0){
					blit = renderer.indestructibleWallBlit;
				} else {
					blit = renderer.wallBlit;
				}
			} else if((property & Room.VOID) > 0) {
				blit = renderer.voidBlit;
			} else return;
			if(bitmapData == renderer.bitmapData){
				blit.x = (int)(renderer.canvasPoint.x + x);
				blit.y = (int)(renderer.canvasPoint.y + y);
			} else {
				blit.x = (int)x;
				blit.y = (int)y;
			}
			// to create a neat square without a shadow we copy straight to the shadow bitmap
			if((property & Room.VOID) > 0) {
				if(bitmapData == renderer.bitmapData){
					blit.render(renderer.bitmapDataShadow);
				} else {
					blit.render(bitmapData);
				}
				return;
			}
			if(state == ANIMATE && displace){
				// displace towards previous postion or nudge towards attacked position 
				int displaceStep = 0;
				if((property & (Room.ATTACK | Room.BLOCKED)) > 0) {
					if((property & Room.ATTACK) > 0){
						displaceStep = (int)(-restStep * (animCount + 1));
					} else if((property & Room.BLOCKED) > 0){
						displaceStep = (int)(-restStep * animCount);
					}
				} else {
					displaceStep = (int)(moveStep * animCount);
				}
				if((property & Room.UP) > 0) {
					blit.y += displaceStep;
				} else if((property & Room.RIGHT) > 0) {
					blit.x -= displaceStep;
				} else if((property & Room.DOWN) > 0) {
					blit.y -= displaceStep;
				} else if((property & Room.LEFT) > 0) {
					blit.x += displaceStep;
				}
			} else {
				if((property & Room.KILLER) > 0) {
					if((property & Room.UP) > 0) {
						blit.y -= (int)(SCALE * 0.5);
					} else if((property & Room.RIGHT) > 0) {
						blit.x += (int)(SCALE * 0.5);
					} else if((property & Room.DOWN) > 0) {
						blit.y += (int)(SCALE * 0.5);
					} else if((property & Room.LEFT) > 0) {
						blit.x -= (int)(SCALE * 0.5);
					}
				}
			}
			if((property & Room.ENEMY) > 0 && (property & Room.WALL) > 0) {
				renderer.enemyWallBlit.x = blit.x;
				renderer.enemyWallBlit.y = blit.y;
				renderer.enemyWallBlit.render(bitmapData);
			}
			if(blit is BlitClip) frame = (blit as BlitClip).frame;
			
			blit.render(bitmapData, frame);
			
			if((property & Room.ENEMY) > 0){
				if((property & Room.GENERATOR) > 0){
					renderer.generatorBlit.x = blit.x;
					renderer.generatorBlit.y = blit.y;
					renderer.generatorBlit.render(bitmapData);
					if(((property & Room.TIMER_1) > 0) && !blink){
						renderer.generatorWarningBlit.x = blit.x;
						renderer.generatorWarningBlit.y = blit.y;
						renderer.generatorWarningBlit.render(bitmapData);
					}
				}
				if(room.type == Room.ADVENTURE && ((property & Room.DOOR) > 0)){
					if((property & (Room.ENDING | Room.INCREMENT)) != 0){
						renderer.incrementBlit.x = blit.x;
						renderer.incrementBlit.y = blit.y;
						renderer.incrementBlit.render(bitmapData, game.frameCount % renderer.incrementBlit.totalFrames);
						renderer.endingDistBlit.x = blit.x;
						renderer.endingDistBlit.y = blit.y;
						if((property & Room.M_UP) > 0) renderer.endingDistBlit.y -= (int)SCALE;
						else if((property & Room.M_RIGHT) > 0) renderer.endingDistBlit.x += (int)SCALE;
						else if((property & Room.M_DOWN) > 0) renderer.endingDistBlit.y += (int)SCALE;
						else if((property & Room.M_LEFT) > 0) renderer.endingDistBlit.x -= (int)SCALE;
						renderer.endingDistBlit.render(bitmapData, room.endingDist - 2);
					}
				}
			}
			if((property & Room.BOMB) == Room.BOMB){
				renderer.bombBlit.x = blit.x;
				renderer.bombBlit.y = blit.y;
				renderer.bombBlit.render(bitmapData);
			}
			// render parity if checkView
			if((property & Room.ENEMY) == Room.ENEMY){
				if(
					state != ANIMATE && renderCheck && checkView && state == IDLE &&
					!((property & (Room.TURNER | Room.GENERATOR | Room.VIRUS | Room.TRAP | Room.DOOR)) > 0) &&
					(((int)(data.player.x + data.player.y * data.width)) & 1) == 
					(((int)(x * INV_SCALE + y * INV_SCALE * data.width) >> 0) & 1)
				){
					renderer.checkMarkBlit.x = blit.x;
					renderer.checkMarkBlit.y = blit.y;
					renderer.checkMarkBlit.render(bitmapData);
				}
			}
		}
		
		/* Get the correct frame for when the turner is facing a direction or turning to face it */
		public int getTurnerFrame(int property, Boolean rotate = false) {
			if((property & Room.UP) > 0){
				if(rotate){
					if((property & Room.M_UP) > 0) return TURNER_N;
					else if((property & Room.M_RIGHT) > 0) return TURNER_NE;
					else if((property & Room.M_DOWN) > 0) return TURNER_W;
					else if((property & Room.M_LEFT) > 0) return TURNER_NW;
				}
				return TURNER_N;
			} else if((property & Room.RIGHT) > 0){
				if(rotate){
					if((property & Room.M_RIGHT) > 0) return TURNER_E;
					else if((property & Room.M_DOWN) > 0) return TURNER_SE;
					else if((property & Room.M_LEFT) > 0) return TURNER_S;
					else if((property & Room.M_UP) > 0) return TURNER_NE;
				}
				return TURNER_E;
			} else if((property & Room.DOWN) > 0){
				if(rotate){
					if((property & Room.M_DOWN) > 0) return TURNER_S;
					else if((property & Room.M_LEFT) > 0) return TURNER_SW;
					else if((property & Room.M_UP) > 0) return TURNER_E;
					else if((property & Room.M_RIGHT) > 0) return TURNER_SE;
				}
				return TURNER_S;
			} else if((property & Room.LEFT) > 0){
				if(rotate){
					if((property & Room.M_LEFT) > 0) return TURNER_W;
					else if((property & Room.M_UP) > 0) return TURNER_NW;
					else if((property & Room.M_RIGHT) > 0) return TURNER_N;
					else if((property & Room.M_DOWN) > 0) return TURNER_SW;
				}
				return TURNER_W;
			}
			return 0;
		}
		
		public void render() {
			moveStep = SCALE / animDelay;
			restStep = moveStep * 0.5;
			int fromX = (int)(-renderer.canvasPoint.x * Game.INV_SCALE);
			int fromY = (int)(-renderer.canvasPoint.y * Game.INV_SCALE);
			int toX = (int)(1 + fromX + Game.WIDTH * Game.INV_SCALE);
			int toY = (int)(1 + fromY + Game.HEIGHT * Game.INV_SCALE);
			int property, i, r, c;
			Point player, p;
			Array<Point> entities = new Array<Point>();
			var renderMap = data.map;
			
			for(r = fromY; r < toY; r++){
				for(c = fromX; c < toX; c++){
					if(
						(c >= 0 && r >= 0 && c < data.width && r < data.height)
					){
						if(blackOutMap != null && uiManager.active && blackOutMap[r][c] == 0){
							renderProperty(c * SCALE, r * SCALE, Room.VOID, renderer.bitmapData);
							
						} else if(renderMap[r][c] != 0){
							property = renderMap[r][c];
							// always render the allies last
							if((property & Room.ALLY) > 0) {
								entities.unshift(new Point(c, r));
								continue;
							} else if((property & Room.ENEMY) > 0){
								entities.push(new Point(c, r));
								continue;
							}

							renderProperty(c * SCALE, r * SCALE, property, renderer.bitmapData);
							
						} else if(checkView){
							if(data.getCheck(c, r)){
								renderer.checkMarkBlit.x = (int)(renderer.canvasPoint.x + c * Game.SCALE);
								renderer.checkMarkBlit.y = (int)(renderer.canvasPoint.y + r * Game.SCALE);
								renderer.checkMarkBlit.render(renderer.guiBitmapData);
							}
						}
					}
				}
			}
			
			// render objects above the map so blocked movement goes over the walls
			for(i = entities.length - 1; i > -1 ; i--){
				p = entities[i];
				property = renderMap[(int)p.y][(int)p.x];
				// blink the player when in check and in checkView
				if(((property & Room.PLAYER) > 0) && checkView && !blink && data.getCheck((int)p.x, (int)p.y)){
					renderer.checkMarkBlit.x = (int)(renderer.canvasPoint.x + p.x * Game.SCALE);
					renderer.checkMarkBlit.y = (int)(renderer.canvasPoint.y + p.y * Game.SCALE);
					renderer.checkMarkBlit.render(renderer.guiBitmapData);
				} else {
					renderProperty(p.x * SCALE, p.y * SCALE, property, renderer.bitmapData);
				}
			}
			
			//renderPathMap();
			
			// gui render
			if(uiManager.active){
				renderer.turnsBlit.x = renderer.turnsBlit.y = 2;
				renderer.turnsBlit.render(renderer.guiBitmapData);
				renderer.numberBlit.x = renderer.turnsBlit.x + 2;
				renderer.numberBlit.y = renderer.turnsBlit.y + 2;
				renderer.numberBlit.setTargetValue(data.food);
				renderer.numberBlit.update();
				renderer.numberBlit.renderNumbers(renderer.guiBitmapData);
				if(uiManager.currentGroup > 0){
					//renderer.guiBitmapData.copyPixels(renderer.darkBitmapData, renderer.darkBitmapData.rect, new Point(), null, null, true);
                    renderer.guiBitmapData.fillRect(renderer.darkBitmapData.rect, renderer.darkBitmapDataColor);

					if(room.type == Room.ADVENTURE){
                        //CONVERSION
						//renderer.guiBitmapData.copyPixels(scoreTextBox.bitmapData, scoreTextBox.bitmapData.rect, new Point(0, Game.HEIGHT - Game.SCALE * 3), null, null, true);
                        renderer.guiBitmapData.copyPixels(scoreTextBox.text, scoreTextBox.bitmapData.rect, new Point(scoreTextBox._position.x, Game.HEIGHT - Game.SCALE * 3), null, null, true);
					}
				}
				uiManager.render(renderer.guiBitmapData);
			}
		}
		
		/* For debugging pathMap errors - needs rewriting a bit */
		public void renderPathMap() {
			int r, c;
			var size = LevelData.pathMap.length;
			var rect = new Rectangle(0, 0, 3, 3);
			uint fill;
			//trace(renderer.canvas.x);
			for(r = 0; r < size; r++){
				for(c = 0; c < size; c++){
					//rect.x = -renderer.canvasPoint.x + ((data.player.x - LevelData.ENEMY_ACTIVE_RADIUS) * SCALE) + c * SCALE;
					//rect.y = -renderer.canvasPoint.y + ((data.player.y - LevelData.ENEMY_ACTIVE_RADIUS) * SCALE) + r * SCALE;
					//rect.width = LevelData.pathMap[r][c];
					//renderer.bitmapData.fillRect(rect, 0xFFFFFF00);
				}
				
			}
		}
		
        private static Texture2D _levelTexture;
        private readonly static BitmapData _levelBitmapData = new BitmapData(13, 13, true, 0xFF282828); 
        private static Array<Array<int>> _previousMap;
        public static void LoadContent(ContentManager contentManager) {
            _levelTexture = new Texture2D(XnaGame.Instance.GraphicsDevice, 13, 13);
            //var pixelData = new Color[1];
            //_levelTexture.GetData(pixelData);
            //pixelData[0] = Color.White;
            //_levelTexture.SetData(pixelData);

            _levelBitmapData.texture = _levelTexture;

        }
        
		/* Creates a minimap of a level (used to preview levels in editor mode) */
		public static BitmapData getLevelBitmapData(Array<Array<int>> map, int width, int height) {
            
            if (_previousMap != null && map == _previousMap) 
                return _levelBitmapData;

            int r, c;
            uint col;
            int property;
            var bitmapData =_levelBitmapData;
            for(r = 0; r < height; r++){
                for(c = 0; c < width; c++){
                    property = map[r][c];
                    if((property & Room.ENEMY) != 0) {
                        bitmapData.setPixel32(c, r, 0xFFFFFFFF);
                    } else if((property & Room.WALL) != 0) {
                        bitmapData.setPixel32(c, r, Renderer.WALL_COL);
                    } else if((property & Room.ALLY) != 0) {
                        bitmapData.setPixel32(c, r, Renderer.UI_COL);
                    } else if((c + r * width & 1) != 0){
                        bitmapData.setPixel32(c, r, 0xFF3A3A3A);
                    } else {
                        bitmapData.setPixel32(c, r, 0xFF282828);
                    }
                }
            }

            bitmapData.setData();

            _previousMap = map;
            return bitmapData;
		}
	}
}
