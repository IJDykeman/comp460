using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class TileMap
    {
        public static int decideTilesWithinWidth = 15;
        public static int alwaysMeshWithinRange = (int)(.8*decideTilesWithinWidth * WorldGenParamaters.tileWidth / Chunk.chunkWidth);
        public static int alwaysUnmeshOutsideRange = (int)(decideTilesWithinWidth * 1.2 / WorldGenParamaters.tileWidth * Chunk.chunkWidth * Chunk.chunkWidth);
        System.Collections.Concurrent.ConcurrentDictionary<IntLoc, ProbabilityDistribution> distributions;
        Dictionary<IntLoc, int> tilesDecided;
        static double undecidedEntropy;

        ConcurrentQueue<IntLoc> meshingQueue;

        TileSet tileSet;
        ChunkManager m;
        public static Vector3 playerPerspectiveLoc = new Vector3();

        public TileMap(TileSet _tiles)
        {
            distributions = new System.Collections.Concurrent.ConcurrentDictionary<IntLoc, ProbabilityDistribution>();
            tilesDecided = new Dictionary<IntLoc, int>();
            meshingQueue = new ConcurrentQueue<IntLoc>();
            tileSet = _tiles;
            undecidedEntropy = ProbabilityDistribution.evenOdds(tileSet.size()).entropy();


        }

        public void placeTile(IntLoc tileSpacePos, int tileIndex, ChunkManager m)
        {
            tilesDecided[tileSpacePos] = tileIndex;
            Tile tile = tileSet.getTile(tileIndex);
            Sphere sphere = tileSet.getSphere(tileIndex);
            if (!distributions[tileSpacePos].isZero())
            {
                multiplyInSphere(sphere, tileSpacePos);
            }


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
                        b.color.R = (byte)MathHelper.Clamp(b.color.R - Globals.random.Next(10), 0, 255);
                        b.color.G = (byte)MathHelper.Clamp(b.color.G - Globals.random.Next(10), 0, 255);
                        b.color.B = (byte)MathHelper.Clamp(b.color.B - Globals.random.Next(5), 0, 255);
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

            placeTile(tileSpaceLoc, tileIndex, m);

            ProbabilityDistribution _;
            distributions.TryRemove(tileSpaceLoc, out _);
        }


        private void undecideAround(IntLoc center, int width)
        {
            List<IntLoc> locs = Globals.gridBFS(width);
            foreach (IntLoc l in locs)
            {
                IntLoc tileSpaceLoc = center + l - new IntLoc(width / 2);
                if (decided(tileSpaceLoc))
                {
                    tilesDecided.Remove(tileSpaceLoc);
                }

            }

            int recalcWidth = width + WorldGenParamaters.sphereWidth;
            List<IntLoc> recalcDisLocs = Globals.gridBFS(recalcWidth);
            foreach (IntLoc l in recalcDisLocs)
            {
                IntLoc tileSpaceLoc = center + l - new IntLoc(recalcWidth / 2);
                if (!decided(tileSpaceLoc))
                {
                    distributions[tileSpaceLoc] = calculateDistributionAt(tileSpaceLoc);
                }
            }


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
            for (int i = 0; i < WorldGenParamaters.sphereWidth; i++)
            {
                for (int j = 0; j < WorldGenParamaters.sphereWidth; j++)
                {
                    for (int k = 0; k < WorldGenParamaters.sphereWidth; k++)
                    {
                        IntLoc location = new IntLoc(i, j, k) + center - new IntLoc(WorldGenParamaters.sphereWidth / 2);
                        if (!decided(location))
                        {
                            ProbabilityDistribution d = getDistributionAt(location) * sphere.get(i, j, k);
                            setDistributionAt(location, d);
                        }
                    }
                }
            }
        }

        public ProbabilityDistribution calculateDistributionAt(IntLoc center)
        {
            ProbabilityDistribution result = ProbabilityDistribution.evenOdds(tileSet.size());
            for (int i = 0; i < WorldGenParamaters.sphereWidth; i++)
            {
                for (int j = 0; j < WorldGenParamaters.sphereWidth; j++)
                {
                    for (int k = 0; k < WorldGenParamaters.sphereWidth; k++)
                    {
                        IntLoc locationOfOtherSphereCenter = new IntLoc(i, j, k) + center - new IntLoc(WorldGenParamaters.sphereWidth / 2);
                        if (decided(locationOfOtherSphereCenter))
                        {

                            IntLoc otherSphereIndex = (center - locationOfOtherSphereCenter) + new IntLoc(WorldGenParamaters.sphereWidth / 2);
                            result = tileSet.getSphere(tilesDecided[locationOfOtherSphereCenter]).get(otherSphereIndex.i, otherSphereIndex.j, otherSphereIndex.k) * result;
                        }
                    }
                }
            }
            return result;
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


        private IntLoc approximatelyBestUndecidedLocation(IntLoc center, List<IntLoc> toChooseFrom)
        {
            if (toChooseFrom.Count == 0) {
            }
            double lowest = 10000000;
            IntLoc best = new IntLoc();
            for (int i = 0;i<200;i++)
            {
                IntLoc loc = toChooseFrom.ElementAt(Globals.random.Next(0, toChooseFrom.Count));
                //double e = distributions[loc].entropy()
                float distance = IntLoc.EuclideanDistance(loc, center / WorldGenParamaters.tileWidth);
                double e = distributions[loc].entropy() + Math.Pow((IntLoc.EuclideanDistance(loc, center / WorldGenParamaters.tileWidth)) * .0001f, 1);
                if (e < lowest)
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


        public ConcurrentQueue<IntLoc> getQueueFromBFS(int width)
        {
            ConcurrentQueue<IntLoc> q = new ConcurrentQueue<IntLoc>();
            foreach (IntLoc BFSloc in Globals.gridBFS(width))
            {
                q.Enqueue(BFSloc);
            }
            return q;
        }

        public ChunkManager getManager()
        {
            m = new ChunkManager();
            for (int i = 0; i < 20; i++)
            {
                int x = Globals.random.Next(-decideTilesWithinWidth, decideTilesWithinWidth);
                int y = Globals.random.Next(-decideTilesWithinWidth, decideTilesWithinWidth);
                int z = Globals.random.Next(-decideTilesWithinWidth, decideTilesWithinWidth);
                //placeTile(new IntLoc(x, y, z), 0, m);
                decide(new IntLoc(x, y, z));
            }
            decide(new IntLoc(0));
            int width = 4;
            foreach (IntLoc l in Globals.gridBFS(width))
            {
                IntLoc toDecide = new IntLoc(-width / 2) + l;
                if (!decided(toDecide))
                {
                    decide(toDecide);
                }
            }
            m.remeshAllSerial();

            /*for (int i = 0; i < 5; i++)
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
            */

            new Thread(() =>
            {
                while (true)
                {
                    decideAroundPlayer();
                    remeshAroundPlayer();
                    m.unmeshOutsideRange();
                }
            }).Start();
            

            return m;
        }




        public void report()
        {
            Console.WriteLine(m.getReport());
        }


        void remeshAroundPlayer()
        {
            int meshRadius = alwaysMeshWithinRange;
            ConcurrentQueue<IntLoc> meshingQueue = getQueueFromBFS(meshRadius * 2);
            //m.remeshAllParallelizeableStep(decided);
            IntLoc centerChunkPos = new IntLoc(TileMap.playerPerspectiveLoc / Chunk.chunkWidth);
            IntLoc toMeshChunkLoc;

            IntLoc BFSloc;
            while (meshingQueue.TryDequeue(out BFSloc))
            {
                toMeshChunkLoc = (BFSloc + centerChunkPos - new IntLoc(meshRadius)) * Chunk.chunkWidth;
                if (m.chunkNeedsMesh(toMeshChunkLoc))
                {
                    m.remesh(m, toMeshChunkLoc);
                }



            }
        }

        public void decideAroundPlayer()
        {
            //decideAroundBySampling(3);
            //decideAroundBySampling(7);
            decideAroundBySampling(decideTilesWithinWidth);

            return;

            ConcurrentQueue<IntLoc> decidingQueue = getQueueFromBFS(decideTilesWithinWidth);
            List<IntLoc> tileLocsToDecide = new List<IntLoc>();
            IntLoc toDecideLoc;
            IntLoc tileSpacePos = new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth);
            IntLoc l;
            while (decidingQueue.TryDequeue(out l))
            {
                toDecideLoc = new IntLoc(-decideTilesWithinWidth / 2) + l + tileSpacePos;
                if (!decided(toDecideLoc) && distributions.ContainsKey(toDecideLoc))
                {
                    tileLocsToDecide.Add(toDecideLoc);
                }
            }
            //tileLocsToDecide = tileLocsToDecide.OrderBy(a => IntLoc.EuclideanDistance(a,new IntLoc(playerPerspectiveLoc))).ToList();
            //tileLocsToDecide = tileLocsToDecide.OrderBy(a => distributions[a].entropy()).Take(10).ToList();
            //tileLocsToDecide = tileLocsToDecide.Take(50).ToList();
            while (tileLocsToDecide.Count > 0)
            {
                IntLoc toDecide = tileLocsToDecide[0];
                //IntLoc toDecide = tileLocsToDecide.Find(b => distributions[b].entropy() == tileLocsToDecide.Min(a => distributions[a].entropy()));
                //IntLoc toDecide = lowestEntropyUndecidedLocation();
                if (distributions[toDecide].isZero())
                {
                    undecideAround(toDecide, 2);
                }
                else
                {
                    decide(toDecide);
                }
                tileLocsToDecide.Remove(toDecide);
                //break;

            }
        }

        private void decideAroundBySampling(int radius_in_tiles)
        {
            IntLoc center = new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth);
            //double e = 

            List<IntLoc> close = new List<IntLoc>(distributions.Keys.Where(
                            x => IntLoc.EuclideanDistance(x, new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth)) < radius_in_tiles));

            close = close.OrderBy(x => distributions[x].entropy() + Math.Pow((IntLoc.EuclideanDistance(x, center)) * .0f, 1)).Take(10).ToList();
            

            for (int i = 0; i < 10; i++)
            {
                if (close.Count > 0)
                {
                    //IntLoc toDecide = approximatelyBestUndecidedLocation(new IntLoc(playerPerspectiveLoc), close);
                    IntLoc toDecide = close[0];
                    close.Remove(toDecide);
                    if (distributions[toDecide].isZero())
                    {
                        undecideAround(toDecide, 3);
                    }
                    else
                    {
                        decide(toDecide);
                    }
                }

            }
        }

        public void notifyOfPlayerLocation(Vector3 l)
        {
            TileMap.playerPerspectiveLoc = new Vector3();
            //TileMap.playerPerspectiveLoc = l;
        }
    }
}

