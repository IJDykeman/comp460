using dungeon_monogame.WorldGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace dungeon_monogame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    internal class Game1 : Game
    {
        public static GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;

        World map;



        ChunkManager landscapeChunks;
        GameObject landscape;
        Player player;
        bool menuOpen = true;
        internal void toggleMainMenu()
        {
            menuOpen = !menuOpen;

            if (!menuOpen)
            {
                player.setMouseToCenter();
            }
        }



        Menu menu;
        void OnResize(Object sender, EventArgs e)
        {

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            Rendering.resetRendertargets();

        }
        public Game1()
        {


            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string defaultTilesFolder = Path.Combine(currentDirectory, "Content\\trivial_tile_set");

            var files = TileSetLoader.getVoxFiles(defaultTilesFolder);
            WorldGeneration.TileSet tiles = new WorldGeneration.TileSet(files, false, false);
            map = new World(tiles);

            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            graphics.PreferredBackBufferWidth = GlobalSettings.startingWindowWidth;
            graphics.PreferredBackBufferHeight = GlobalSettings.startingWindowHeight;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            Window.Title = "Generate Worlds";

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
            int i = 0;
            //for (i = 0; i < s.Length; i++)
            //listBox1.Items.Add(s[i]);
        }

        protected override void LoadContent()
        {
            // use C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools pipeline tool for this
            spriteBatch = new SpriteBatch(GraphicsDevice);
            DungeonContentManager.loadContent(Content);
            menu = new MainMenu();
            //System.Windows.Forms.Form gameForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            //gameForm.AllowDrop = true;
            //gameForm.DragEnter += new DragEventHandler(gameForm_DragEnter);
            //gameForm.DragDrop += new DragEventHandler(gameForm_DragDrop);

            //TODO replace with generic default world


            Rendering.LoadContent(Content, graphics, map);
            player = new Player();
            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);



            // TODO error check here
            string[] voxFiles = TileSetLoader.getVoxFiles(Path.Combine(currentDirectory, "Content\\trivial_tile_set"));
            map.addChild(new TileSetLoader(false, false, voxFiles));
            map.addChild(player.getActor());
            map.addChild(new Slime(new Vector3(12, 20, 12)));
            Console.WriteLine("global seed is " + Globals.getSeed().ToString());
        }


        public void setTileset(TileSet tileset)
        {
            map.resetTileMap(tileset, tileset.worldAssumedFlat);
        }




        public void loadNewTileset(bool exampleBased)
        {

            bool userWantsFlatWorld = false;

            string[] files = TileSetLoader.getTilesetPathFromUser(exampleBased);
            if (files != null)
            {
                if (!files.Any(x => x.ToUpper().EndsWith(".VOX")))
                {
                    FileManagement.ShowDialog("Please select an example .vox file or folder of .vox files that form a tile set.", "");
                }
                else
                {
                    if (!exampleBased)
                    {
                        userWantsFlatWorld = FileManagement.AskWhetherWorldIsFlat();
                    }
                    map.addChild(new TileSetLoader(exampleBased, userWantsFlatWorld, files));
                }
            }

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }




        protected override void Update(GameTime gameTime)
        {
            graphics.ApplyChanges();
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();
            
            List<Action> actions = map.updateWithChildren();
            if (menuOpen)
            {
                actions.AddRange(menu.handleInput());
            }
            else
            {
                actions.AddRange(player.handleInput());
            }

            foreach (Action action in actions)
            {
                action.actOnGame(this);
            }

            foreach (Action action in actions){
                action.actOnWorld(map, gameTime);
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
            //myForm.Activate();

            IsMouseVisible = menuOpen;
            Rendering.renderWorld(player);
            if (menuOpen)
            {
                menu.updateDimensions(new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
                menu.draw(graphics);
            }
            map.draw2D(graphics);
            base.Draw(gameTime);

        }
    }
}


