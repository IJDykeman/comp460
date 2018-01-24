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
        public static int decideTilesWithinWidth = 20;
        public static int alwaysMeshWithinRange = (int)(.8*decideTilesWithinWidth * WorldGenParamaters.tileWidth / Chunk.chunkWidth);
        public static int alwaysUnmeshOutsideRange = (int)(decideTilesWithinWidth * 1.2 / WorldGenParamaters.tileWidth * Chunk.chunkWidth * Chunk.chunkWidth);
        System.Collections.Concurrent.ConcurrentDictionary<IntLoc, ProbabilityDistribution> distributions;
        Dictionary<IntLoc, int> tilesDecided;

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
            else
            {

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

        public void update()
        {
            /*IntLoc i;
            while (meshingQueue.TryDequeue(out i))
            {
                //clear queue
            }
            foreach (IntLoc BFSloc in Globals.gridBFS(TileMap.decideTilesWithinWidth))//.OrderBy(a=>Globals.random.Next()))
            {
                meshingQueue.Enqueue(BFSloc);
            }*/
            //meshingQueue
            

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
            ConcurrentQueue<IntLoc> decidingQueue = getQueueFromBFS(decideTilesWithinWidth);

            IntLoc toDecideLoc;
            IntLoc tileSpacePos = new IntLoc(playerPerspectiveLoc / WorldGenParamaters.tileWidth);
            bool decidedOne = false;
            IntLoc l;
            while(decidingQueue.TryDequeue(out l))
            {
                toDecideLoc = new IntLoc(-decideTilesWithinWidth / 2) + l + tileSpacePos;
                if (!decided(toDecideLoc))
                {
                    //tileLocsToDecide.Add(toDecideLoc);
                    decide(toDecideLoc);
                    decidedOne = true;
                }
            }
            if (!decidedOne)
            {
                //Thread.Sleep(1);
            }



        }

        public void notifyOfPlayerLocation(Vector3 l)
        {
            TileMap.playerPerspectiveLoc = l;
        }
    }
}

