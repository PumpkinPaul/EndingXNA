using System;
using System.Threading;
using KiloWatt.Runtime.Support;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using com.robotacid.gfx;
using com.robotacid.sound;
using flash.display;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using pumpkin;

/// <summary>
/// This is the bridge between XNA and the Flash / Ending code
/// </summary>
public class XnaGame : Microsoft.Xna.Framework.Game
{
    public static XnaGame Instance { get; private set; }
     
    private readonly GraphicsDeviceManager _graphics;
    public StorageManager StorageManager { get; private set; }
    public ThreadPoolComponent ThreadPoolComponent { get; private set; }
    
    public SpriteFont SpriteFont { get; private set; }
    public SpriteBatch SpriteBatch { get; private set; }
    private RenderTarget2D _sceneRenderTarget;
    public FlashRenderer FlashRenderer { get; private set; }

    public bool CanDraw { get; set; }

    private Game _game;
    
    public static Stage Stage { get; private set; }
    public float Scale { get { return (float)Instance._game.scaleRatio; } }

    /// <summary></summary>
    protected PostProcess PostProcess;

    #if XBOX360
        public const float DefaultZoomFactor = 0.0f;
        public const int TitleSafeTestOffset = 0;
    #else
        public const float DefaultZoomFactor = 1.0f;
        public const int TitleSafeTestOffset = 50;
    #endif

    private float _zoomFactor = DefaultZoomFactor;

    public float ZoomFactor { 
        get { return this._zoomFactor; } 
        set { 
            this._zoomFactor = value;
            this.SetRenderViewport();
        } 
    }
    public Viewport RenderViewport { get; private set; }

    public XnaGame()
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        IsFixedTimeStep = true;

        _graphics.PreferredBackBufferWidth = 480;
        _graphics.PreferredBackBufferHeight = 320;

        _graphics.PreferredBackBufferWidth = 960;
        _graphics.PreferredBackBufferHeight = 640;

        //Make 30 FPS or put a key limiter on KeyDown!
        TargetElapsedTime = TimeSpan.FromSeconds(1/30.0f);

        StorageManager = new StorageManager("EndingXNA");
        StorageManager.ShowStorageGuide();
        StorageManager.StorageDeviceAction += (sender, e) => {
            if (e.DialogAction == DialogAction.Select)
                //UserData.loadSettingsFile(e.StorageContainer);
                Game.LoadFromUserStorage(e.StorageContainer);
        };
        

        FlashRenderer = new FlashRenderer();

        PostProcess = new PostProcess(this, null);
        PostProcess.AddProcessor(new BloomProcessor(this) { 
            Active = true, Settings = BloomProcessor.BloomSettings.PresetSettings[7]
        });
        PostProcess.AddProcessor(new BarrelDistortionProcessor(this) { 
            Active = true
        });
        PostProcess.AddProcessor(new ScanlinesProcessor(this) { 
            Active = true, ScanlinesValue = 0.25f
        });

        Components.Add(new GamerServicesComponent(this));
        Components.Add(ThreadPoolComponent = new ThreadPoolComponent(this));
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        Stage = new Stage();
        _game = new Game();

        Stage.addChild(_game);

        IsMouseVisible = true;

        PostProcess.Initialize();

