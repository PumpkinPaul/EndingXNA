using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


/// <summary>
/// Manages the scanlines effect.
/// </summary>
public class ScanlinesProcessor : BaseProcessor
{
    /// <summary>The texture containing the scene with scanlines applied.</summary>
    private Texture2D _scanlineTexture;

    public float ScanlinesValue { get; set; }

    /// <summary>
    /// Initialises a new instance of a <see cref="ScanlinesProcessor">ScanlinesProcessor</see>.
    /// </summary>
    /// <param name="game">A reference to the game.</param>
    public ScanlinesProcessor(XnaGame game) : base(game) { }

    /// <summary>
    /// Occurs when a derived effect needs to load content.
    /// </summary>
    protected override void OnLoadContent()
    {
        this._scanlineTexture = this.Game.Content.Load<Texture2D>("textures/scanline");

        this.Effect = this.Game.Content.Load<Effect>("Effects/Scanlines");
        this.Effect.Parameters["scanlineTexture"].SetValue(this._scanlineTexture);
    }

    /// <summary>
    /// Occurs when a derived effect needs to load content.
    /// </summary>
    protected override void OnUnloadContent()
    {
        this._scanlineTexture.Dispose();
    }

    /// <summary>
    /// Draws the post processing effect.
    /// </summary>
    /// <param name="deltaTime">Time passed since the last frame.</param>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="renderTarget"></param>
    protected override bool OnDraw(SpriteBatch spriteBatch, float deltaTime, Texture2D texture, RenderTarget2D renderTarget)
    {
        //this.Game.GraphicsDevice.SetRenderTarget(renderTarget);

        //Less than 1% what's the point!
        if (this.ScanlinesValue <= 0.01f)
            return false;

        this.Effect.Parameters["repeatY"].SetValue((float)this.Game.GraphicsDevice.PresentationParameters.BackBufferHeight / (float)this._scanlineTexture.Height);
        this.Effect.Parameters["value"].SetValue(this.ScanlinesValue);
        return base.OnDraw(spriteBatch, deltaTime, texture, renderTarget);
    }
}
