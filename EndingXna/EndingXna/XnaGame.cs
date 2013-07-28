using System;
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
    
    public SpriteBatch SpriteBatch { get; private set; }
    private RenderTarget2D _sceneRenderTarget;
    public FlashRenderer FlashRenderer { get; private set; }

    private Game _game;
    
    public static Stage Stage { get; private set; }
    public float Scale { get { return Instance._game.scaleRatio; } }
    public bool CanDraw { get; private set; }

    public XnaGame()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        _graphics.PreferredBackBufferWidth = 480;
        _graphics.PreferredBackBufferHeight = 320;

        _graphics.PreferredBackBufferWidth = 960;
        _graphics.PreferredBackBufferHeight = 640;

        //Make 30 FPS or put a key limiter on KeyDown!
        TargetElapsedTime = TimeSpan.FromSeconds(1/30.0f);

        FlashRenderer = new FlashRenderer();
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

        IsMouseVisible = true;
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        PresentationParameters pp = GraphicsDevice.PresentationParameters;
        SurfaceFormat format = pp.BackBufferFormat;

        SpriteBatch = new SpriteBatch(GraphicsDevice);
        _sceneRenderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

        FlashRenderer.LoadContent(Content, GraphicsDevice);

        Renderer.LoadContent(Content);
        SoundLibrary.LoadContent(Content);
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
        InputHelper.Update();

        // Allows the game to exit
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        //Send event to flash objects
        DispatchEvents();

        base.Update(gameTime);
    }

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

        //GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
        ////GraphicsDevice.Clear(Color.Black);

        //////Blit the game with no scaling here...
        ////SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, null, null, null);
        _game.Draw(gameTime);
        ////SpriteBatch.End();
        
        //GraphicsDevice.SetRenderTarget(null);

        //////...now apply the scaling to the final image - use poit sampling for a nice clean look with none of the fuzziness that linear causes. 
        //SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null,null);
        //SpriteBatch.Draw(_sceneRenderTarget, Vector2.Zero, GraphicsDevice.Viewport.Bounds, Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        //SpriteBatch.End();

        base.Draw(gameTime);

        CanDraw = false;
    }

    protected override void OnActivated(object sender, EventArgs args) {
        base.OnActivated(sender, args);
                                                     
        if (_game.activateActions != null)
            _game.activateActions(null);

        if (Stage.activateActions != null)
            Stage.activateActions(null);
    }

    protected override void OnDeactivated(object sender, EventArgs args) {
        base.OnDeactivated(sender, args);

        if (_game.deactivateActions != null)
            _game.deactivateActions(null);

        if (Stage.deactivateActions != null)
            Stage.deactivateActions(null);
    }
}
