using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
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

        WorldGeneration.TileMap map;
        ChunkManager landscapeChunks;
        GameObject landscape;
        Player player;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";

        }

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
            //landscapeChunks = MagicaVoxel.ChunkManagerFromVox(@"castleOnHill.vox");
            map = new WorldGeneration.TileMap(new WorldGeneration.TileSet(MagicaVoxel.tileRoot));

            landscape = new GameObject(map.getManager(), new Vector3(), Vector3.One);
            
            //landscapeChunks.makeColorfulFloor();
            
            player = new Player();
            landscape.addChild(player.getActor());
            landscape.addChild(new Monster(new Vector3(12, 20, 12)));
            //landscape.addChild(new GameObject(map.getManager(), new Vector3(1,2,1), Vector3.One * .2f));
            
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
            
            //player.update(gameTime.ElapsedGameTime.Milliseconds / 1000f, landscapeChunks);
            //player.update(0, chunkManager);
            List<Action> actions = landscape.updateWithChildren();
            actions.AddRange(player.handleInput());
            foreach (Action action in actions){
                action.act(landscape, gameTime);
            }
            map.notifyOfPlayerLocation(player.getCameraLocation());
            map.report();
            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            Rendering.renderWorld(graphics, landscape, player);

            base.Draw(gameTime);
        }
    }
}


