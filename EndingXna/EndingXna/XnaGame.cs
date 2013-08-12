using System;
using System.Threading;
using com.robotacid.engine;
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
    
    private Texture2D _mousepointerTexture;
    public SpriteFont SpriteFont { get; private set; }
    public SpriteBatch SpriteBatch { get; private set; }
    private RenderTarget2D _sceneRenderTarget;
    public FlashRenderer FlashRenderer { get; private set; }

    public bool CanDraw { get; set; }

    private Game _game;
    
    public static Stage Stage { get; private set; }
    public float Scale { get { return (float)Instance._game.scaleRatio; } }

    private bool _mousepointer;
    public bool MouseVisible 
    {
        get { return _mousepointer; }
        set 
        { 
            #if (WINDOWS || XBOX360)
            _mousepointer = value; 
            #endif
        }
    }

    private Vector2 _mousePosition;
    public Vector2 MousePosition
    {
        get { return _mousePosition; }
        set { _mousePosition = value; }
    }

    /// <summary></summary>
    protected PostProcess PostProcess;

    #if XBOX360
        public const float DefaultZoomFactor = 0.0f;
        public const int TitleSafeTestOffset = 0;
    #else
        public const float DefaultZoomFactor = 0.0f;
        public const int TitleSafeTestOffset = 0;
    #endif

    private float _zoomFactor = DefaultZoomFactor;

    public float ZoomFactor { 
        get { return this._zoomFactor; } 
        set { 
            this._zoomFactor = value;
            this.SetRenderViewport();
        } 
    }

    public Rectangle GameWindow { get; set; }
    public Viewport RenderViewport { get; private set; }
    public Viewport FullscreenViewport { get; private set; }

    public XnaGame()
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        IsFixedTimeStep = true;
        IsMouseVisible = false;

        #if WINDOWS
        //_graphics.PreferredBackBufferWidth = 960;
        //_graphics.PreferredBackBufferHeight = 640;

        _mousepointer = true;
        #elif XBOX360
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;

        _mousepointer = true;
        #elif WINDOWS_PHONE
        //_graphics.PreferredBackBufferWidth = 960;
        //_graphics.PreferredBackBufferHeight = 480;
        //_graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
    
        _graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
        _mousepointer = false;
        #endif

        //Make 30 FPS or put a key limiter on KeyDown!
        TargetElapsedTime = TimeSpan.FromSeconds(1/60.0f);

        StorageManager = new StorageManager("EndingXNA");
        StorageManager.ShowStorageGuide();
        StorageManager.StorageDeviceAction += (sender, e) => {
            if (e.DialogAction == DialogAction.Select)
                Game.LoadFromUserStorage(e.StorageContainer);
        };
        

        FlashRenderer = new FlashRenderer();

        //Post processing effects for bloom, fisheye and scanlines...
        PostProcess = new PostProcess(this, null);
        #if WINDOWS || XBOX360
        PostProcess.AddProcessor(new BloomProcessor(this) { 
            Active = true, Settings = BloomProcessor.BloomSettings.PresetSettings[7]
        });
        PostProcess.AddProcessor(new BarrelDistortionProcessor(this) { 
            Active = false
        });
        PostProcess.AddProcessor(new ScanlinesProcessor(this) { 
            Active = false, ScanlinesValue = 0.25f
        });

        Components.Add(new GamerServicesComponent(this));

        #endif
        
        //Components.Add(ThreadPoolComponent = new ThreadPoolComponent(this));
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

        InputHelper.Initialize();

        //Initialise the final rendering viewport - this will allow the user to size the rendered image to match their display device
        SetRenderViewport();
        PostProcess.Initialize();

        Stage = new Stage();
        _game = new Game();

        Stage.addChild(_game);
    }

    public void SetRenderViewport()
    {
        FullscreenViewport = GraphicsDevice.Viewport;

        //aspect ratio (1.5) - we want to mimic that in current resolution.
        //Ideal stage is 2 x 1 (e.g 960x480) 
        var ratioPixelsX = (int)((FullscreenViewport.Width - FullscreenViewport.Height * 1.5f) / 2.0f);

        //Create the final render viewport repecting the titlesafe and above aspect ratio
        RenderViewport = new Viewport(
                (int)MathHelper.Lerp(GraphicsDevice.Viewport.TitleSafeArea.X + TitleSafeTestOffset + ratioPixelsX, FullscreenViewport.X, ZoomFactor),
                (int)MathHelper.Lerp(GraphicsDevice.Viewport.TitleSafeArea.Y + TitleSafeTestOffset, FullscreenViewport.Y, ZoomFactor),
                (int)MathHelper.Lerp(GraphicsDevice.Viewport.TitleSafeArea.Width - ((TitleSafeTestOffset + ratioPixelsX) * 2), FullscreenViewport.Width, ZoomFactor),
                (int)MathHelper.Lerp(GraphicsDevice.Viewport.TitleSafeArea.Height - (TitleSafeTestOffset * 2), FullscreenViewport.Height, ZoomFactor));

        GameWindow = new Rectangle(0, 0, (int)(FullscreenViewport.Height * 1.5f), FullscreenViewport.Height);
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
        _mousepointerTexture = Content.Load<Texture2D>("textures/Mousepointer");

        SpriteBatch = new SpriteBatch(GraphicsDevice);
        _sceneRenderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

        FlashRenderer.LoadContent(Content, GraphicsDevice);
        FlashRenderer.Register(_sceneRenderTarget);

        Renderer.LoadContent(Content);
        SoundLibrary.LoadContent(Content);
        Level.LoadContent(Content);

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

        _mousePosition.X = InputHelper.MousePos.X;
        _mousePosition.Y = InputHelper.MousePos.Y;

        //Send event to flash objects
        DispatchEvents();

        base.Update(gameTime);
    }

    /// <summary>
    /// Listen for events and dispatch to 'flash' game 
    /// </summary>
    private void DispatchEvents()
    {
        if (GraphicsDevice.Viewport.Bounds.Contains(new Point((int)MousePosition.X, (int)MousePosition.Y))) {
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

        //Ending wants a 1.5 aspect ratio - the default 1280x720 Xbox ratio is 1.7777 - therefore we'll add a modifier to the render viewport and the 
        //mouseposition to fix along the x-axis by the correct amount.
        #if WINDOWS || XBOX360
        float xScale = ((FullscreenViewport.Width / (float)FullscreenViewport.Height) / 1.5f);
        #else
        const float xScale = 1.0f;
        #endif
        _mousePosition.X = (int)(MousePosition.X * xScale);

        Stage.Draw(_sceneRenderTarget, gameTime);
   
        this.PostProcess.PreRender();
       
        GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null,null);
        SpriteBatch.Draw(_sceneRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, new Vector2(Scale * xScale, Scale) , SpriteEffects.None, 0);
        SpriteBatch.End();
        
        if (_mousepointer) 
        {
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null,null);
            SpriteBatch.Draw(_mousepointerTexture, MousePosition, null, Color.White, 0, new Vector2(64), 1.0f, SpriteEffects.None, 1.0f);
            SpriteBatch.End();
        }

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
