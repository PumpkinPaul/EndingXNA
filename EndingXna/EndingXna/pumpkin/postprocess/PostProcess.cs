using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Responsible for post processing effects.
/// </summary>
public class PostProcess
{
    public delegate void InitializeDelegate();
    public InitializeDelegate Initializer;

#if WINDOWS || XBOX360
    private XnaGame _game;

    private SpriteBatch _spriteBatch;
        
    public RenderTarget2D SceneRenderTarget1 { get; private set; }
    public RenderTarget2D SceneRenderTarget2 { get; private set; }

    public readonly List<BaseProcessor> _postProcessEffects = new List<BaseProcessor>();
#endif
        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    public PostProcess(XnaGame game, InitializeDelegate initializer) 
    {
#if WINDOWS || XBOX360
        this._game = game;
        this.Initializer = initializer;
#endif
    }

    public void Initialize() 
    {
#if WINDOWS || XBOX360
        if (this.Initializer != null) {
            this.Initializer();
        }
            
        //Post processing effects
        //this._scanlinesEffect = new ScanlinesEffect(this);
        //this._scanlinesEffect.ScanlinesValue = 1.0f;

        //foreach(var processor in this._postProcessEffects)
        //{
        //    processor.Initialize();
        //}
#endif  
    }

    public void AddProcessor(BaseProcessor processor) 
    {
#if WINDOWS || XBOX360
        this._postProcessEffects.Add(processor);
#endif
    }
        
    public void LoadContent() 
    {
#if WINDOWS || XBOX360
        this._spriteBatch = new SpriteBatch(this._game.GraphicsDevice);

        PresentationParameters pp = this._game.GraphicsDevice.PresentationParameters;
        SurfaceFormat format = pp.BackBufferFormat;

        this.SceneRenderTarget1 = new RenderTarget2D(this._game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
        this.SceneRenderTarget2 = new RenderTarget2D(this._game.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

        foreach(var processor in this._postProcessEffects)
        {
            processor.LoadContent();
        }
#endif
    }

    public void UnloadContent() 
    {
#if WINDOWS || XBOX360

        this.SceneRenderTarget1.Dispose();
        this.SceneRenderTarget2.Dispose();

        foreach(var processor in this._postProcessEffects)
        {
            processor.UnloadContent();
        }
#endif
    }

    /// <summary>
    /// Called before any rendering is done
    /// </summary>
    public RenderTarget2D PreRender() 
    {
#if WINDOWS || XBOX360
        //Set up the graphics device to render to the texture...
        this._game.GraphicsDevice.SetRenderTarget(this.SceneRenderTarget1);
        this._game.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

        return this.SceneRenderTarget1;
#endif
        return null;
    }

    public void PostRender() 
    {
#if WINDOWS || XBOX360
        //Draw all scene post processing effects.
        foreach(var processor in this._postProcessEffects)
        {
            if (processor.Active == false)
                continue;

            //Draw each post processing effect
            if (processor.Draw(this._spriteBatch, 1.0f, this.SceneRenderTarget1, this.SceneRenderTarget2) == false) 
                continue;
                
            //Effect rendered - swap the render targets so that the results of the previous post processing effect can be utilised by the next one.
            RenderTarget2D swapTexture = this.SceneRenderTarget1;
            this.SceneRenderTarget1 = this.SceneRenderTarget2;
            this.SceneRenderTarget2 = swapTexture;
        }

        //Draw the scene - everything that was drawn in the render target will now get drawn to the screen respecting the title safe area and the zoom factor!.
        this._game.GraphicsDevice.SetRenderTarget(null);
        this._game.GraphicsDevice.Clear(Color.FromNonPremultiplied(0,0,0,0));

        this._spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
        this._spriteBatch.Draw(this.SceneRenderTarget1, this._game.RenderViewport.Bounds, Color.White);
        this._spriteBatch.End();
#endif
    }
}