        //Initialise the final rendering viewport - this will allow the user to size the rendered image to match their display device
        SetRenderViewport();
    }

    private void SetRenderViewport()
    {
        //Specify offset to test Titlesafe on Windows...
        RenderViewport = new Viewport(
                (int)MathHelper.Lerp(GraphicsDevice.Viewport.TitleSafeArea.X + TitleSafeTestOffset, GraphicsDevice.Viewport.X, ZoomFactor),
                (int)MathHelper.Lerp(GraphicsDevice.Viewport.TitleSafeArea.Y + TitleSafeTestOffset, GraphicsDevice.Viewport.Y, ZoomFactor),
                (int)MathHelper.Lerp(GraphicsDevice.Viewport.TitleSafeArea.Width - (TitleSafeTestOffset * 2), GraphicsDevice.Viewport.Width, ZoomFactor),
                (int)MathHelper.Lerp(GraphicsDevice.Viewport.TitleSafeArea.Height - (TitleSafeTestOffset * 2), GraphicsDevice.Viewport.Height, ZoomFactor));
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        PresentationParameters pp = GraphicsDevice.PresentationParameters;
        SurfaceFormat format = pp.BackBufferFormat;

        SpriteFont = Content.Load<SpriteFont>("textures/font-sprites");

        SpriteBatch = new SpriteBatch(GraphicsDevice);
        _sceneRenderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

        FlashRenderer.LoadContent(Content, GraphicsDevice);
        FlashRenderer.Register(_sceneRenderTarget);

        Renderer.LoadContent(Content);
        SoundLibrary.LoadContent(Content);

        PostProcess.LoadContent();
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();

        SpriteBatch.Dispose();
        _sceneRenderTarget.Dispose();
        _sceneRenderTarget.Dispose();
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
        if (Guide.IsVisible)
            return;

        InputHelper.Update();
        StorageManager.Update();

        // Allows the game to exit
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        //Send event to flash objects
        DispatchEvents();

        base.Update(gameTime);
    }

    /// <summary>
    /// Listen for events and dispatch to 'flash' game 
    /// </summary>
    private void DispatchEvents()
    {
        if (GraphicsDevice.Viewport.Bounds.Contains(InputHelper.MousePos)) {
            if (InputHelper.HasMouseMoved) 
            {
                if (_game.mouseMoveActions != null)
                    _game.mouseMoveActions(null);

                if (Stage.mouseMoveActions != null)
                    Stage.mouseMoveActions(null);
            }

            if (InputHelper.MouseLeftButtonJustPressed) 
            {
                if (_game.mouseDownActions != null)
                    _game.mouseDownActions(null);

                if (Stage.mouseDownActions != null)
                    Stage.mouseDownActions(null);
            }

            if (InputHelper.MouseLeftButtonJustReleased) 
            {
                if (_game.mouseUpActions != null)
                    _game.mouseUpActions(null);

                if (Stage.mouseUpActions != null)
                    Stage.mouseUpActions(null);
            }
        }

        if (InputHelper.Keyboard.GetPressedKeys().Length > 0)
        {
            if (_game.keyDownActions != null)
                _game.keyDownActions(null);

            if (Stage.keyDownActions != null)
                Stage.keyDownActions(null);
        }
            

        //Need to raise the enter frame events for all objects?
        if (_game.enterFrameActions != null)
            _game.enterFrameActions(null);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        CanDraw = true;

        Stage.Draw(_sceneRenderTarget, gameTime);
   
        this.PostProcess.PreRender();
        //GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        //...now apply the scaling to the final image - use point sampling for a nice clean look with none of the fuzziness that linear causes. 
        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null,null);
        SpriteBatch.Draw(_sceneRenderTarget, Vector2.Zero, GraphicsDevice.Viewport.Bounds, Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        SpriteBatch.End();
        
        base.Draw(gameTime);

        this.PostProcess.PostRender();

        CanDraw = false;
    }

    protected override void OnActivated(object sender, EventArgs args) {
        base.OnActivated(sender, args);

        //This event seems to be firing lots of times making the whole app quite unresponsive
        //Do nothing for now.
        return;
                                                     
        if (_game.activateActions != null)
            _game.activateActions(null);

        if (Stage.activateActions != null)
            Stage.activateActions(null);
    }

    protected override void OnDeactivated(object sender, EventArgs args) {
        base.OnDeactivated(sender, args);

        //This event seems to be firing lots of times making the whole app quite unresponsive
        //Do nothing for now.
        return;

        if (_game.deactivateActions != null)
            _game.deactivateActions(null);

        if (Stage.deactivateActions != null)
            Stage.deactivateActions(null);
    }
}
