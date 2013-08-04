using System;
using EndingXna;

namespace flash
{
    /// <summary>
    /// Basic implementation of the flash Mouse class.
    /// </summary>
    public static class Mouse {
        
        public static void show() {
            XnaGame.Instance.MouseVisible = true;
        }

        public static void hide() {
            XnaGame.Instance.MouseVisible = false;
        }
    }
}
