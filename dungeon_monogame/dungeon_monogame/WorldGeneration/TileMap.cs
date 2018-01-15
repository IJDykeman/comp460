using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class TileMap
    {
        System.Collections.Concurrent.ConcurrentDictionary<IntLoc, ProbabilityDistribution> distributions;
        Dictionary<IntLoc, int> tilesDecided;
        TileSet tileSet;
        ChunkManager m;
        public static Vector3 playerPerspectiveLoc = new Vector3();

        public TileMap(TileSet _tiles)
        {
            distributions = new System.Collections.Concurrent.ConcurrentDictionary<IntLoc, ProbabilityDistribution>();
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
            for (int i = 0; i < WorldGenParamaters.tileWidth - 1; i++)
            {
                for (int j = 0; j < WorldGenParamaters.tileWidth - 1; j++)
                {
                    for (int k = 0; k < WorldGenParamaters.tileWidth - 1; k++)
                    {
                        Block b = tile.get(i, j, k);
                        b.color.R -= (byte)Globals.random.Next(10);
                        b.color.G -= (byte)Globals.random.Next(10);
                        b.color.B -= (byte)Globals.random.Next(5);
                        m.set((tileSpacePos * (WorldGenParamaters.tileWidth - 1)) + new IntLoc(i, j, k), b);
                    }
                }
            }
            return tileSpacePos;
        }

        private void decide(IntLoc tileSpaceLoc)
        {
            ProbabilityDistribution d = getDistributionAt(tileSpaceLoc);
            int tileIndex = d.sample();
            tilesDecided[tileSpaceLoc] = tileIndex;
            multiplyInSphere(tileSet.getSphere(tileIndex), tileSpaceLoc);
            placeTile(tileSpaceLoc, tileIndex, m);
        }

        private bool decided(IntLoc loc)
        {
            return tilesDecided.ContainsKey(loc);
        }

        private ProbabilityDistribution getDistributionAt(IntLoc loc)
        {
            ProbabilityDistribution p;
            if (distributions.TryGetValue(loc, out p))
            {
                return p;
            }
            p = ProbabilityDistribution.evenOdds(tileSet.size());
            distributions[loc] = p;
            return p;
        }

        private void setDistributionAt(IntLoc loc, ProbabilityDistribution d)
        {
            distributions[loc] = d;
        }

        public void multiplyInSphere(Sphere sphere, IntLoc center)
        {
            //Parallel.For(0, WorldGenParamaters.sphereWidth, i =>
            //{
            for (int i = 0; i < WorldGenParamaters.sphereWidth; i++)
            {
                for (int j = 0; j < WorldGenParamaters.sphereWidth; j++)
                {
                    for (int k = 0; k < WorldGenParamaters.sphereWidth; k++)
                    {
                        IntLoc location = new IntLoc(i, j, k) + center - new IntLoc(WorldGenParamaters.sphereWidth / 2);
                        ProbabilityDistribution d = getDistributionAt(location) * sphere.get(i, j, k);
                        setDistributionAt(location, d);
                    }
                }
                //});
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
            decide(toPlace);
        }

        void remeshParallel()
        {
            m.remeshAllParallelizeableStep(new IntLoc(playerPerspectiveLoc));
        }

        void remeshSerial()
        {
            m.remeshAllSerialStep(new IntLoc(playerPerspectiveLoc));
        }

        public ChunkManager getManager()
        {
            m = new ChunkManager();
            decide(new IntLoc(0));
            int width = 3;
            foreach (IntLoc l in Globals.gridBFS(width / 2, width / 2, width / 2, width))
            {
                IntLoc toDecide = new IntLoc(-width / 2) + l;
                if (!decided(toDecide))
                {
                    decide(toDecide);
                }
            }

            //m.remeshAll();
            for (int i = 0; i < 3; i++)
            {
                new Thread(() =>
                {
                    while (true)
                    {
                        remeshParallel();
                        Thread.Sleep(2);
                    }
                }).Start();
            }

            new Thread(() =>
            {
                while (true)
                {
                    remeshSerial();
                    Thread.Sleep(2);
                }
            }).Start();
            new Thread(() => { keepTilesUpdated(); }).Start();





            return m;
        }

        public void keepTilesUpdated()
        {
            while (true)
            {
                IntLoc tileSpacePlayerPos = new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth);
                decideAround(tileSpacePlayerPos);
                Thread.Sleep(5);
            }
        }

        public void decideAround(IntLoc tileSpacePos)
        {
            int width = 13;
            //IntLoc toDecide = tileSpacePos;
            //if (!decided(toDecide))
            //{
            //    decide(toDecide);
            //}

            foreach (IntLoc l in Globals.gridBFS(width / 2, width / 2, width / 2, width))
            {
                IntLoc toDecide = new IntLoc(-width / 2) + l + tileSpacePos;
                if (!decided(toDecide))
                {
                    decide(toDecide);
                }
            }
            
        }



        public void notifyOfPlayerLocation(Vector3 l)
        {
            TileMap.playerPerspectiveLoc = l;
        }
    }
}

