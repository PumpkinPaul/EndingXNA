using System;
using flash.events;
using flash.geom;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using pumpkin;

namespace flash.display
{
    public class DisplayObject : EventDispatcher, IBitmapDrawable {

        /// <summary>Indicates the alpha transparency value of the object specified.</summary>
        public Number alpha { get; set; }

        public Stage stage { get { return XnaGame.Stage; } }

        public Transform transform { get; set; }

        public DisplayObjectContainer parent { get; internal set; }

        public Number mouseX { get { return InputHelper.MousePos.X / XnaGame.Instance.Scale; } }
        public Number mouseY { get { return InputHelper.MousePos.Y / XnaGame.Instance.Scale; } }

        /// <summary>Whether or not the display object is visible.</summary>
        public Boolean visible { get; set; }

        public Number x { get; set; }
        public Number y { get; set; }
        public Number z { get; set; }

        public Number scaleX { get; set; }
        public Number scaleY { get; set; }
        public Number scaleZ { get; set; }

        //public Action addedToStage;

        public DisplayObject() {
            visible = true;
        }

        //Bridge to XNA
        public virtual void Draw(RenderTarget2D sceneRenderTarget, GameTime gameTime) {
            
            if (visible)
                OnDraw(sceneRenderTarget, gameTime);
        }

        protected internal virtual void OnDraw(RenderTarget2D sceneRenderTarget, GameTime gameTime)
        {
            //Replay cached drawing commands
        }
    }
}
