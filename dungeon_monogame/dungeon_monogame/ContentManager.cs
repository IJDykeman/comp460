using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class DungeonContentManager
    {
        public static SpriteFont menuFont;
        public static  void loadContent(ContentManager Content)
        {
            menuFont = Content.Load<SpriteFont>("menuFont");

        }
    }
}
