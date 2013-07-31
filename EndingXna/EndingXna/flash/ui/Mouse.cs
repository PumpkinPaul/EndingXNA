using System;
using EndingXna;

namespace flash
{
    /// <summary>
    /// Basic implementation of the flash Mouse class.
    /// </summary>
    public static class Mouse {
        
        public static void show() {
            XnaGame.Instance.IsMouseVisible = true;
        }

        public static void hide() {
            XnaGame.Instance.IsMouseVisible = false;
        }
    }
}
