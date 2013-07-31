using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Manages the scanlines effect.
/// </summary>
public class BloomProcessor : BaseProcessor
{
    /// <summary>
    /// Class holds all the settings used to tweak the bloom effect.
    /// </summary>
    public class BloomSettings
    {
        /// <summary>Name of a preset bloom setting, for display to the user.</summary>
        public readonly string Name;

        /// <summary> 
        /// Controls how bright a pixel needs to be before it will bloom. Zero makes everything bloom equally, while higher values select
        /// only brighter colors. Somewhere between 0.25 and 0.5 is good.
        /// </summary>
        public readonly float BloomThreshold;

        /// <summary> 
        /// Controls how much blurring is applied to the bloom image. The typical range is from 1 up to 10 or so.
        /// </summary>
        public readonly float BlurAmount;

        /// <summary>
        /// Controls the amount of the bloom and base images that will be mixed into the final scene. Range 0 to 1.
        /// </summary>
        public readonly float BloomIntensity;
        public readonly float BaseIntensity;

        /// <summary>
        /// Independently control the color saturation of the bloom and base images. Zero is totally desaturated, 1.0 leaves saturation
        /// unchanged, while higher values increase the saturation level.
        /// </summary>
        public readonly float BloomSaturation;
        public readonly float BaseSaturation;

        /// <summary>
        /// Initilises a new instance of a <see cref="BloomSettings">BloomSettings</see>.
        /// </summary>
        public BloomSettings(string name, float bloomThreshold, float blurAmount, float bloomIntensity, float baseIntensity, float bloomSaturation, float baseSaturation)
        {
            this.Name = name;
            this.BloomThreshold = bloomThreshold;
            this.BlurAmount = blurAmount;
            this.BloomIntensity = bloomIntensity;
            this.BaseIntensity = baseIntensity;
            this.BloomSaturation = bloomSaturation;
            this.BaseSaturation = baseSaturation;
        }
        
        /// <summary>
        /// Table of preset bloom settings, used by the sample program.
        /// </summary>
        public static BloomSettings[] PresetSettings =
        {
            //                Name           Thresh  Blur Bloom  Base  BloomSat BaseSat
            new BloomSettings("OFF",         0.0f,   0,   0.0f,  0,    0,       0),
            new BloomSettings("SUBTLE",      0.5f,   2,   1,     1,    1,       1),
            new BloomSettings("MEDIUM",      0.25f,  4,   1.25f, 1,    1,       1),
            new BloomSettings("SOFT",        0,      3,   1,     1,    1,       1),
            new BloomSettings("HIGH",        0.05f,  4,   1.5f,  1,    1.5f,    1.5f),
            new BloomSettings("DESATURATED", 0.5f,   5,   1.5f,  1,    0,       1),
            new BloomSettings("SATURATED",   0.25f,  4,   2,     1,    1.5f,    0.5f),
            new BloomSettings("ENDING",      0.25f,  1f,   1f,  1f, 1f,   1f),
        };
    }

    /// <summary></summary>
    private Effect _bloomExtractEffect;

    /// <summary></summary>
    private Effect _bloomCombineEffect;

    /// <summary></summary>
    private Effect _gaussianBlurEffect;

    /// <summary></summary>
    private RenderTarget2D _renderTarget1;

    /// <summary></summary>
    private RenderTarget2D _renderTarget2;

    /// <summary></summary>
    private int _sampleCount;

    /// <summary></summary>
    private float[] _sampleWeights;

    /// <summary></summary>
    private Vector2[] _sampleOffsets;

    /// <summary>The display settings the bloom should use.</summary>
    public BloomSettings Settings = BloomSettings.PresetSettings[0];

    /// <summary>
    /// Initialises a new instance of a <see cref="BloomProcessor">BloomProcessor</see>.
    /// </summary>
    /// <param name="game">A reference to the game.</param>
    public BloomProcessor(XnaGame game) : base(game) { }

