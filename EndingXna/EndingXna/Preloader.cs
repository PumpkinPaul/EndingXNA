using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using flash;
using Array = flash.Array;

//using Array = flash.Array<object>;

namespace EndingXna
{
    /// <summary>
    /// This is the bridge between XNA and the Flash / Endig code
    /// </summary>
    public class Preloader : Microsoft.Xna.Framework.Game
    {
        readonly GraphicsDeviceManager _graphics;
        Game _game;

        public Preloader()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _graphics.PreferredBackBufferWidth = 480;
            _graphics.PreferredBackBufferHeight = 320;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            //Test();

            _game = new Game();
        }

        //private void Test() {
        //    Number x = 1.8; 
        //    Number y = 1; 
        //    Number z = 2; 

        //    int p = x;

        //    //var letters = new [] {"a", "b", "c", "d", "e", "f"};
        //    //var someLetters = letters.slice(1,3);
        //    //var someLetters2 = letters.slice(2);
        //    //var someLetters3 = letters.slice(-2);

        //    //trace(letters);
        //    //trace(someLetters);
        //    //trace(someLetters2);
        //    //trace(someLetters3);

        //    //var letters = new flash.Array{ "a", "b", "c", "d", "e", "f"};
        //    //var someLetters = letters.slice(1,3);
        //    //var someLetters2 = letters.slice(2);
        //    //var someLetters3 = letters.slice(-2);

        //    //this.trace(letters);
        //    //this.trace(someLetters);
        //    //this.trace(someLetters2);
        //    //this.trace(someLetters3);

        //    //Array<Array> rooms = new Array<Array> { new Array { 1 }}; 
        //    //rooms[0] = new Array { 2, 3, 4};
        //    //rooms[0][1] = 3;

        //    var vegetables = new Array {"spinach",
        //         "green pepper",
        //         "cilantro",
        //         "onion",
        //         "avocado"};

        //    var spliced = vegetables.splice(2, 2);
        //    language.trace(vegetables); // spinach,green pepper,avocado
        //    language.trace(spliced);    // cilantro,onion

        //    vegetables.splice(1, 0, spliced);
        //    //vegetables.splice(1, 0, "cilantro", "onion");
        //    language.trace(vegetables); // spinach,cilantro,onion,green pepper,avocado
        //}

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            _game.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _game.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
