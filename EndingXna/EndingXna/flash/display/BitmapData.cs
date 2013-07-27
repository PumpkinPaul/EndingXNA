using System;
using System.Collections.Generic;
using flash.geom;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = flash.geom.Point;
using Rectangle = flash.geom.Rectangle;

namespace flash.display
{
    public class BitmapData {

        static Dictionary<Texture2D, Color[]> PixelData = new Dictionary<Texture2D, Color[]>();

        public Texture2D texture;
        public int height { get; private set; }
        public Rectangle rect { get; private set; }
        public Boolean transparent { get; private set; }
        public int width { get; private set; }
        public uint fillColor { get; private set; }

        public BitmapData(int width, int height, Boolean transparent = true, uint fillColor = 0xFFFFFFFF) {
            //TODO
            this.width = width;
            this.height = height;
            this.transparent = transparent;
            this.fillColor = fillColor;

            rect = new Rectangle(0, 0, width, height);          
        }

        public BitmapData(Texture2D texture, int width, int height, Boolean transparent = true, uint fillColor = 0xFFFFFFFF) : this(width, height, transparent, fillColor) {
            this.texture = texture;
            
        }

        public void colorTransform(Rectangle rect, ColorTransform colorTransform) {
            //TODO
        }

        public void copyPixels(BitmapData sourceBitmapData, Rectangle sourceRect, Point destPoint, BitmapData alphaBitmapData = null, Point alphaPoint = null, Boolean mergeAlpha = false) {
            if (!XnaGame.Instance.CanDraw)
                throw new InvalidOperationException(); 

            if (sourceBitmapData.texture == null)
                return;

            XnaGame.Instance._spriteBatch.Draw(sourceBitmapData.texture, new Vector2(destPoint.x, destPoint.y), new Microsoft.Xna.Framework.Rectangle(sourceRect.x, sourceRect.y, sourceRect.width, sourceRect.height), Color.White);
        }

        public void fillRect(Rectangle rect, uint color) {
             if (!XnaGame.Instance.CanDraw)
                throw new InvalidOperationException(); 

             //XnaGame.Instance._spriteBatch.Draw(XnaGame.Instance._1x1PixelTexture, new Microsoft.Xna.Framework.Rectangle(rect.x, rect.y, rect.width, rect.height), new Color { PackedValue = color });
             XnaGame.Instance._spriteBatch.Draw(XnaGame.Instance._1x1PixelTexture, new Microsoft.Xna.Framework.Rectangle(rect.x, rect.y, rect.width, rect.height), UIntToColor(color));
        }

        public BitmapData clone() {
            return new BitmapData(width, height, transparent, fillColor);
        }

        public void dispose() {
            //NOP - graphics resources handle by XNA
        }

        private static Color UIntToColor(uint color)
        {
             var a = (byte)(color >> 24);
             var r = (byte)(color >> 16);
             var g = (byte)(color >> 8);
             var b = (byte)(color >> 0);
             return new Color(r, g, b, a);
        }

        public uint getPixel32(int x, int y) {
            //Hmmmm, hacky much - haha

            if (PixelData.ContainsKey(texture) == false) {
                var pixelData = new Color[texture.Width * texture.Height];
                texture.GetData(pixelData);
                PixelData[texture] = pixelData;
            }

            var pd = PixelData[texture];
            return pd[(y * texture.Width) + x].PackedValue;
        }
    }
}