    /// <summary>
    /// Occurs when a derived effect needs to load content.
    /// </summary>
    protected override void OnLoadContent()
    {
        this._bloomExtractEffect = Game.Content.Load<Effect>("effects/BloomExtract");
        this._bloomCombineEffect = Game.Content.Load<Effect>("effects/BloomCombine");
        this._gaussianBlurEffect = Game.Content.Load<Effect>("effects/GaussianBlur");

        this._sampleCount = _gaussianBlurEffect.Parameters["SampleWeights"].Elements.Count;

        //Create temporary arrays for computing our filter settings.
        this._sampleWeights = new float[this._sampleCount];
        this._sampleOffsets = new Vector2[this._sampleCount];

        //Look up the resolution and format of our main backbuffer.
        PresentationParameters pp = this.Game.GraphicsDevice.PresentationParameters;

        int width = pp.BackBufferWidth;
        int height = pp.BackBufferHeight;

        SurfaceFormat format = pp.BackBufferFormat;

        //Create a texture for rendering the main scene, prior to applying bloom.
        //this._sceneRenderTarget = new RenderTarget2D(GraphicsDevice, width, height, false,
        //        format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

        //Create two rendertargets for the bloom processing. These are half the size of the backbuffer, in order to minimize fillrate costs. 
        //Reducing the resolution in this way doesn't hurt quality, because we are going to be blurring the bloom images in any case.
        width /= 2;
        height /= 2;

        this._renderTarget1 = new RenderTarget2D(this.Game.GraphicsDevice, width, height, false, format, DepthFormat.None);
        this._renderTarget2 = new RenderTarget2D(this.Game.GraphicsDevice, width, height, false, format, DepthFormat.None);
    }

    /// <summary>
    /// Occurs when a derived effect needs to load content.
    /// </summary>
    protected override void OnUnloadContent()
    {
        this._bloomExtractEffect.Dispose();
        this._bloomCombineEffect.Dispose();
        this._gaussianBlurEffect.Dispose();

        this._renderTarget1.Dispose();
        this._renderTarget2.Dispose();
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
        this.Game.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

        //Pass 1: draw the scene into rendertarget 1, using a shader that extracts only the brightest parts of the image.
        this._bloomExtractEffect.Parameters["BloomThreshold"].SetValue(this.Settings.BloomThreshold);
        this.DrawFullscreenQuad(spriteBatch, texture, this._renderTarget1, this._bloomExtractEffect);

        //Pass 2: draw from rendertarget 1 into rendertarget 2, using a shader to apply a horizontal gaussian blur filter.
        this.SetBlurEffectParameters(1.0f / (float)this._renderTarget1.Width, 0);
        this.DrawFullscreenQuad(spriteBatch, this._renderTarget1, this._renderTarget2, this._gaussianBlurEffect);

        //Pass 3: draw from rendertarget 2 back into rendertarget 1, using a shader to apply a vertical gaussian blur filter.
        this.SetBlurEffectParameters(0, 1.0f / (float)this._renderTarget1.Height);
        this.DrawFullscreenQuad(spriteBatch, this._renderTarget2, this._renderTarget1, this._gaussianBlurEffect);

        //Pass 4: draw both rendertarget 1 and the original scene image back into the main backbuffer, using a shader that
        //combines them to produce the final bloomed result.
        this.Game.GraphicsDevice.SetRenderTarget(renderTarget);

        EffectParameterCollection parameters = this._bloomCombineEffect.Parameters;

        parameters["BloomIntensity"].SetValue(this.Settings.BloomIntensity);
        parameters["BaseIntensity"].SetValue(this.Settings.BaseIntensity);
        parameters["BloomSaturation"].SetValue(this.Settings.BloomSaturation);
        parameters["BaseSaturation"].SetValue(this.Settings.BaseSaturation);

        this.Game.GraphicsDevice.Textures[1] = texture;

        var viewport = this.Game.GraphicsDevice.Viewport;

        this.DrawFullscreenQuad(spriteBatch, this._renderTarget1, viewport.Width, viewport.Height, this._bloomCombineEffect);

        return true;
    }

