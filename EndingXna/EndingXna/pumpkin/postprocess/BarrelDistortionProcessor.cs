using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Renders a barrel distprtion / crt bulge / fish eye effect.
/// </summary>
public class BarrelDistortionProcessor : BaseProcessor
{
    public float CrtStrength { get; set; }
    public float Bulge { get; set; }

    private EffectParameter _viewportParameter;
    private EffectParameter _crtStrengthParameter;
    private EffectParameter _bulgeParameter;

    /// <summary>
    /// Initialises a new instance of a <see cref="BarrelDistortionProcessor">BarrelDistortionProcessor</see>.
    /// </summary>
    /// <param name="game">A reference to the game.</param>
    public BarrelDistortionProcessor(XnaGame game) : base(game) 
    { 
        CrtStrength = 0.15f;
        Bulge = 0.95f;
    }

    /// <summary>
    /// Occurs when a derived effect needs to load content.
    /// </summary>
    protected override void OnLoadContent()
    {
        this.Effect = this.Game.Content.Load<Effect>("Effects/BarrelDistortion");

        this._viewportParameter = this.Effect.Parameters["Viewport"];
        this._crtStrengthParameter = this.Effect.Parameters["CrtStrength"];
        this._bulgeParameter = this.Effect.Parameters["Bulge"];
    }

    /// <summary>
    /// Occurs when a derived effect needs to load content.
    /// </summary>
    protected override void OnUnloadContent()
    {
        this.Effect.Dispose();
    }

    /// <summary>
    /// Draws the post processing effect.
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="deltaTime">Time passed since the last frame.</param>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="renderTarget"></param>
    protected override bool OnDraw(SpriteBatch spriteBatch, float deltaTime, Texture2D texture, RenderTarget2D renderTarget)
    {
        //Less than == 0 what's the point!
        if (this.CrtStrength <= 0.0f)
            return false;

        this._viewportParameter.SetValue(new Vector2(this.Game.GraphicsDevice.Viewport.Width, this.Game.GraphicsDevice.Viewport.Height));
        this._crtStrengthParameter.SetValue(CrtStrength);
        this._bulgeParameter.SetValue(Bulge);
            
        return base.OnDraw(spriteBatch, deltaTime, texture, renderTarget);
    }
}
