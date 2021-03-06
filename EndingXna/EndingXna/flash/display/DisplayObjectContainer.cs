﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace flash.display
{
    public class DisplayObjectContainer : InteractiveObject {
        readonly List<DisplayObject> _displayObjects = new List<DisplayObject>();

        public DisplayObject addChild (DisplayObject child) {
            _displayObjects.Add(child);
            child.parent = this;

            return child;
        }

        public void removeChild(DisplayObject child) {
            _displayObjects.Remove(child);
            child.parent = null;
        }

        public int numChildren { get { return _displayObjects.Count; } }

        public void removeChildAt(int index) {
            _displayObjects.RemoveAt(index);
        }

        public override void Draw(RenderTarget2D sceneRenderTarget, GameTime gameTime) 
        {
            base.Draw(sceneRenderTarget, gameTime);

            if (visible)
                //...and then draw any objects that we contain.
                DrawChildren(sceneRenderTarget, gameTime);
        }

        private void DrawChildren(RenderTarget2D sceneRenderTarget, GameTime gameTime)
        {
            foreach (var displayObject in _displayObjects)
                displayObject.Draw(sceneRenderTarget, gameTime);    
        }
    }
}
