using System;
using Microsoft.Xna.Framework.Input;
using flash;
using pumpkin;
using BitmapData = flash.display.BitmapData;
using Rectangle = flash.geom.Rectangle;

namespace com.robotacid.gfx
{
    /// <summary>
    /// Keyboard input utility to wrap XNA keyboard
	/// 
	/// @conversion Paul Cunningham, pumpkin-games.net
	/// 
    /// Usage:
    /// Key.initialize(stage);
    /// if (Key.isDown(Keyboard.LEFT)) {
    ///    // Left key is being pressed
    /// }
    /// </summary>
    public class Key {

        public static readonly Boolean lockOut = false; // used to brick the Key class

        public const int NUMBER_2 = (int)Keys.D2;
        public const int NUMBER_4 = (int)Keys.D4;
        public const int NUMBER_6 = (int)Keys.D6;
        public const int NUMBER_8 = (int)Keys.D8;

        public const int H = (int)Keys.H;
        public const int J = (int)Keys.J;
        public const int K = (int)Keys.K;
        public const int L = (int)Keys.L;
        public const int R = (int)Keys.R;

        /**
        * Returns true or false if the key represented by the
        * custom key index is being pressed
        */
        public static Boolean customDown(int index) {
            //TODO:
            //return !lockOut && custom != null && Boolean(keysDown[custom[index]]);
            return false;
        }

        public static bool isDown(int key) {
            return InputHelper.IsKeyDown(key);
        }

        public static void clearKeys() {
            InputHelper.ClearKeyboardState();
        }
    }
}
