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
    public class Game1 : Game
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

            WorldGeneration.TileSet tiles = null;
            try
            {
                string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string defaultTilesFolder = Path.Combine(currentDirectory, "Content/21_dungeon");
                var files = getVoxFiles(defaultTilesFolder);
                tiles = new WorldGeneration.TileSet(files, false, false);
            }
            catch (Exception e)
            {
                tiles = LoadNewTilesFromDialog(false);
            }
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
            int i=0;
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
            map.addChild(player.getActor());
            map.addChild(new Slime(new Vector3(12, 20, 12)));
            Console.WriteLine("global seed is " + Globals.getSeed().ToString());


        }

        private static string[] getVoxFiles(string root)
        {
            List<string> allFiles = new List<string>(Directory.GetFiles(root));
            return allFiles.Where(x => x.ToLower().EndsWith(".vox")).ToArray();

        }

        private static WorldGeneration.TileSet LoadNewTilesFromDialog(bool isExampleBased)
        {
            bool userWantsFlatWorld = false;
            try
            {
                string[] files;
                if (isExampleBased)
                {
                    string examplePath = FileManagement.OpenFileDialog();
                    files = new string[] { examplePath };
                }
                else
                {
                    string tileSetDir = FileManagement.getDirectoryFromDialogue();
                    userWantsFlatWorld = FileManagement.AskWhetherWorldIsFlat();

                    files = getVoxFiles(tileSetDir);

                }
                try
                {
                    WorldGeneration.TileSet tiles = new WorldGeneration.TileSet(files, isExampleBased, userWantsFlatWorld);
                    return tiles;

                }
                catch (InvalidTilesetException e)
                {

                    if (!userWantsFlatWorld)
                    {
                        WorldGeneration.TileSet tiles = new WorldGeneration.TileSet(files, isExampleBased, true);
                        // this tileset generation succeeds with a flat world where it failed with a deep one
                        // This means we can report to the user that they should be setting their world to flat.
                        throw new InvalidTilesetException("This tile set isn't valid for a world that is more than one tile high.  " +
                                                          "Perhaps you tried to create a landscape tile set without tiles to go below the ground or above the surface tiles.  " +
                                                          "If you intend to have a world that is only one tile in height, please select \"one tile high world\" when you are selecting the tile set folder to load.");
                    }
                    

                }



            }
            catch (InvalidTilesetException e)
            {
                FileManagement.ShowDialog(e.Message, "This isn't a valid tile set.");
            }

            FileManagement.ShowDialog("This folder does not contain a valid tile set.", "Error");
            return null;

        }

        public void loadNewTileset(bool exampleBased)
        {
            WorldGeneration.TileSet tiles = LoadNewTilesFromDialog(exampleBased);
            if (tiles != null)
            {
                map.resetTileMap(tiles, tiles.worldAssumedFlat);
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
            base.Draw(gameTime);

        }
    }
}


