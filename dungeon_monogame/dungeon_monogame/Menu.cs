using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    abstract class Menu : InputHandler
    {
        protected List<Button> buttons;
        protected Point size;

        string defaultCaption = "Welcome to Generate Worlds\n"
                                + "[esc]    open menu\n"
                                + "[up arrow]    increase ambient light\n"
                                + "[down arrow]    reduce ambient light\n"
                                + "[right click]    launch a light\n"
                                + "[left ctrl]    enter/exit flying mode\n"
                                + "[Space]    Jump while walking, or while flying, go up\n"
                                + "[left shift]    while flying, go down\n"
                                + "[M]    spawn a slime\n";


        string caption = "";

        public Menu()
        {
            buttons = new List<Button>();

        }


        public override List<Action> handleInput()
        {
            caption = defaultCaption;
            List<Action> result = new List<Action>();
            KeyboardState newState = Keyboard.GetState();
            MouseState newMouseState = Mouse.GetState();

            if (justHit(GlobalSettings.OpenMainMenuKey, newState))
            {
                result.Add(new ToggleMainMenu());
            }

            if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
            {
                foreach (Button b in buttons)
                {
                    if (b.isMousedOver(new Vector2(newMouseState.Position.X, newMouseState.Position.Y)))
                    {
                        result.AddRange(b.clicked());
                    }
                }
            }

            foreach (Button b in buttons)

            {
                b.currentlyMousedOver = false;
                if (b.isMousedOver(new Vector2(newMouseState.Position.X, newMouseState.Position.Y)))
                {

                    b.mousedOver();
                    b.currentlyMousedOver = true;
                    caption = b.getToolTip();
                }
            }

            // last things
            oldMouseState = Mouse.GetState();
            oldKeyboardState = Keyboard.GetState();
            return result;
        }


        public void updateDimensions(Point _size)
        {
            size = _size;
        }

        public virtual void draw(GraphicsDeviceManager graphics)
        {
            drawBackground(graphics);

            foreach (Button b in buttons)
            {
                b.draw(graphics);
            }

            string captionToShow = TextBox.getWrappedContent(caption, DungeonContentManager.menuFont, new Point(size.X / 2, size.Y));
            using (SpriteBatch spriteBatch = new SpriteBatch(graphics.GraphicsDevice))
            {
                spriteBatch.Begin();
                Color color = GlobalSettings.defaultButtonColor;
                spriteBatch.DrawString(DungeonContentManager.menuFont, captionToShow, new Vector2(size.X / 2, 50), color);
                spriteBatch.End();
            }

        }

        private static void drawBackground(GraphicsDeviceManager graphics)
        {
            Texture2D dummyTexture = new Texture2D(graphics.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { new Color(0, 0, 0, .5f) });
            using (SpriteBatch spriteBatch = new SpriteBatch(graphics.GraphicsDevice))
            {
                spriteBatch.Begin();
                Rectangle rect = new Rectangle(new Point(0, 0), new Point(10000, 10000));
                spriteBatch.Draw(dummyTexture, rect, new Color(1, 1, 1, .5f));
                spriteBatch.End();
            }

        }
    }
    class TextBox
    {

        public static string getWrappedContent(string content, SpriteFont font, Point dimensions)
        {
            List<string> lines = new List<string>();
            string[] words = content.Split(" ".ToArray());
            string currentLine = "";
            foreach (string word in words)
            {
                string candidateLine = currentLine + " " + word;
                if (font.MeasureString(candidateLine).X > dimensions.X -10)
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = candidateLine;
                }
            }
            lines.Add(currentLine);
            return string.Join("\n", lines);
            
        }


        


    }





    class MainMenu : Menu
    {
        public MainMenu() : base()
        {
            // buttons.Add(new LoadExampleButton());
            buttons.Add(new LoadTileSetButton());
            buttons.Add(new ExportModelButton());
            buttons.Add(new SetPlayerScaleButton());
            buttons.Add(new QuitButton());
            for (int i =0; i<buttons.Count; i++)
            {
                buttons[i].setLocationBasedOnIndex(i);
            }

        }

    }

    class Button
    {
        protected string tooltip = "";
        protected string text = "";
        protected Vector2 location;
        protected Vector2 size;
        public bool currentlyMousedOver = false;
        protected int offsetFromLeft = 50;
        protected int offsetFromTop = 50;

        public Button()
        {
            location = new Vector2(100, 100);
            size = new Vector2(100, 100);
        }

        public string getToolTip(){ return tooltip; }

        public bool isMousedOver(Vector2 mouseLocation)
        {
            Vector2 offset = (mouseLocation - location);
            return offset.X < size.X && offset.Y < size.Y && offset.X > 0 && offset.Y > 0;
        }

        virtual public List<Action> clicked()
        {
            return new List<Action>();
        }

        virtual public void mousedOver()
        {
            
        }

        public void setLocationBasedOnIndex(int index){
            location = new Vector2(offsetFromLeft, offsetFromLeft * (index + 1));
            size = DungeonContentManager.menuFont.MeasureString(text) + new Vector2(5, 5);
        }


        public void draw(GraphicsDeviceManager graphics)
        {
            using (SpriteBatch spriteBatch = new SpriteBatch(graphics.GraphicsDevice))
            {
                spriteBatch.Begin();
                Color color = currentlyMousedOver ? GlobalSettings.mousedOVerButtonColor: GlobalSettings.defaultButtonColor;
                spriteBatch.DrawString(DungeonContentManager.menuFont, text, location, color);
                spriteBatch.End();
            }
        }

    }

    class LoadExampleButton : Button
    {
        public LoadExampleButton()
        {
            text = "Load Example Model (experimental)";
            tooltip = "Select a .vox model to load.  It will be automagically turned into a tile set, and an infinite world will be generated from that tile set.  "
                      + "To turn a tile into a tileset, the tile is divided into pieces of size 3x3x3 which become tiles in the tile set.  This tileset should "
                      + "produce worlds that look like that original tile, although this depends heavily on the original input.";
        }
        public override List<Action> clicked()
        {
            List<Action> result = new List<Action>();
            result.Add(new RequestTilesetLoad(true));
            return result;
        }
    }

    class LoadTileSetButton : Button
    {
        public LoadTileSetButton()
        {
            text = "Load Tile Set";
            tooltip = "Select a folder containing a new tile set.  The world is then reset and regenerated using that new tile set.  " +
                      "The folder you select must contain .vox files for each tile.  These tiles must all be the same size, and that size must be odd-numbered.  " +
                      "For instance, you can have tiles that are each a 5x5x5 voxel model.";
        }
        public override List<Action> clicked()
        {
            List<Action> result = new List<Action>();
            result.Add(new RequestTilesetLoad(false));
            return result;
        }
    }

    class ExportModelButton : Button
    {
        public ExportModelButton()
        {
            text = "Export .obj model";
            tooltip = "Save a .obj file of the world you are currently exploring.  This common format can then be imported into other 3D applications, such as Blender, Maya, Unity, and Unreal Engine.";
        }
        public override List<Action> clicked()
        {
            List<Action> result = new List<Action>();
            result.Add(new ExportModelAction());
            return result;
        }
    }

    class SetPlayerScaleButton : Button
    {
        public SetPlayerScaleButton()
        {
            text = "Change player size";
            tooltip = "This lets you scale your avatar to be larger or smaller so that you move comfortably around in worlds of different sizes.";
            

        }
        public override List<Action> clicked()
        {
            List<Action> result = new List<Action>();
            result.Add(new SetPlayerScaleAction(FileManagement.getIntFromDialogBox("This number will be a multiplier on the default size of your avatar.  The defualt value is 1.",
                                                                                    "Select a player scale")));
            return result;
        }
    }

    class QuitButton : Button
    {
        public QuitButton()
        {
            text = "Quit";
            tooltip = "Exit application.  The world you are exploring will not be automatically saved, but another world like it can be generated from the same tiles.";
        }
        public override List<Action> clicked()
        {
            List<Action> result = new List<Action>();
            result.Add(new ExitApplicationAction());
            return result;
        }
    }
}
