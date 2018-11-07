﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class TileMap : GameObjectModel
    {
        public static readonly int alwaysMeshWithinRange = WorldGenParamaters.MeshWithinBlockRange; 
        public static readonly int alwaysUnmeshOutsideRange = (int)(alwaysMeshWithinRange * 1.5f);
        System.Collections.Concurrent.ConcurrentDictionary<IntLoc, Domain> distributions;
        Dictionary<IntLoc, int> tilesDecided;

        ConcurrentQueue<IntLoc> meshingQueue;

        TileSet tileSet;
        public static Vector3 playerPerspectiveLoc = new Vector3();

        internal ChunkManager getChunkManager()
        {
            return chunkManager;
        }

        public TileMap(TileSet _tiles, Vector3 loc, Vector3 scale) : base(loc, scale)
        {
            
            distributions = new System.Collections.Concurrent.ConcurrentDictionary<IntLoc, Domain>();
            tilesDecided = new Dictionary<IntLoc, int>();
            meshingQueue = new ConcurrentQueue<IntLoc>();
            tileSet = _tiles;
            chunkManager = getManager();


        }

        public void placeTile(IntLoc tileSpacePos, int tileIndex, ChunkManager m)
        {
            tilesDecided[tileSpacePos] = tileIndex;
            Tile tile = tileSet.getTile(tileIndex);
            Sphere sphere = tileSet.getSphere(tileIndex);
            multiplyInSphere(sphere, tileSpacePos);
            placeBlocksFromTile(tileSpacePos, m, tile);
            postprocess(tileSpacePos);
        }

        protected virtual void postprocess(IntLoc tileSpacePos)
        {
        }

        private static void placeBlocksFromTile(IntLoc tileSpacePos, ChunkManager m, Tile tile)
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
                            b.color.R = (byte)MathHelper.Clamp(b.color.R - Globals.random.Next(6), 0, 255);
                            b.color.G = (byte)MathHelper.Clamp(b.color.G - Globals.random.Next(6), 0, 255);
                            b.color.B = (byte)MathHelper.Clamp(b.color.B - Globals.random.Next(3), 0, 255);
                            m.set((tileSpacePos * (WorldGenParamaters.tileWidth - 1)) + new IntLoc(i, j, k), b);
                        }
                    }
                }
            }
        }

        private void decide(IntLoc tileSpaceLoc)
        {
            Domain d = getDistributionAt(tileSpaceLoc);
            int tileIndex = d.getRandomTrueIndex();
            placeTile(tileSpaceLoc, tileIndex, chunkManager);

            Domain _;
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

        private Domain getDistributionAt(IntLoc loc)
        {
            Domain p;
            if (distributions.TryGetValue(loc, out p))
            {
                return p;
            }
            p = Domain.allTrue(tileSet.size());
            return p;
        }

        private void setDistributionAt(IntLoc loc, Domain d)
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
                        if (!decided(location) && (location.j == 0 || !WorldGenParamaters.onlyOneHorizontalLevel) && (location.i == 0 || !WorldGenParamaters.onlyOneVerticalLevel))
                        {
                            Domain d = getDistributionAt(location) * (sphere.get(i, j, k));
                            if(d.sum() == 0)
                            {
                                d = Domain.allTrue(d.k());
                            }
                            setDistributionAt(location, d);

                        }
                    }
                }
            }
        }

        private bool undecidedTilesAreInTheAlwaysRegion()
        {
            foreach (IntLoc loc in getQueueFromBFS(WorldGenParamaters.decideTilesWithinWidth * 2))
            {
                if (!decided(loc))
                {
                    return true;
                }
            }
            return false;
            
        }

        private float decisionUrgency(Domain d, IntLoc tileLoc, IntLoc perspectiveTileLoc)
        {
            float urgency = -d.sum();

            if (IntLoc.EuclideanDistance(perspectiveTileLoc, tileLoc) < WorldGenParamaters.decideTilesWithinWidth)
            {
                urgency += 1000;
            }
            return urgency;
        }


        private IntLoc? lowestEntropyUndecidedLocation()
        {
            IntLoc snapped_player_loc = new IntLoc(TileMap.playerPerspectiveLoc / WorldGenParamaters.tileWidth);

            if (!decided(snapped_player_loc) && !(WorldGenParamaters.onlyOneHorizontalLevel || WorldGenParamaters.onlyOneVerticalLevel))
            {
                return snapped_player_loc;
            }
            double greatestUrgency = 10000000;
            IntLoc? best = null;
            if (undecidedTilesAreInTheAlwaysRegion())
            {
                foreach (IntLoc loc in distributions.Keys)
                {
                    //if(!decided(loc)){
                    Domain d = getDistributionAt(loc);
                    float distance = IntLoc.EuclideanDistance(loc, snapped_player_loc);
                    double e = decisionUrgency(d, loc, snapped_player_loc);
                    bool isBetter = e > greatestUrgency;
                    if ((!best.HasValue || isBetter) && distance < WorldGenParamaters.doNotGenerateOutsideRadius)
                    {
                        greatestUrgency = e;
                        best = loc;
                        if (d.sum() == 1)
                        {
                                return best;
                        }
                    }
                    //}
                }

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
            chunkManager = new ChunkManager();

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
            decide(new IntLoc());
            new Thread(() =>
            {
                
                while (true)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (stopwatch.ElapsedMilliseconds < 50)
                    {
                    placeATile(chunkManager);
                    }
                    remeshAroundPlayer();
                    chunkManager.unmeshOutsideRange();
                    
                }
            }).Start();


            return chunkManager;
        }

        public void report()
        {
            Console.WriteLine(chunkManager.getReport());
        }


        void remeshAroundPlayer()
        {
            int meshRadius = alwaysMeshWithinRange / Chunk.chunkWidth;
            ConcurrentQueue<IntLoc> chunksNearPlayer = getQueueFromBFS(meshRadius * 2);
            //m.remeshAllParallelizeableStep(decided);
            IntLoc centerChunkPos = new IntLoc(TileMap.playerPerspectiveLoc / Chunk.chunkWidth);
            IntLoc toMeshChunkLoc;

            IntLoc BFSloc;
            while (chunksNearPlayer.TryDequeue(out BFSloc))
            {
                toMeshChunkLoc = (BFSloc + centerChunkPos - new IntLoc(meshRadius)) * Chunk.chunkWidth;
                if (chunkManager.chunkNeedsMesh(toMeshChunkLoc))
                {
                    chunkManager.remesh(chunkManager, toMeshChunkLoc);
                }

            }
        }




        public void notifyOfPlayerLocation(Vector3 l)
        {
            TileMap.playerPerspectiveLoc = l;
        }

    }
}

