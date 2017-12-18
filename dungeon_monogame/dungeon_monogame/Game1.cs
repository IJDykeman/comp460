using Microsoft.Xna.Framework; 
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace dungeon_monogame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public static GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;

        ChunkManager landscapeChunks;
        Player player;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        //Content.RootDirectory = "Content";
            // use C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools pipeline tool for this

            // TODO: use this.Content to load your game content here
            Rendering.LoadContent(Content, graphics);
            landscapeChunks = new ChunkManager();
            landscapeChunks.makeColorfulFloor();
            player = new Player();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            handleInput();
            player.update(gameTime.ElapsedGameTime.Milliseconds / 1000f, landscapeChunks);
            //player.update(0, chunkManager);
            base.Update(gameTime);
        }

        void handleInput()
        {
            player.handleInput();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            Rendering.renderWorld(graphics, player, landscapeChunks);

            base.Draw(gameTime);
        }
    }
}


