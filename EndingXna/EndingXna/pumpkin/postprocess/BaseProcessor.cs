using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// A base class for post processing.
/// </summary>
public abstract class BaseProcessor 
{
    /// <summary>A reference to the game.</summary>
    protected readonly XnaGame Game;

    /// <summary></summary>
    protected Effect Effect;

    /// <summary></summary>
    public bool Active { get; set; }

    /// <summary>
    /// Initialises a new instance of a <see cref="BaseProcessor">PostProcessEffect</see>.
    /// </summary>
    /// <param name="game">A reference to the game.</param>
    protected BaseProcessor(XnaGame game)
    {
        this.Game = game;
    }

    /// <summary>
    /// Loads content for the post processing effect.
    /// </summary>
    public void LoadContent()
    {
        this.OnLoadContent();
    }

    /// <summary>
    /// Occurs when a derived effect needs to load content.
    /// </summary>
    protected abstract void OnLoadContent();

    /// <summary>
    /// Unloads content for the post processing effect.
    /// </summary>
    public void UnloadContent()
    {
        this.Effect.Dispose();

        this.OnUnloadContent();
    }

    /// <summary>
    /// Occurs when a derived effect needs to load content.
    /// </summary>
    protected abstract void OnUnloadContent();

    /// <summary>
    /// Applies the effect to the scene.
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="deltaTime">Time passed since the last frame.</param>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="renderTarget">A new render target for the device, or null to set the device render target to the back buffer of the device.</param>
    public bool Draw(SpriteBatch spriteBatch, float deltaTime, Texture2D texture, RenderTarget2D renderTarget)
    {
        //Do common stuff here before deferring to processor...

        //...ok, now render the effect(s).
        return this.OnDraw(spriteBatch, deltaTime, texture, renderTarget);
    }

    /// <summary>
    /// Draws the post processing effect.
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="deltaTime">Time passed since the last frame.</param>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="renderTarget"></param>
    protected virtual bool OnDraw(SpriteBatch spriteBatch, float deltaTime, Texture2D texture, RenderTarget2D renderTarget)
    {
        this.Game.GraphicsDevice.SetRenderTarget(renderTarget);

        PresentationParameters pp = this.Game.GraphicsDevice.PresentationParameters;
        var destinationRect = new Rectangle(0, 0, pp.BackBufferWidth,pp.BackBufferHeight);
     
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, this.Effect);

        foreach (var pass in this.Effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            spriteBatch.Draw(texture, destinationRect, Color.White);
        }

        spriteBatch.End();

        return true;
    }
}
