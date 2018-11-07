using dungeon_monogame.WorldGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class World : GameObject
    {
        public TileMap map;
        List<IntLoc> tileSpaceLocsNeedingPostprocess;

        public World(TileSet _tiles)
        {
            map = new TileMap(_tiles);
            tileSpaceLocsNeedingPostprocess = new List<IntLoc>();
        }


        protected void postprocess(IntLoc tileSpacePos)
        {
            tileSpaceLocsNeedingPostprocess.Add(tileSpacePos);
        }

        public void update()
        {
            // place actors in tiles that need postprocessing   
        }


    }
}
