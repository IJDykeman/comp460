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
        public static int alwaysMeshWithinRange = (int)(.8 * WorldGenParamaters.decideTilesWithinWidth * WorldGenParamaters.tileWidth / Chunk.chunkWidth);
        public static int alwaysUnmeshOutsideRange = (int)(WorldGenParamaters.decideTilesWithinWidth * 1.5 / WorldGenParamaters.tileWidth * Chunk.chunkWidth * Chunk.chunkWidth);
        System.Collections.Concurrent.ConcurrentDictionary<IntLoc, WorldTileDistribution> distributions;
        Dictionary<IntLoc, int> tilesDecided;
        static double undecidedEntropy;

        ConcurrentQueue<IntLoc> meshingQueue;

        TileSet tileSet;
        ChunkManager m;
        public static Vector3 playerPerspectiveLoc = new Vector3();

        public TileMap(TileSet _tiles)
        {
            distributions = new System.Collections.Concurrent.ConcurrentDictionary<IntLoc, WorldTileDistribution>();
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
            //if (!distributions[tileSpacePos].isZero())
           // {
                multiplyInSphere(sphere, tileSpacePos);
            //}

            placeBlocksFromTile(tileSpacePos, m, tile);
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
                        if (b.color.R == 252)
                        {
                            continue; // ignore air white
                        }
                        else
                        {
                            b.color.R = (byte)MathHelper.Clamp(b.color.R - Globals.random.Next(5), 0, 255);
                            b.color.G = (byte)MathHelper.Clamp(b.color.G - Globals.random.Next(5), 0, 255);
                            b.color.B = (byte)MathHelper.Clamp(b.color.B - Globals.random.Next(2), 0, 255);
                            m.set((tileSpacePos * (WorldGenParamaters.tileWidth - 1)) + new IntLoc(i, j, k), b);
                        }
                    }
                }
            }
            return tileSpacePos;
        }

        private void decide(IntLoc tileSpaceLoc)
        {
            WorldTileDistribution d = getDistributionAt(tileSpaceLoc);
            int tileIndex = d.sample();
            placeTile(tileSpaceLoc, tileIndex, m);

            WorldTileDistribution _;
            distributions.TryRemove(tileSpaceLoc, out _);
        }

        /*
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


        }*/

        private bool decided(IntLoc loc)
        {
            return tilesDecided.ContainsKey(loc);
        }

        private WorldTileDistribution getDistributionAt(IntLoc loc)
        {
            WorldTileDistribution p;
            if (distributions.TryGetValue(loc, out p))
            {
                return p;
            }
            p = new WorldTileDistribution(ProbabilityDistribution.evenOdds(tileSet.size()));
            distributions[loc] = p;
            return p;
        }

        private void setDistributionAt(IntLoc loc, WorldTileDistribution d)
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
                            WorldTileDistribution d = new WorldTileDistribution ((ProbabilityDistribution)getDistributionAt(location) * (sphere.get(i, j, k)));
                            setDistributionAt(location, d);
                        }
                    }
                }
            }
        }

        /*

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
        }*/


        private IntLoc? lowestEntropyUndecidedLocation()
        {
            double lowest = 10000000;
            IntLoc? best = null;
            Console.WriteLine(distributions.Keys.Count);
            foreach (IntLoc loc in distributions.Keys)
            {
                float distance = IntLoc.EuclideanDistance(loc, new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth));
                double e = distributions[loc].entropy();
                if (!decided(loc) && (!best.HasValue || e < lowest) && distance < WorldGenParamaters.decideTilesWithinWidth)
                {
                    lowest = e;
                    best = loc;
                }
            }
            if (best.HasValue)
            {
                Console.WriteLine(best.Value.i);
            }

            return best;
        }

        /*
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
        }*/

        public void placeATile(ChunkManager m)
        {
            IntLoc? toPlace = lowestEntropyUndecidedLocation();
            if (toPlace.HasValue)
            {
                decide(toPlace.Value);
            }
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
                decide(new IntLoc());
                while (true)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        placeATile(m);
                    }
                    remeshAroundPlayer();
                    m.unmeshOutsideRange();
                    //Console.WriteLine("completed an iteration of deciding and meshing");
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
            ConcurrentQueue<IntLoc> chunksNearPlayer = getQueueFromBFS(meshRadius * 2);
            //m.remeshAllParallelizeableStep(decided);
            IntLoc centerChunkPos = new IntLoc(TileMap.playerPerspectiveLoc / Chunk.chunkWidth);
            IntLoc toMeshChunkLoc;

            IntLoc BFSloc;
            while (chunksNearPlayer.TryDequeue(out BFSloc))
            {
                toMeshChunkLoc = (BFSloc + centerChunkPos - new IntLoc(meshRadius)) * Chunk.chunkWidth;
                if (m.chunkNeedsMesh(toMeshChunkLoc))
                {
                    m.remesh(m, toMeshChunkLoc);
                    //Console.WriteLine("remeshing a chunk near " + centerChunkPos);
                }

            }
        }
        /*
        public void decideAroundPlayer()
        {

            IntLoc center = new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth);
            Console.WriteLine("deciding around " + playerPerspectiveLoc);

            List<IntLoc> locationsToDecide = new List<IntLoc>(distributions.Keys.Where(
                            x => IntLoc.EuclideanDistance(x, new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth)) <= WorldGenParamaters.decideTilesWithinWidth));

            while (locationsToDecide.Count > 0)
            {
                locationsToDecide = locationsToDecide.OrderBy(x => distributions[x].entropy()).Take(1).ToList();
                IntLoc toDecide = locationsToDecide[0];
                locationsToDecide.Remove(toDecide);

                ProbabilityDistribution dist = distributions[toDecide];
                decide(toDecide);
                //Console.WriteLine("deciding at " + toDecide);

            }
            }*/




        public void notifyOfPlayerLocation(Vector3 l)
        {
            TileMap.playerPerspectiveLoc = l;
        }

    }
}

