using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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

        World map;
        ChunkManager landscapeChunks;
        GameObject landscape;
        Player player;
        void OnResize(Object sender, EventArgs e)
        {
            
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            
        }
        public Game1()
        {

            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            graphics.PreferredBackBufferWidth = 1920 *1 ;
            graphics.PreferredBackBufferHeight = 1080 * 1;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";


            this.Window.AllowUserResizing = true;


            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(OnResize);

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }


        public void resetMap(string tileRootPath)
        {

        }

        void dragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop, false);
            int i=0;
            //for (i = 0; i < s.Length; i++)
                //listBox1.Items.Add(s[i]);
        }

        protected override void LoadContent()
        {
            // use C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools pipeline tool for this
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //System.Windows.Forms.Form gameForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            //gameForm.AllowDrop = true;
            //gameForm.DragEnter += new DragEventHandler(gameForm_DragEnter);
            //gameForm.DragDrop += new DragEventHandler(gameForm_DragDrop);

            map = new World(new WorldGeneration.TileSet(MagicaVoxel.tileRoot));

            Rendering.LoadContent(Content, graphics, map);
            player = new Player();
            map.addChild(player.getActor());
            map.addChild(new Slime(new Vector3(12, 20, 12)));
            Console.WriteLine("global seed is " + Globals.getSeed().ToString());
            
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
            graphics.ApplyChanges();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            //player.update(gameTime.ElapsedGameTime.Milliseconds / 1000f, landscapeChunks);
            //player.update(0, chunkManager);
            List<Action> actions = map.updateWithChildren();
            actions.AddRange(player.handleInput());
            foreach (Action action in actions){
                action.act(map, gameTime);
            }
            map.getMap().notifyOfPlayerLocation(player.getCameraLocation());
            //map.report();
            base.Update(gameTime);
            Console.WriteLine(Globals.random.Next());
            Globals.horribleRandomRefresh();

        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            Rendering.renderWorld(player);

            base.Draw(gameTime);
        }
    }
}


