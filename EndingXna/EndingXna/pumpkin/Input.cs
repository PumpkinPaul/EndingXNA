//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Input;

//namespace pumpkin
//{
//    public static class Input
//    {
//        public static MouseState _mouseState;
//        public static MouseState _previousMouseState;

//        public static KeyboardState _keyboardState  ;
//        public static KeyboardState _previouskeyboardState;

//        public static Point MouseLocation { get; private set; } 

//        public static void Update(GameTime gameTime) {
//            _previousMouseState = _mouseState;
//            _previouskeyboardState = _keyboardState;

//            _mouseState = Mouse.GetState();
//            _keyboardState = Keyboard.GetState();


//            //HasMouseMoved = _mouseState.X != _previousMouseState.X || _mouseState.Y != _previousMouseState.Y;
//        }

//        public 
//        public static bool HasMouseMoved { get { return _mouseState.X != _previousMouseState.X || _mouseState.Y != _previousMouseState.Y; }  }

//        public static int MouseX { get { return _mouseState.X; } }
//        public static int MouseY { get { return _mouseState.Y; } }

//        public static bool IsKeyDown(int key) 
//        { 
//            return _keyboardState.IsKeyDown((Keys)key);
//        }

//        public static void ClearKeyboardState() {
//            _keyboardState = new KeyboardState();
//        }
//    }
//}