    /// <summary>
    /// Helper for drawing a texture into a rendertarget, using
    /// a custom shader to apply postprocessing effects.
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="renderTarget">The render target used to capture the result of the draw.</param>
    /// <param name="effect">The effect to use to draw the texture.</param>
    private void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, RenderTarget2D renderTarget, Effect effect)
    {
        this.Game.GraphicsDevice.SetRenderTarget(renderTarget);

        this.DrawFullscreenQuad(spriteBatch, texture, renderTarget.Width, renderTarget.Height, effect);
    }

    /// <summary>
    /// Helper for drawing a texture into the current rendertarget,
    /// using a custom shader to apply postprocessing effects.
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="texture">The tecture to draw.</param>
    /// <param name="width">The width of the destination rectangle.</param>
    /// <param name="height">The height of the destination rectangle.</param>
    /// <param name="effect">The effect to use to draw the texture.</param>
    void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, int width, int height, Effect effect)
    {
        spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
        spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
        spriteBatch.End();
    }

    /// <summary>
    /// Computes sample weightings and texture coordinate offsets
    /// for one pass of a separable gaussian blur filter.
    /// </summary>
    /// <param name="dx">The number of pixels to offset the texture sample on the x-axis.</param>
    /// <param name="dy">The number of pixels to offset the texture sample on the y-axis.</param>
    private void SetBlurEffectParameters(float dx, float dy)
    {
        //Look up the sample weight and offset effect parameters.
        EffectParameter weightsParameter = _gaussianBlurEffect.Parameters["SampleWeights"];
        EffectParameter offsetsParameter = _gaussianBlurEffect.Parameters["SampleOffsets"];

        //The first sample always has a zero offset.
        this._sampleWeights[0] = ComputeGaussian(0);
        this._sampleOffsets[0] = new Vector2(0);

        //Maintain a sum of all the weighting values.
        float totalWeights = this._sampleWeights[0];

        //Add pairs of additional sample taps, positioned
        //along a line in both directions from the center.
        for (int i = 0; i < this._sampleCount / 2; i++)
        {
            //Store weights for the positive and negative taps.
            float weight = this.ComputeGaussian(i + 1);

            this._sampleWeights[i * 2 + 1] = weight;
            this._sampleWeights[i * 2 + 2] = weight;

            totalWeights += weight * 2;

            //To get the maximum amount of blurring from a limited number of pixel shader samples, we take advantage of the bilinear filtering
            //hardware inside the texture fetch unit. If we position our texture coordinates exactly halfway between two texels, the filtering unit
            //will average them for us, giving two samples for the price of one. This allows us to step in units of two texels per sample, rather
            //than just one at a time. The 1.5 offset kicks things off by positioning us nicely in between two texels.
            float sampleOffset = i * 2 + 1.5f;

            Vector2 delta = new Vector2(dx, dy) * sampleOffset;

            //Store texture coordinate offsets for the positive and negative taps.
            this._sampleOffsets[i * 2 + 1] = delta;
            this._sampleOffsets[i * 2 + 2] = -delta;
        }

        //Normalize the list of sample weightings, so they will always sum to one.
        for (int i = 0; i < this._sampleWeights.Length; i++)
            this._sampleWeights[i] /= totalWeights;

        //Tell the effect about our new filter settings.
        weightsParameter.SetValue(this._sampleWeights);
        offsetsParameter.SetValue(this._sampleOffsets);
    }

    /// <summary>
    /// Evaluates a single point on the gaussian falloff curve.
    /// Used for setting up the blur filter weightings.
    /// </summary>
    /// <param name="n"></param>
    private float ComputeGaussian(float n)
        {
            float theta = this.Settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) * Math.Exp(-(n * n) / (2 * theta * theta)));
        }
    }
