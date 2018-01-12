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
        Dictionary<IntLoc, int> tilesDecided;
        TileSet tileSet;

        public TileMap(TileSet _tiles)
        {
            distributions = new Dictionary<IntLoc, ProbabilityDistribution>();
            tilesDecided = new Dictionary<IntLoc, int>();
            tileSet = _tiles;
        }

        public void placeTile(IntLoc tileSpacePos, int tileIndex, ChunkManager m)
        {
            tilesDecided[tileSpacePos] = tileIndex;
            Tile tile = tileSet.getTile(tileIndex);
            Sphere sphere = tileSet.getSphere(tileIndex);
            multiplyInSphere(sphere, tileSpacePos);
            tileSpacePos = placeBlocksFromTile(tileSpacePos, m, tile);
        }

        private static IntLoc placeBlocksFromTile(IntLoc tileSpacePos, ChunkManager m, Tile tile)
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
            return tileSpacePos;
        }

        private void decide(IntLoc loc, ChunkManager m)
        {
            ProbabilityDistribution d =  distributions[loc];
            int tileIndex = d.sample();
            tilesDecided[loc] = tileIndex;
            multiplyInSphere(tileSet.getSphere(tileIndex), loc);
            placeTile(loc, tileIndex, m);
        }

        private bool decided(IntLoc loc)
        {
            return tilesDecided.ContainsKey(loc);
        }

        private ProbabilityDistribution getDistributionAt(IntLoc loc)
        {
            if (!distributions.ContainsKey(loc))
            {
                distributions[loc] = ProbabilityDistribution.evenOdds(tileSet.size());
            }
            return distributions[loc];
        }

        private void setDistributionAt(IntLoc loc, ProbabilityDistribution d)
        {
            distributions[loc] = d;
        }

        public void multiplyInSphere(Sphere sphere, IntLoc center)
        {
            for (int i = 0; i < WorldGenParamaters.sphereWidth; i++)
            {
                for (int j = 0; j < WorldGenParamaters.sphereWidth; j++)
                {
                    for (int k = 0; k < WorldGenParamaters.sphereWidth; k++)
                    {
                        IntLoc location = new IntLoc(i, j, k) + center - new IntLoc(WorldGenParamaters.sphereWidth  / 2);
                        ProbabilityDistribution d = getDistributionAt(location) * sphere.get(i, j, k);
                        setDistributionAt(location, d);
                    }
                }
            }
        }

        private IntLoc lowestEntropyUndecidedLocation()
        {
            double lowest = 10000000;
            IntLoc best = new IntLoc();
            foreach (IntLoc loc in distributions.Keys)
            {
                double e = distributions[loc].entropy();
                if (!decided(loc) && e < lowest)
                {
                    lowest = e;
                    best = loc;
                }
            }
            return best;
        }

        public void placeATile(ChunkManager m)
        {
            IntLoc toPlace = lowestEntropyUndecidedLocation();
            decide(toPlace, m);
        }

        public ChunkManager getManager()
        {
            ChunkManager m = new ChunkManager();
            placeTile(new IntLoc(0, 0, 1), 1, m);
            placeTile(new IntLoc(0, 1, 0), 0, m);
            //for (int i = 0; i < 15; i++)
            //{
            //    placeATile(m);
            //}
            MyMatrix octor = tileSet.getTransitionMatrix(new IntLoc(0, 1, 0));

            foreach (IntLoc l in Globals.gridBFS(0, 0, 0, 13))
            {
                ProbabilityDistribution p = distributions[l];
                decide(l, m);
            }
            m.remeshAll();
            return m;
            
        }
    }
}
