using System;
using System.Collections.Generic;
using flash.display;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

public class FlashRenderer
{
    /// <summary>
    /// Responsible for caching drawing commands for play back at render end.
    /// </summary>
    private struct RenderInfo
    {
        public Texture2D Texture;
        public string Text;
        public Microsoft.Xna.Framework.Rectangle Destination;
        public Microsoft.Xna.Framework.Rectangle? Source;
        public Color Color;
    }

    private readonly Dictionary<RenderTarget2D, List<RenderInfo>> _queues = new Dictionary<RenderTarget2D, List<RenderInfo>>();
    private RenderTarget2D _renderTarget;
    private RenderTarget2D _defaultTarget;

    private Texture2D _pixelTexture;

    public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice) 
    {
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        var pixelData = new Color[1];
        _pixelTexture.GetData(pixelData);
        pixelData[0] = Color.White;
        _pixelTexture.SetData(pixelData);
    
    }

    public void Register(RenderTarget2D renderTarget) 
    {
        _queues.Add(renderTarget, new List<RenderInfo>());  

        if (_queues.Count == 1)
            _defaultTarget = renderTarget;
    }

    public int GetCommandCount(RenderTarget2D renderTarget) 
    {
        return _queues[renderTarget ?? _defaultTarget].Count;
    }

    public RenderTarget2D RenderTarget 
    { 
        get { return _renderTarget; }
        set 
        { 
            _renderTarget = value; 
            XnaGame.Instance.GraphicsDevice.SetRenderTarget(value);
        } 
    }

    public void CopyPixels(RenderTarget2D renderTarget, BitmapData sourceBitmapData, Rectangle sourceRect, Point destPoint, BitmapData alphaBitmapData = null, Point alphaPoint = null, Boolean mergeAlpha = false) {
        //XnaGame.Instance.SpriteBatch.Draw(sourceBitmapData.texture, new Vector2(destPoint.x, destPoint.y), new Microsoft.Xna.Framework.Rectangle(sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height), Color.White);

        var info = new RenderInfo
        {
            Texture = sourceBitmapData.texture,
            Destination = new Microsoft.Xna.Framework.Rectangle(destPoint.x, destPoint.y, sourceRect.width, sourceRect.height),
            Source = new Microsoft.Xna.Framework.Rectangle(sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height),
            Color = Color.White
        };

        _queues[renderTarget ?? _defaultTarget].Add(info);
    }

    public void DrawText(RenderTarget2D renderTarget, string text, Rectangle sourceRect, Point destPoint, uint color, float alpha)
    {
        this.DrawText(renderTarget, text, sourceRect, destPoint, UIntToColor(color) * alpha);
    }

    public void DrawText(RenderTarget2D renderTarget, string text, Rectangle sourceRect, Point destPoint, Color color, BitmapData alphaBitmapData = null, Point alphaPoint = null, Boolean mergeAlpha = false) {
        //XnaGame.Instance.SpriteBatch.Draw(sourceBitmapData.texture, new Vector2(destPoint.x, destPoint.y), new Microsoft.Xna.Framework.Rectangle(sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height), Color.White);

        var info = new RenderInfo
        {
            Text = text,
            Destination = new Microsoft.Xna.Framework.Rectangle(destPoint.x, destPoint.y, sourceRect.width, sourceRect.height),
            Color = color
        };

        _queues[renderTarget ?? _defaultTarget].Add(info);

    }

    public void FillRect(RenderTarget2D renderTarget, Rectangle rect, uint color, float alpha = 1.0f) {
        //XnaGame.Instance.SpriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rect.x, rect.y, rect.width, rect.height), UIntToColor(color));

        var info = new RenderInfo
        {
            Texture = _pixelTexture,
            Destination = new Microsoft.Xna.Framework.Rectangle(rect.x, rect.y, rect.width, rect.height),
            Source = null,
            Color = UIntToColor(color) * alpha
        };

        _queues[renderTarget ?? _defaultTarget].Add(info);
    }

    public void Flush(SpriteBatch spriteBatch, RenderTarget2D renderTarget) 
    {
       Draw(spriteBatch, renderTarget, Vector2.Zero, null);

       _queues[renderTarget ?? _defaultTarget].Clear();
    }

    public void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget, Vector2 offset, Color? color) 
    {
        var queue = _queues[renderTarget ?? _defaultTarget];

        foreach(var info in queue) 
        {
            var destination = new Microsoft.Xna.Framework.Rectangle(info.Destination.X + (int)offset.X, info.Destination.Y + (int)offset.Y, info.Destination.Width, info.Destination.Height);
            
            if (info.Texture != null)
                spriteBatch.Draw(info.Texture, destination, info.Source , color ?? info.Color);    
            else
                spriteBatch.DrawString(XnaGame.Instance.SpriteFont, info.Text, new Vector2(destination.X-1, destination.Y-1), color ?? info.Color);    
        }
    }

    public static Color UIntToColor(uint color)
    {
        var a = (byte)(color >> 24);
        var r = (byte)(color >> 16);
        var g = (byte)(color >> 8);
        var b = (byte)(color >> 0);
        return new Color(r, g, b, a);
    }
}

