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
        public double alpha { get; set; }
        public double effectiveAlpha { get; set; }

        public Stage stage { get { return XnaGame.Stage; } }

        public Transform transform { get; set; }

        public DisplayObjectContainer parent { get; internal set; }

        public double mouseX { get { return XnaGame.Instance.MousePosition.X / XnaGame.Instance.Scale; } }
        public double mouseY { get { return XnaGame.Instance.MousePosition.Y / XnaGame.Instance.Scale; } }

        /// <summary>Whether or not the display object is visible.</summary>
        public Boolean visible { get; set; }

        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        public double scaleX { get; set; }
        public double scaleY { get; set; }
        public double scaleZ { get; set; }

        //public Action addedToStage;

        public DisplayObject() {
            visible = true;
            alpha = 1.0f;
        }

        //Bridge to XNA
        public virtual void Draw(RenderTarget2D sceneRenderTarget, GameTime gameTime) {
            
            bool parentVisible = true;

            effectiveAlpha = alpha;
            if (parent != null) {
                effectiveAlpha *= parent.effectiveAlpha;
                parentVisible = parent.visible;
            }

            //Draw this object first...
            if (visible && parentVisible) 
                OnDraw(sceneRenderTarget, gameTime);
        }

        protected internal virtual void OnDraw(RenderTarget2D sceneRenderTarget, GameTime gameTime)
        {
            //Replay cached drawing commands
        }
    }
}
