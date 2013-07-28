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
    private struct RenderInfo
    {
        public Texture2D Texture;
        public Microsoft.Xna.Framework.Rectangle Destination;
        public Microsoft.Xna.Framework.Rectangle? Source;
        public Color Color;
    }

    private readonly Dictionary<RenderTarget2D, List<RenderInfo>> _queues = new Dictionary<RenderTarget2D, List<RenderInfo>>();

    private Texture2D _pixelTexture;

    public void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice) 
    {
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        var pixelData = new Color[1];
        _pixelTexture.GetData(pixelData);
        pixelData[0] = Color.White;
        _pixelTexture.SetData(pixelData);
    
    }

    public void Register(RenderTarget2D renderTarget2D) 
    {
        _queues.Add(renderTarget2D, new List<RenderInfo>());  
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

        _queues[renderTarget].Add(info);

    }

    public void FillRect(RenderTarget2D renderTarget, Rectangle rect, uint color) {
        //XnaGame.Instance.SpriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rect.x, rect.y, rect.width, rect.height), UIntToColor(color));

        var info = new RenderInfo
        {
            Texture = _pixelTexture,
            Destination = new Microsoft.Xna.Framework.Rectangle(rect.x, rect.y, rect.width, rect.height),
            Source = null,
            Color = UIntToColor(color)
        };

        _queues[renderTarget].Add(info);
    }

    public void Flush(SpriteBatch spriteBatch, RenderTarget2D renderTarget) 
    {
       Draw(spriteBatch, renderTarget, Vector2.Zero, null);

       _queues[renderTarget].Clear();
    }

    public void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget, Vector2 offset, Color? color) 
    {
        var queue = _queues[renderTarget];

        foreach(var info in queue) 
        {
            var destination = new Microsoft.Xna.Framework.Rectangle(info.Destination.X + (int)offset.X, info.Destination.Y + (int)offset.Y, info.Destination.Width, info.Destination.Height);
            spriteBatch.Draw(info.Texture, destination, info.Source , color ?? info.Color);    
        }
    }

    private static Color UIntToColor(uint color)
    {
        var a = (byte)(color >> 24);
        var r = (byte)(color >> 16);
        var g = (byte)(color >> 8);
        var b = (byte)(color >> 0);
        return new Color(r, g, b, a);
    }
}

