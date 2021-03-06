﻿using System;
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

        public RenderTarget2D renderTarget { get; set; }
        public Texture2D texture;
        public int height { get; private set; }
        public Rectangle rect { get; private set; }
        public Boolean transparent { get; private set; }
        public int width { get; private set; }
        public uint fillColor { get; private set; }

        public BitmapData(int width, int height, Boolean transparent = true, uint fillColor = 0xFFFFFFFF) {
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

        }

        public void copyPixels(BitmapData sourceBitmapData, Rectangle sourceRect, Point destPoint, BitmapData alphaBitmapData = null, Point alphaPoint = null, Boolean mergeAlpha = false) {

            if (sourceBitmapData.texture == null)
                return;

            XnaGame.Instance.FlashRenderer.CopyPixels(renderTarget, sourceBitmapData, sourceRect, destPoint, alphaBitmapData, alphaPoint, mergeAlpha);
        }

        public void copyPixels(string text, Rectangle sourceRect, Point destPoint, float alpha) 
        {
            XnaGame.Instance.FlashRenderer.DrawText(renderTarget, text, sourceRect, destPoint, Color.White * alpha);
        }

        public void copyPixels(string text, Rectangle sourceRect, Point destPoint, BitmapData alphaBitmapData = null, Point alphaPoint = null, Boolean mergeAlpha = false) {

            XnaGame.Instance.FlashRenderer.DrawText(renderTarget, text, sourceRect, destPoint, Color.White, alphaBitmapData, alphaPoint, mergeAlpha);
        }

        public void fillRect(Rectangle rect, uint color) {

             XnaGame.Instance.FlashRenderer.FillRect(renderTarget, rect, color); 
        }

        public BitmapData clone() {
            return new BitmapData(texture, width, height, transparent, fillColor);
        }

        public void dispose() {
            //NOP - graphics resources handle by XNA
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

        public void setPixel32(int x, int y, uint color) {
            //Hmmmm, hacky much - haha

            if (PixelData.ContainsKey(texture) == false) {
                var pixelData = new Color[texture.Width * texture.Height];
                texture.GetData(pixelData);
                PixelData[texture] = pixelData;
            }

            PixelData[texture][(y * texture.Width) + x] = FlashRenderer.UIntToColor(color);
        }

        public void setData() {
            var pixelData = PixelData[texture];
            texture.SetData(pixelData);   
        }
    }
}
