using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class TileMap
    {
        Dictionary<IntLoc, ProbabilityDistribution> distributions;
        TileSet tiles;

        public TileMap(TileSet _tiles)
        {
            distributions = new Dictionary<IntLoc, ProbabilityDistribution>();
            tiles = _tiles;
        }

        public void placeTile(IntLoc tileSpacePos, Tile tile, ChunkManager m)
        {
            for (int i = 0; i < WorldGenParamaters.tileWidth; i++)
            {
                for (int j = 0; j < WorldGenParamaters.tileWidth; j++)
                {
                    for (int k = 0; k < WorldGenParamaters.tileWidth; k++)
                    {
                        m.set((tileSpacePos * WorldGenParamaters.tileWidth) + new IntLoc(i, j, k), tile.get(i, j, k));
                    }
                }
            }
        }

        public ChunkManager getManager()
        {
            ChunkManager m = new ChunkManager();
            for (int i = 0; i < 5; i++)
            {
                placeTile(new IntLoc(i,0,1), tiles.getTile(i), m);
            }
            m.remeshAll();
            return m;
            
        }
    }
}
