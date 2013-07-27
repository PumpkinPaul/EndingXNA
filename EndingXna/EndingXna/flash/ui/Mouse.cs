using System;
using EndingXna;

namespace flash
{
    public static class Mouse {
        
        public static void show() {
            XnaGame.Instance.IsMouseVisible = true;
        }

        public static void hide() {
            XnaGame.Instance.IsMouseVisible = false;
        }
    }
}
