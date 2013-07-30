using System;
using com.robotacid.engine;
using com.robotacid.ui;
using com.robotacid.ui.editor;
using flash;
using flash.display;
using flash.geom;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Array = flash.Array;
using Math = System.Math;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

//using Math = System.Math;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Renders an animation to a BitmapData using a series of references to animation frames
	/// on a sprite sheet
	/// 
	/// @author Aaron Steed, robotacid.com
	/// @conversion Paul Cunningham, pumpkin-games.net
    /// </summary>
    public class Renderer  {

        //Some render targets to replace the bitmaps
        static RenderTarget2D _defaultRenderTarget;
        static RenderTarget2D _backgroundRenderTarget;
        static RenderTarget2D _gameRenderTarget;
        static RenderTarget2D _shadowRenderTarget;
        static RenderTarget2D _guiRenderTarget;

        public Game game;
		public CanvasCamera camera;

        // gfx holders
		public Sprite canvas;
		public Point canvasPoint;
		public Boolean mouseScroll;
		public BitmapData bitmapData;
		public BitmapData bitmapDataShadow;
		public BitmapData guiBitmapData;
		public Shape bitmap;
		public Shape guiBitmap;
		public Shape bitmapShadow;
		public Shape backgroundShape;
		public BitmapData backgroundBitmapData;
		public BitmapData darkBitmapData;
        public uint darkBitmapDataColor = 0xaa000000;
		public TextBox exhibitTimeOut;
		
		// sprite sheets
		public static Texture2D GameSpriteSheet;
		public static Texture2D MenuSpriteSheet;
		public BitmapData gameSpriteSheet;
		public BitmapData menuSpriteSheet;

        public static Texture2D BackgroundSpriteSheet;
        public BitmapData backgroundSpriteSheet;
		
		// blits
		public BlitRect sparkBlit;
		public BlitSprite wallBlit;
		public BlitSprite indestructibleWallBlit;
		public BlitSprite playerBlit;
		public BlitSprite playerBuffer;
		public BlitSprite allyBlit;
		public BlitSprite moverBlit;
		public BlitSprite horizMoverBlit;
		public BlitSprite vertMoverBlit;
		public BlitClip turnerBlit;
		public BlitSprite virusBlit;
		public BlitRect debrisBlit;
		public BlitRect wallDebrisBlit;
		public BlitSprite enemyWallBlit;
		public BlitClip trapBlit;
		public BlitRect errorBlit;
		public BlitRect swapBlit;
		public BlitSprite generatorBlit;
		public BlitRect generatorWarningBlit;
		public BlitSprite turnsBlit;
		public NumberBlit numberBlit;
		public BlitRect notCompletedBlit;
		public BlitClip timerCountBlit;
		public BlitSprite checkMarkBlit;
		public BlitSprite propertySelectedBlit;
		public BlitSprite roomPaletteBlit;
		public BlitSprite parityBlit;
		public BlitRect voidBlit;
		public BlitClip doorBlit;
		public BlitSprite bombBlit;
		public BlitClip explosionBlit;
		public Array<FadingBlitRect> mapFadeBlits;
		public BlitClip paintBlit;
		public BlitSprite lockedBlit;
		public BlitSprite slideFade;
		public BlitClip incrementBlit;
		public BlitClip decrementBlit;
		public BlitClip endingDistBlit;
		
		public BlitClip puzzleButtonBlit;
		public BlitClip adventureButtonBlit;
		public BlitClip editorButtonBlit;
		public BlitClip checkButtonBlit;
		public BlitClip settingsButtonBlit;
		public BlitClip playButtonBlit;
		public BlitClip scrollButtonBlit;
		public BlitClip propertyButtonBlit;
		public BlitClip loadButtonBlit;
		public BlitClip saveButtonBlit;
		public BlitClip cancelButtonBlit;
		public BlitClip confirmButtonBlit;
		public BlitClip resetButtonBlit;
		public BlitClip controlsButtonBlit;
		public BlitClip orientationButtonBlit;
		public BlitClip soundButtonBlit;
		public BlitClip fullscreenButtonBlit;
		public BlitClip editButtonBlit;
		public BlitClip numberButtonBlit;
		public BlitClip leftButtonBlit;
		public BlitClip rightButtonBlit;
		public BlitSprite confirmPanelBlit;
		public BlitSprite levelMovePanelBlit;
		public BlitSprite levelPreviewPanelBlit;
		public BlitClip swapButtonBlit;
		public BlitClip insertBeforeButtonBlit;
		public BlitClip insertAfterButtonBlit;
		public BlitSprite whiteBlit;
		
		// self maintaining animations
		public Array<FX> fx;
		public Array<FX> roomFx;
		public Func<FX, int, Array<FX>, bool> fxFilterCallBack;
		
		// states
		public GlitchMap glitchMap;
		public Point shakeOffset;
		public int shakeDirX;
		public int shakeDirY;
		public Boolean trackPlayer;
		public Boolean refresh;
		public double slideX;
		public double slideY;
		public Boolean sliding;
		
		// temp variables
		private int i;
		
		public static Point point = new Point();
		//CONVERSION - don't need this anymore
        //public static var matrix:Matrix = new Matrix();
		
		// measurements from Game.as
		public readonly static double SCALE = Game.SCALE;
		public readonly static double INV_SCALE = Game.INV_SCALE;
		public readonly static double WIDTH = Game.WIDTH;
		public readonly static double HEIGHT = Game.HEIGHT;
		
		public const int SHAKE_DIST_MAX = 12;
		public readonly static double INV_SHAKE_DIST_MAX = 1.0 / SHAKE_DIST_MAX;
		public readonly static ColorTransform WALL_COL_TRANSFORM = new ColorTransform(1, 1, 1, 1, -100, -100, -100);
		public static ColorTransform WHITE_COL_TRANSFORM = new ColorTransform(1, 1, 1, 1, 255, 255, 255);
		public const uint WALL_COL = 0xff9b9b9b;
		public readonly static ColorTransform UI_COL_TRANSFORM = new ColorTransform(1, 1, 1, 1, -255 + 214, -255 + 232);
		public readonly static ColorTransform UI_COL_BORDER_TRANSFORM = new ColorTransform(1, 1, 1, 1, -255 + 166, -255 + 198, -255 + 239);
		public readonly static ColorTransform UI_COL_BACK_TRANSFORM = new ColorTransform(1, 1, 1, 1, -255 + 49, -255 + 59, -255 + 73);
		public const uint UI_COL = 0xffd6e8ff;
		public const uint UI_COL_BORDER = 0xffa6c6ef;
		public const uint UI_COL_BACK = 0xff313b49;
		public readonly static Array<double> DEBRIS_SPEEDS = new Array<double> { 1.5, 1, 1.5, 2, 3, 2.5, 2, 1.5, 1 };

        public Renderer(Game game) {
            this.game = game;
            trackPlayer = true;
        }

        /// <summary>
        /// Xna bridge to load graphics recources
        /// </summary>
        /// <param name="content">Xna Content Manager.</param>
        public static void LoadContent(ContentManager content) {
 
            BackgroundSpriteSheet = content.Load<Texture2D>("textures/background");
		    GameSpriteSheet = content.Load<Texture2D>("textures/game-sprites");
            MenuSpriteSheet = content.Load<Texture2D>("textures/menu-sprites");

            PresentationParameters pp = XnaGame.Instance.GraphicsDevice.PresentationParameters;
            SurfaceFormat format = pp.BackBufferFormat;

            _defaultRenderTarget = new RenderTarget2D(XnaGame.Instance.GraphicsDevice, (int)Game.WIDTH, (int)Game.HEIGHT, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.PreserveContents);
            _backgroundRenderTarget = new RenderTarget2D(XnaGame.Instance.GraphicsDevice, (int)Game.WIDTH, (int)Game.HEIGHT, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.PreserveContents);
            _gameRenderTarget = new RenderTarget2D(XnaGame.Instance.GraphicsDevice, (int)Game.WIDTH, (int)Game.HEIGHT, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.PreserveContents);
            _shadowRenderTarget = new RenderTarget2D(XnaGame.Instance.GraphicsDevice, (int)Game.WIDTH, (int)Game.HEIGHT, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.PreserveContents);
            _guiRenderTarget = new RenderTarget2D(XnaGame.Instance.GraphicsDevice, (int)Game.WIDTH, (int)Game.HEIGHT, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.PreserveContents);

            XnaGame.Instance.FlashRenderer.Register(_defaultRenderTarget);
            XnaGame.Instance.FlashRenderer.Register(_backgroundRenderTarget);
            XnaGame.Instance.FlashRenderer.Register(_gameRenderTarget);
            XnaGame.Instance.FlashRenderer.Register(_shadowRenderTarget);
            XnaGame.Instance.FlashRenderer.Register(_guiRenderTarget);
        }

        /* Prepares sprites and bitmaps for a game session */
		public void createRenderLayers(Sprite holder = null) {
			
			if(holder == null) holder = game;
			
			canvasPoint = new Point();
			canvas = new Sprite();
			holder.addChild(canvas);
		
			backgroundShape = new Shape();
			backgroundBitmapData = new BitmapData(16, 16, true, 0x0);

            //CONVERSION - simply made this a texture in the Content project
			//backgroundBitmapData.copyPixels(gameSpriteSheet, new Rectangle(0, 0, 16, 16), new Point());
			
			bitmapData = new BitmapData((int)WIDTH, (int)HEIGHT, true, 0x0);
			bitmap = new Shape();
			bitmapDataShadow = bitmapData.clone();
			bitmapShadow = new Shape();
			guiBitmapData = new BitmapData((int)WIDTH, (int)HEIGHT, true, 0x0);
			guiBitmap = new Shape();
			
			canvas.addChild(backgroundShape);
			canvas.addChild(bitmapShadow);
			canvas.addChild(bitmap);
			game.addChild(guiBitmap);
			
			fx = new Array<FX>();
			roomFx = new Array<FX>();
			
			camera = new CanvasCamera(canvasPoint, this);
			
			shakeOffset = new Point();
			shakeDirX = 0;
			shakeDirY = 0;
			slideX = 0;
			slideY = 0;
			sliding = false;
			refresh = true;

            //backgroundBitmapData.renderTarget = _backgroundRenderTarget;
            bitmapData.renderTarget = _gameRenderTarget;
            bitmapDataShadow.renderTarget = _shadowRenderTarget;
            guiBitmapData.renderTarget = _guiRenderTarget;
		}

        /* Destroy all objects */
		public void clearAll() {
			while(canvas.numChildren > 0){
				canvas.removeChildAt(0);
			}
			bitmap = null;
			bitmapShadow = null;
			bitmapData.dispose();
			bitmapData = null;
			fx = null;
			game = null;
		}

        /* Clean graphics and reset camera - no object destruction/creation */
		public void reset() {
            //CONVERSION - no action here
			//bitmapData.fillRect(bitmapData.rect, 0x0);
			//bitmapDataShadow.fillRect(bitmapData.rect, 0x0);
			//guiBitmapData.fillRect(bitmapData.rect, 0x0);
            //backgroundShape.graphics.clear();
			glitchMap.reset();
			var data = game.level.data;
			camera.mapRect = new Rectangle(0, 0, data.width * SCALE, data.height * SCALE);
			camera.setTarget((data.player.x + 0.5) * SCALE, (data.player.y + 0.5) * SCALE);
			camera.skipPan();
			fx.length = 0;
			slideX = slideY = 0;
			sliding = false;
			refresh = true;
			trackPlayer = true;
			if(exhibitTimeOut != null){
				exhibitTimeOut.alpha = 0;
			}
		}

        /* ================================================================================================
		 * MAIN
		 * Updates all of the rendering 
		 * ================================================================================================
		 */
		public void main() {
            //This is now a NOP - rendering is done in the main method below.
            //We keep this around to minimise impact on original source.
        }

        internal void main(RenderTarget2D sceneRenderTarget)
        {
            // clear bitmapDatas - refresh can be set to false for glitchy trails
            //if(refresh) bitmapData.fillRect(bitmapData.rect, 0x0);
            //bitmapDataShadow.fillRect(bitmapDataShadow.rect, 0x0);
            //guiBitmapData.fillRect(guiBitmapData.rect, 0x0);
			
            XnaGame.Instance.FlashRenderer.RenderTarget = _defaultRenderTarget;
            XnaGame.Instance.GraphicsDevice.Clear(Color.Transparent);

			if(game.state == Game.MENU){
				guiBitmapData.fillRect(guiBitmapData.rect, 0xFF1A1E26);
				game.titleMenu.render();
				canvasPoint.x -= 0.5;
				
			} 
            else if(game.state == Game.GAME){

                updateShaker();
				
                Level level;
                level = game.level;
				
                if(trackPlayer){
                    camera.setTarget(
                        (level.data.player.x + 0.5) * SCALE,
                        (level.data.player.y + 0.5) * SCALE
                    );
                } else if(sliding){
                    camera.targetPos.x += slideX;
                    camera.targetPos.y += slideY;
                    slideFade.render(bitmapData);
                }
				
                camera.main();
				
                updateCheckers();
				
                // black border around small levels
                if(canvasPoint.x > camera.mapRect.x){
                    bitmapDataShadow.fillRect(new Rectangle(0, 0, canvasPoint.x, Game.HEIGHT), 0xFF000000);
                }
                if(canvasPoint.x + camera.mapRect.x + camera.mapRect.width < Game.WIDTH){
                    bitmapDataShadow.fillRect(new Rectangle(canvasPoint.x + camera.mapRect.x + camera.mapRect.width, 0, Game.WIDTH - (canvasPoint.x + camera.mapRect.x + camera.mapRect.width), Game.HEIGHT), 0xFF000000);
                }
                if(canvasPoint.y > 0){
                    bitmapDataShadow.fillRect(new Rectangle(0, 0, Game.WIDTH, canvasPoint.y), 0xFF000000);
                }
                if(canvasPoint.y + camera.mapRect.height < Game.HEIGHT){
                    bitmapDataShadow.fillRect(new Rectangle(0, canvasPoint.y + camera.mapRect.height, Game.WIDTH, Game.HEIGHT - (canvasPoint.y + camera.mapRect.height)), 0xFF000000);
                }
				
                if(game.roomPainter.active) game.roomPainter.render();
                level.render();
                if(game.roomPainter.active) game.roomPainter.palette.render();
				
                if(fx.length > 0) fx = fx.filter(fxFilterCallBack);
				
                glitchMap.apply(bitmapData, (int)canvasPoint.x, (int)canvasPoint.y);
                glitchMap.update();
				
                //CONVERSION - handled below
                //bitmapDataShadow.copyPixels(bitmapData, bitmapData.rect, new Point(1, 1), null, null, true);
                //bitmapDataShadow.colorTransform(bitmapDataShadow.rect, new ColorTransform(0, 0, 0));
				
                if(roomFx.length > 0) roomFx = roomFx.filter(fxFilterCallBack);
            }

            // update shapes
            //CONVERION
            // Now draw the deferred scene object applying whatever effects are required to mimic the orignal game
            XnaGame.Instance.GraphicsDevice.SetRenderTarget(_shadowRenderTarget);
            if(refresh) XnaGame.Instance.GraphicsDevice.Clear(Color.Transparent);
            //XnaGame.Instance.GraphicsDevice.Clear(Color.Transparent);
            XnaGame.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            XnaGame.Instance.FlashRenderer.Flush(XnaGame.Instance.SpriteBatch, _shadowRenderTarget);
            XnaGame.Instance.FlashRenderer.Draw(XnaGame.Instance.SpriteBatch, _gameRenderTarget, Vector2.One, Color.Black);
            XnaGame.Instance.SpriteBatch.End();
            
            XnaGame.Instance.GraphicsDevice.SetRenderTarget(_gameRenderTarget);
            if(refresh) XnaGame.Instance.GraphicsDevice.Clear(Color.Transparent);
            XnaGame.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            XnaGame.Instance.FlashRenderer.Flush(XnaGame.Instance.SpriteBatch, _gameRenderTarget);
            XnaGame.Instance.SpriteBatch.End();
            
            XnaGame.Instance.GraphicsDevice.SetRenderTarget(_guiRenderTarget);
            XnaGame.Instance.GraphicsDevice.Clear(Color.Transparent);
            XnaGame.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            XnaGame.Instance.FlashRenderer.Flush(XnaGame.Instance.SpriteBatch, _guiRenderTarget);
            XnaGame.Instance.SpriteBatch.End();

            XnaGame.Instance.GraphicsDevice.SetRenderTarget(_defaultRenderTarget);
            XnaGame.Instance.GraphicsDevice.Clear(Color.Transparent);
            XnaGame.Instance.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            XnaGame.Instance.FlashRenderer.Flush(XnaGame.Instance.SpriteBatch, null);
            XnaGame.Instance.SpriteBatch.End();

            //Draw final composite image - we'll draw to another render target first - the XNA wrapper will blit this final render target
            //to the screen later respecting title safe viewport.
            //Note - it could all be done here if you wanted but the game wrapper was already set up to do it.
            XnaGame.Instance.GraphicsDevice.SetRenderTarget(sceneRenderTarget);
            XnaGame.Instance.GraphicsDevice.Clear(Color.Transparent);
            XnaGame.Instance.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            XnaGame.Instance.SpriteBatch.Draw(_backgroundRenderTarget, Vector2.Zero, XnaGame.Instance.GraphicsDevice.Viewport.Bounds, Color.White);
            XnaGame.Instance.SpriteBatch.Draw(_shadowRenderTarget, Vector2.Zero, XnaGame.Instance.GraphicsDevice.Viewport.Bounds, Color.White);
            XnaGame.Instance.SpriteBatch.Draw(_gameRenderTarget, Vector2.Zero, XnaGame.Instance.GraphicsDevice.Viewport.Bounds, Color.White);
            XnaGame.Instance.SpriteBatch.Draw(_guiRenderTarget, Vector2.Zero, XnaGame.Instance.GraphicsDevice.Viewport.Bounds, Color.White);
            XnaGame.Instance.SpriteBatch.Draw(_defaultRenderTarget, Vector2.Zero, XnaGame.Instance.GraphicsDevice.Viewport.Bounds, Color.White);
            XnaGame.Instance.SpriteBatch.End();
        }

        private void updateCheckers() {
            // checker background
            //CONVERSION
            XnaGame.Instance.GraphicsDevice.SetRenderTarget(_backgroundRenderTarget);
            XnaGame.Instance.GraphicsDevice.Clear(Color.Transparent);
            XnaGame.Instance.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, null,null);
            XnaGame.Instance.SpriteBatch.Draw(backgroundSpriteSheet.texture, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle((int)-canvasPoint.x, (int)-canvasPoint.y, (int)Game.WIDTH, (int)Game.HEIGHT), Color.White);
            XnaGame.Instance.SpriteBatch.End();
		}
		
		public void displace(double x, double y) {
			int i;
            FX item;
			for(i = 0; i < fx.length; i++){
				item = fx[i];
				item.x += x;
				item.y += y;
			}
			camera.displace(x, y);
		}
		
		public void setSlide(double x, double y) {
			slideX = x;
			slideY = y;
			int i;
            FX item;
			for(i = 0; i < fx.length; i++){
				item = fx[i];
				if(item.dir != null){
					if(slideX > 0){
						item.dir.y = 0;
					} else if(slideY > 0){
						item.dir.x = 0;
					}
				}
			}
			if(x == 0){
				camera.targetPos.x = camera.canvas.x;
			}
			if(y == 0){
				camera.targetPos.y = camera.canvas.y;
			}
			sliding = true;
		}
		
		/* Shake the screen in any direction */
		public void shake(int x, int y, Point shakeSource = null) {
			if(!refresh) return;
			// sourced shakes drop off in intensity by distance
			// it stops the player feeling like they're in a cocktail shaker
			if(shakeSource != null){
				double dist = Math.Abs(game.level.data.player.x - shakeSource.x) + Math.Abs(game.level.data.player.x - shakeSource.y);
				if(dist >= SHAKE_DIST_MAX) return;
				x = x * (int)((SHAKE_DIST_MAX - dist) * INV_SHAKE_DIST_MAX);
				y = y * (int)((SHAKE_DIST_MAX - dist) * INV_SHAKE_DIST_MAX);
				if(x == 0 && y == 0) return;
			}
			// ignore lesser shakes
			if(Math.Abs(x) < Math.Abs(shakeOffset.x)) return;
			if(Math.Abs(y) < Math.Abs(shakeOffset.y)) return;
			shakeOffset.x = x;
			shakeOffset.y = y;
			shakeDirX = x > 0 ? 1 : -1;
			shakeDirY = y > 0 ? 1 : -1;
		}
		
		/* resolve the shake */
		private void updateShaker() {
			// shake first
			if(shakeOffset.y != 0){
				shakeOffset.y = -shakeOffset.y;
				if(shakeDirY == 1 && shakeOffset.y > 0) shakeOffset.y--;
				if(shakeDirY == -1 && shakeOffset.y < 0) shakeOffset.y++;
			}
			if(shakeOffset.x != 0){
				shakeOffset.x = -shakeOffset.x;
				if(shakeDirX == 1 && shakeOffset.x > 0) shakeOffset.x--;
				if(shakeDirX == -1 && shakeOffset.x < 0) shakeOffset.x++;
			}
		}
		
		/* Add to list */
		public FX addFX(double x, double y, BlitRect blit, Point dir = null, int delay = 0, Boolean push = true, Boolean looped = false, Boolean killOffScreen = true, Boolean room = false) {
			var item = new FX(x, y, blit, bitmapData, canvasPoint, dir, delay, looped, killOffScreen);
			if(room){
				if(push) roomFx.push(item);
				else roomFx.unshift(item);
			} else {
				if(push) fx.push(item);
				else fx.unshift(item);
			}
			return item;
		}
		
		/* Cyclically throw off pixel debris from where white pixels used to be on the blit
		 * use dir to specify bitwise flags for directions able to throw debris in */
		public void bitmapDebris(BlitSprite blit, int x, int y, int dir = 15) {
            //TODO
            int r, c;
            var blitClip = blit as BlitClip;
            var source = blitClip != null ? blitClip.frames[blitClip.frame] : blit.rect;
            var compassIndex = 0;
            var speedIndex = 0;
            Point compassPoint;
            var p = new Point();
            double debrisSpeed;
            uint u;
            for(r = 0; r < source.height; r++){
                for(c = 0; c < source.width; c++){
                    u = blit.spriteSheet.getPixel32((int)source.x + c, (int)source.y + r);
                    if(u == 0xFFFFFFFF || u == WALL_COL){
                    //u = 0xFFFFFFFF;
                        compassPoint = Room.compassPoints[compassIndex];
                        debrisSpeed = DEBRIS_SPEEDS[speedIndex];
                        p.x = compassPoint.x * debrisSpeed;
                        p.y = compassPoint.y * debrisSpeed;
                        if((Room.compass[compassIndex] & dir) > 0) addFX(x * SCALE + c + blit.dx, y * SCALE + r + blit.dy, u == 0xFFFFFFFF ? debrisBlit : wallDebrisBlit, p.clone(), 0, true, true);
                        speedIndex++;
                        compassIndex++;
                        if(compassIndex >= Room.compassPoints.length) compassIndex = 0;
                        if(speedIndex >= DEBRIS_SPEEDS.length) speedIndex = 0;
                    }
                }
            }
		}

        /* A check to see if (x,y) is on screen plus a border */
		public Boolean onScreen(double x, double y, double border) {
            return x + border >= -canvasPoint.x && y + border >= -canvasPoint.y && x - border < -canvasPoint.x + Game.WIDTH && y - border < -canvasPoint.y + Game.HEIGHT;
        }

        /* ===============================================================================================================
		 * 
		 *  INIT
		 * 
		 * Initialisation is separated from the constructor to allow reference paths to be complete before all
		 * of the graphics are generated - an object is null until its constructor has been exited
		 * 
		 * ===============================================================================================================
		 */
		public void init() {
			
			FX.renderer = this;
            //TODO
            //FoodClockFX.renderer = this;
			Level.renderer = this;
			TitleMenu.renderer = this;
			RoomPainter.renderer = this;
			RoomPalette.renderer = this;

            var gameSpriteSheetBitmap = GameSpriteSheet;
			var menuSpriteSheetBitmap = MenuSpriteSheet;

			//gameSpriteSheet = gameSpriteSheetBitmap.bitmapData;
            gameSpriteSheet = new BitmapData(GameSpriteSheet, 0, 0);//  menuSpriteSheetBitmap.bitmapData);
			menuSpriteSheet = new BitmapData(MenuSpriteSheet, 0, 0);//  menuSpriteSheetBitmap.bitmapData);
			backgroundSpriteSheet = new BitmapData(BackgroundSpriteSheet, 0, 0);//  menuSpriteSheetBitmap.bitmapData);

			sparkBlit = new BlitRect(0, 0, 1, 1, 0xffffffff);
			debrisBlit = new BlitRect(0, 0, 1, 1, 0xffffffff);
			wallDebrisBlit = new BlitRect(0, 0, 1, 1, WALL_COL);
			wallBlit = new BlitSprite(gameSpriteSheet, new Rectangle(24, 0, 8, 8));
			indestructibleWallBlit = new BlitSprite(gameSpriteSheet, new Rectangle(80, 0, 8, 8));
			enemyWallBlit = new BlitSprite(gameSpriteSheet, new Rectangle(88, 0, 8, 8));
			trapBlit = new BlitClip(gameSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 16, 8, 8),
				new Rectangle(8, 16, 8, 8),
				new Rectangle(16, 16, 8, 8),
				new Rectangle(24, 16, 8, 8),
				new Rectangle(32, 16, 8, 8),
				new Rectangle(40, 16, 8, 8),
				new Rectangle(48, 16, 8, 8),
				new Rectangle(56, 16, 8, 8),
				new Rectangle(64, 16, 8, 8),
				new Rectangle(72, 16, 8, 8),
				new Rectangle(80, 16, 8, 8),
				new Rectangle(88, 16, 8, 8),
				new Rectangle(96, 16, 8, 8),
				new Rectangle(104, 16, 8, 8),
				new Rectangle(0, 24, 8, 8)
			});
			generatorBlit = new BlitSprite(gameSpriteSheet, new Rectangle(96, 0, 8, 8));
			generatorWarningBlit = new BlitRect(0, 0, 7, 7, 0xFFFFFFFF);
			playerBlit = new BlitSprite(gameSpriteSheet, new Rectangle(16, 0, 8, 8));
			playerBuffer = new BlitSprite(gameSpriteSheet, new Rectangle(96, 64, 8, 8));
			allyBlit = new BlitSprite(gameSpriteSheet, new Rectangle(64, 0, 8, 8));
			moverBlit = new BlitSprite(gameSpriteSheet, new Rectangle(48, 0, 8, 8));
			horizMoverBlit = new BlitSprite(gameSpriteSheet, new Rectangle(40, 0, 8, 8));
			vertMoverBlit = new BlitSprite(gameSpriteSheet, new Rectangle(32, 0, 8, 8));
			turnerBlit = new BlitClip(gameSpriteSheet, new Array<Rectangle> {
				new Rectangle(16, 8, 8, 8),
				new Rectangle(24, 8, 8, 8),
				new Rectangle(32, 8, 8, 8),
				new Rectangle(40, 8, 8, 8),
				new Rectangle(48, 8, 8, 8),
				new Rectangle(56, 8, 8, 8),
				new Rectangle(64, 8, 8, 8),
				new Rectangle(72, 8, 8, 8)
			});
			virusBlit = new BlitSprite(gameSpriteSheet, new Rectangle(56, 0, 8, 8));
			errorBlit = new BlitRect(3, 3, 3, 3, 0xFFFF0000);
			swapBlit = new BlitSprite(gameSpriteSheet, new Rectangle(72, 0, 8, 8));
			numberBlit = new NumberBlit(gameSpriteSheet, new Rectangle(112, 0, 8, 80), 2, 0.25, -1, -1, 6);
			notCompletedBlit = new BlitRect( -1, -1, 3, 3, UI_COL_BORDER);
			timerCountBlit = new BlitClip(gameSpriteSheet, new Array<Rectangle> {
				new Rectangle(112, 0, 8, 8),
				new Rectangle(112, 8, 8, 8),
				new Rectangle(112, 16, 8, 8),
				new Rectangle(112, 24, 8, 8)
			});
			checkMarkBlit = new BlitSprite(gameSpriteSheet, new Rectangle(56, 32, 8, 8));
			voidBlit = new BlitRect(0, 0, (int)SCALE, (int)SCALE, 0xFF000000);
			doorBlit = new BlitClip(gameSpriteSheet, new Array<Rectangle> {
				new Rectangle(8, 24, 8, 8),
				new Rectangle(16, 24, 8, 8),
				new Rectangle(24, 24, 8, 8),
				new Rectangle(32, 24, 8, 8),
				new Rectangle(40, 24, 8, 8),
				new Rectangle(48, 24, 8, 8),
				new Rectangle(56, 24, 8, 8),
				new Rectangle(64, 24, 8, 8),
				new Rectangle(72, 24, 8, 8),
				new Rectangle(80, 24, 8, 8)
			});
			bombBlit = new BlitSprite(gameSpriteSheet, new Rectangle(104, 0, 8, 8));
			explosionBlit = new BlitClip(gameSpriteSheet, new Array<Rectangle> {
				new Rectangle(80, 8, 8, 8),
				new Rectangle(80, 8, 8, 8),
				new Rectangle(88, 8, 8, 8),
				new Rectangle(88, 8, 8, 8),
				new Rectangle(96, 8, 8, 8),
				new Rectangle(96, 8, 8, 8),
				new Rectangle(104, 8, 8, 8),
				new Rectangle(104, 8, 8, 8)
			});
			whiteBlit = new BlitSprite(gameSpriteSheet, new Rectangle(88, 64, 8, 8));
			errorBlit = new BlitRect(1, 1, 5, 5, 0xFFFF0000);
			incrementBlit = new BlitClip(gameSpriteSheet, new Array<Rectangle> {
				new Rectangle(88, 24, 8, 8),
				new Rectangle(88, 24, 8, 8),
				new Rectangle(96, 24, 8, 8),
				new Rectangle(96, 24, 8, 8),
				new Rectangle(104, 24, 8, 8),
				new Rectangle(104, 24, 8, 8),
				new Rectangle(0, 32, 8, 8),
				new Rectangle(0, 32, 8, 8),
				new Rectangle(8, 32, 8, 8),
				new Rectangle(8, 32, 8, 8),
				new Rectangle(16, 32, 8, 8),
				new Rectangle(16, 32, 8, 8),
				new Rectangle(24, 32, 8, 8),
				new Rectangle(24, 32, 8, 8),
				new Rectangle(32, 32, 8, 8),
				new Rectangle(32, 32, 8, 8),
				new Rectangle(40, 32, 8, 8),
				new Rectangle(40, 32, 8, 8),
				new Rectangle(48, 32, 8, 8),
				new Rectangle(48, 32, 8, 8),
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			});
			endingDistBlit = new BlitClip(gameSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 56, 8, 8),
				new Rectangle(8, 56, 8, 8),
				new Rectangle(16, 56, 8, 8),
				new Rectangle(24, 56, 8, 8),
				new Rectangle(32, 56, 8, 8),
				new Rectangle(40, 56, 8, 8),
				new Rectangle(48, 56, 8, 8),
				new Rectangle(56, 56, 8, 8),
				new Rectangle(64, 56, 8, 8),
				new Rectangle(72, 56, 8, 8)
			});
			settingsButtonBlit = new BlitClip(gameSpriteSheet, new Array<Rectangle> {
				new Rectangle(64, 32, 8, 8),
				new Rectangle(72, 32, 8, 8)
			});
			turnsBlit = new BlitSprite(menuSpriteSheet, new Rectangle(48, 60, 16, 10));
			propertySelectedBlit = new BlitSprite(menuSpriteSheet, new Rectangle(79, 88, 9, 9), -1, -1);
			roomPaletteBlit = new BlitSprite(menuSpriteSheet, new Rectangle(44, 72, 35, 67), -1, -1);
			
			puzzleButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 72, 22, 22),
				new Rectangle(22, 72, 22, 22)
			});
			adventureButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 94, 22, 22),
				new Rectangle(22, 94, 22, 22)
			});
			editorButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 116, 22, 22),
				new Rectangle(22, 116, 22, 22)
			});
			checkButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(48, 12, 12, 12),
				new Rectangle(60, 12, 12, 12),
				new Rectangle(72, 12, 12, 12),
				new Rectangle(84, 12, 12, 12)
			});
			playButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(48, 48, 12, 12),
				new Rectangle(60, 48, 12, 12)
			});
			propertyButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(72, 48, 12, 12),
				new Rectangle(84, 48, 12, 12)
			});
			scrollButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(24, 60, 12, 12),
				new Rectangle(36, 60, 12, 12),
				new Rectangle(79, 106, 12, 12),
				new Rectangle(79, 118, 12, 12)
			});
			loadButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 36, 12, 12),
				new Rectangle(12, 36, 12, 12)
			});
			saveButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(72, 24, 12, 12),
				new Rectangle(84, 24, 12, 12)
			});
			cancelButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 0, 12, 12),
				new Rectangle(12, 0, 12, 12)
			});
			confirmButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(24, 0, 12, 12),
				new Rectangle(36, 0, 12, 12)
			});
			resetButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 12, 12, 12),
				new Rectangle(12, 12, 12, 12)
			});
			controlsButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 48, 12, 12),
				new Rectangle(12, 48, 12, 12),
				new Rectangle(24, 48, 12, 12),
				new Rectangle(36, 48, 12, 12)
			});
			orientationButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(24, 12, 12, 12),
				new Rectangle(36, 12, 12, 12)
			});
			soundButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(24, 36, 12, 12),
				new Rectangle(36, 36, 12, 12),
				new Rectangle(48, 36, 12, 12),
				new Rectangle(36, 36, 12, 12)
			});
			fullscreenButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(84, 36, 12, 12),
				new Rectangle(72, 36, 12, 12),
				new Rectangle(60, 36, 12, 12),
				new Rectangle(72, 36, 12, 12)
			});
			editButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 60, 12, 12),
				new Rectangle(12, 60, 12, 12)
			});
			numberButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(48, 60, 16, 10),
				new Rectangle(64, 60, 16, 10)
			});
			leftButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(48, 0, 12, 12),
				new Rectangle(60, 0, 12, 12)
			});
			rightButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(72, 0, 12, 12),
				new Rectangle(84, 0, 12, 12)
			});
			swapButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(48, 24, 12, 12),
				new Rectangle(60, 24, 12, 12)
			});
			insertBeforeButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(0, 24, 12, 12),
				new Rectangle(12, 24, 12, 12)
			});
			insertAfterButtonBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(24, 24, 12, 12),
				new Rectangle(36, 24, 12, 12)
			});
			confirmPanelBlit = new BlitSprite(menuSpriteSheet, new Rectangle(0, 157, 31, 19));
			levelMovePanelBlit = new BlitSprite(menuSpriteSheet, new Rectangle(0, 139, 70, 18));
			levelPreviewPanelBlit = new BlitSprite(menuSpriteSheet, new Rectangle(79, 72, 16, 16));
			paintBlit = new BlitClip(menuSpriteSheet, new Array<Rectangle> {
				new Rectangle(79, 97, 9, 9),
				new Rectangle(80, 60, 9, 9)
			}, -1, -1);
			lockedBlit = new BlitSprite(menuSpriteSheet, new Rectangle(89, 60, 7, 7));
			
			int fade_delay = 10;
			mapFadeBlits = new Array<FadingBlitRect> {
				new FadingBlitRect(0, 0, (int)(Level.MAP_WIDTH * SCALE), (int)((Level.ROOM_HEIGHT - 1) * SCALE), fade_delay),
				new FadingBlitRect((int)(Level.ROOM_WIDTH * SCALE), 0, (int)((Level.ROOM_WIDTH - 1) * SCALE), (int)(Level.MAP_HEIGHT * SCALE), fade_delay),
				new FadingBlitRect(0, (int)(Level.ROOM_HEIGHT * SCALE), (int)(Level.MAP_WIDTH * SCALE), (int)((Level.ROOM_HEIGHT - 1) * SCALE), fade_delay),
				new FadingBlitRect(0, 0, (int)((Level.ROOM_WIDTH - 1) * SCALE), (int)(Level.MAP_HEIGHT * SCALE), fade_delay),
			};
			darkBitmapData = new BitmapData((int)Game.WIDTH, (int)Game.HEIGHT, true, 0x88000000);
			
			glitchMap = new GlitchMap();
			
			slideFade = new BlitSprite(new BitmapData((int)Game.WIDTH, (int)Game.HEIGHT, true, 0x08000000), new Rectangle(0, 0, Game.WIDTH, Game.HEIGHT));
			
			TextBox.init(new Array<Rectangle> { 
				new Rectangle(1, 40, 6, 7),// a
				new Rectangle(9, 40, 6, 7),// b
				new Rectangle(17, 40, 6, 7),// c
				new Rectangle(25, 40, 6, 7),// d
				new Rectangle(33, 40, 6, 7),// e
				new Rectangle(41, 40, 6, 7),// f
				new Rectangle(49, 40, 6, 7),// g
				new Rectangle(57, 40, 6, 7),// h
				new Rectangle(65, 40, 6, 7),// i
				new Rectangle(73, 40, 6, 7),// j
				new Rectangle(81, 40, 6, 7),// k
				new Rectangle(89, 40, 6, 7),// l
				new Rectangle(97, 40, 6, 7),// m
				new Rectangle(105, 40, 6, 7),// n
				new Rectangle(1, 48, 6, 7),// o
				new Rectangle(9, 48, 6, 7),// p
				new Rectangle(17, 48, 6, 7),// q
				new Rectangle(25, 48, 6, 7),// r
				new Rectangle(33, 48, 6, 7),// s
				new Rectangle(41, 48, 6, 7),// t
				new Rectangle(49, 48, 6, 7),// u
				new Rectangle(57, 48, 6, 7),// v
				new Rectangle(65, 48, 6, 7),// w
				new Rectangle(73, 48, 6, 7),// x
				new Rectangle(81, 48, 6, 7),// y
				new Rectangle(89, 48, 6, 7),// z
				new Rectangle(1, 56, 6, 7),// 0
				new Rectangle(9, 56, 6, 7),// 1
				new Rectangle(17, 56, 6, 7),// 2
				new Rectangle(25, 56, 6, 7),// 3
				new Rectangle(33, 56, 6, 7),// 4
				new Rectangle(41, 56, 6, 7),// 5
				new Rectangle(49, 56, 6, 7),// 6
				new Rectangle(57, 56, 6, 7),// 7
				new Rectangle(65, 56, 6, 7),// 8
				new Rectangle(73, 56, 6, 7),// 9
				new Rectangle(19, 64, 2, 4),// '
				new Rectangle(33, 64, 6, 7),// backslash
				new Rectangle(),// :
				new Rectangle(3, 64, 2, 8),// ,
				new Rectangle(73, 64, 6, 7),// =
				new Rectangle(99, 56, 2, 7),// !
				new Rectangle(81, 64, 6, 7),// /
				new Rectangle(41, 64, 6, 7),// -
				new Rectangle(58, 64, 3, 8),// (
				new Rectangle(49, 64, 6, 7),// +
				new Rectangle(89, 56, 6, 7),// ?
				new Rectangle(66, 64, 3, 8),// )
				new Rectangle(),// ;
				new Rectangle(107, 56, 2, 7),// .
				new Rectangle(8, 64, 8, 7),// @
				new Rectangle(),// _
				new Rectangle(81, 56, 6, 7),// %
				new Rectangle(),// *
				new Rectangle(26, 64, 4, 4)// "
			}, gameSpriteSheet);
			
			fxFilterCallBack = (FX item, int index, Array<FX> list) => {
				item.main();
				return item.active;
			};
        }
    }
}
