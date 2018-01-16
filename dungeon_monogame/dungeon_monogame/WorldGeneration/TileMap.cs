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

            //m.remeshAllParallelizeableStep(decided);
            while (true)
            {
                IntLoc centerTilePos = new IntLoc(TileMap.playerPerspectiveLoc / WorldGenParamaters.tileWidth);
                IntLoc toMeshTileLoc;

                foreach (IntLoc BFSloc in Globals.gridBFS(TileMap.decideTilesWithinWidth))
                {
                    toMeshTileLoc = new IntLoc(-TileMap.decideTilesWithinWidth / 2) + BFSloc + centerTilePos;
                    if (decided(toMeshTileLoc))
                    {
                        IntLoc ToMeshChunkLoc = ChunkManager.locToChunkLoc(toMeshTileLoc * WorldGenParamaters.tileWidth);
                        if (m.chunkNeedsMesh(ToMeshChunkLoc))
                        {
                            m.remesh(m, ToMeshChunkLoc);
                            break;
                        }

                    }

                }
                Thread.Sleep(2);

            }
        }

        public ChunkManager getManager()
        {
            m = new ChunkManager();
            decide(new IntLoc(0));
            int width = 3;
            foreach (IntLoc l in Globals.gridBFS(width))
            {
                IntLoc toDecide = new IntLoc(-width / 2) + l;
                if (!decided(toDecide))
                {
                    decide(toDecide);
                }
            }

            //m.remeshAll();
            for (int i = 0; i < 1; i++)
            {
                new Thread(() =>
                {
                   remeshParallel();
                }).Start();
            }

            new Thread(() =>
            {
                unmeshFarawayTiles();
            }).Start();
            new Thread(() => { keepTilesUpdated(); }).Start();





            return m;
        }

        public void keepTilesUpdated()
        {
                decideAround();
        }
        public static int decideTilesWithinWidth = 12;
        public static int alwaysMeshWithinRange = decideTilesWithinWidth * WorldGenParamaters.tileWidth / 2;
        public static int alwaysUnmeshOutsideRange = (int)(alwaysMeshWithinRange * 1.5);

        public void unmeshFarawayTiles()
        {
            while (true)
            {
                m.unmeshOutsideRange();
                Thread.Sleep(5);
            }
        }


        public void report()
        {
            Console.WriteLine(m.getReport());
        }


        public void decideAround()
        {
            IntLoc toDecideLoc;
            while (true)
            {
                IntLoc tileSpacePos = new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth);
                bool decidedOne = false;
                foreach (IntLoc l in Globals.gridBFS(decideTilesWithinWidth))
                {
                    toDecideLoc = new IntLoc(-decideTilesWithinWidth / 2) + l + tileSpacePos;
                    if (!decided(toDecideLoc))
                    {
                        //tileLocsToDecide.Add(toDecideLoc);
                        decide(toDecideLoc);
                        decidedOne = true;
                        break;
                    }
                }
                if (!decidedOne)
                {
                    Thread.Sleep(1);
                }
            }

            /*
            List<IntLoc> toDecide;
            if(tileLocsToDecide.TakeLeastKInLinearTime(a => IntLoc.EuclideanDistance(tileSpacePos, a), 10, out toDecide))
            {
                foreach (IntLoc loc in toDecide)
                {
                    if (!decided(loc))
                    {
                        decide(loc);

                    }
                }

            }*/

        }



        public void notifyOfPlayerLocation(Vector3 l)
        {
            TileMap.playerPerspectiveLoc = l;
        }
    }
}

