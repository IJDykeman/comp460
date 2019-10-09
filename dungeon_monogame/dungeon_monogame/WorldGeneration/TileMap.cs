using Microsoft.Xna.Framework;
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

        System.Collections.Concurrent.ConcurrentDictionary<IntLoc, Domain> distributions;
        Dictionary<IntLoc, int> tilesDecided;

        ConcurrentQueue<IntLoc> meshingQueue;
        ConcurrentQueue<IntLoc> postprocessingQueue;

        TileSet tileSet;
        public static Vector3 playerPerspectiveLoc = new Vector3();
        bool assumeFlatWorld;
        public int alwaysMeshWithinRange;
        public int alwaysUnmeshOutsideRange;

        internal ChunkManager getChunkManager()
        {
            return chunkManager;
        }

        public TileMap(TileSet _tiles, Vector3 loc, Vector3 scale, bool isFlatWorld) : base(loc, scale)
        {
            assumeFlatWorld = isFlatWorld;
            distributions = new System.Collections.Concurrent.ConcurrentDictionary<IntLoc, Domain>();
            tilesDecided = new Dictionary<IntLoc, int>();
            meshingQueue = new ConcurrentQueue<IntLoc>();
            postprocessingQueue = new ConcurrentQueue<IntLoc>();
            tileSet = _tiles;
            chunkManager = getManager();
            alwaysMeshWithinRange = 500;
            alwaysUnmeshOutsideRange = (int)(alwaysMeshWithinRange * 1.5f);

        }

        public List<HemisphereLight> getLightsForTileLoc(IntLoc loc)
        {
            int tileWidth = tileSet.getTileWidth();
            List<HemisphereLight> result = new List<HemisphereLight>();
            if (tilesDecided.ContainsKey(loc))
            {
                Tile tile = tileSet.getTile(tilesDecided[loc]);
                for (int i=0; i<tileWidth; i++)
                {
                    for (int p = 0; p < tileWidth; p++)
                    {
                        for (int k = 0; k < tileWidth; k++)
                        {
                            var color = tile.get(i, p, k).color;
                            if (color.R == 255 || color.G == 255 || color.B == 255)
                            {
                                var light = new HemisphereLight(1, color, Vector3.UnitX, ((loc * tileWidth) + new IntLoc(i, p, k)).toVector3());
                                //result.Add();
                                addChild(light);
                            }

                        }
                    }
                    }
            }
            return result;

        }

        public void writeObjFileNearPlayer()
        {
            chunkManager.writeObjFile(new IntLoc(playerPerspectiveLoc));
        }

        public void placeTile(IntLoc tileSpacePos, int tileIndex, ChunkManager m)
        {
            tilesDecided[tileSpacePos] = tileIndex;
            Tile tile = tileSet.getTile(tileIndex);
            Sphere sphere = tileSet.getSphere(tileIndex);
            multiplyInSphere(sphere, tileSpacePos);
            placeBlocksFromTile(tileSpacePos, m, tile);
            concurrentPostprocess(tileSpacePos);
        }

        protected virtual void concurrentPostprocess(IntLoc tileSpacePos)
        {
            postprocessingQueue.Enqueue(tileSpacePos);
            
        }
        protected override List<Action> update()
        {
            for (int i=0; i<postprocessingQueue.Count; i++)
            {
                IntLoc loc;
                //if(postprocessingQueue.TryDequeue(out loc))
                //{
                    //getLightsForTileLoc(loc);
                //}
            }
            return new List<Action>();
        }

        private static void placeBlocksFromTile(IntLoc tileSpacePos, ChunkManager m, Tile tile)
        {
            int tileWidth = tile.tileWidth;
            IntLoc blockSpaceTileLoc = (tileSpacePos * (tileWidth - 1));
            
            for (int i = 0; i < tileWidth - 1; i++)
            {
                for (int j = 0; j < tileWidth - 1; j++)
                {
                    for (int k = 0; k < tileWidth - 1; k++)
                    {
                        Block b = tile.get(i, j, k);
                        if (b.color.A == 0)
                        {
                            continue; // ignore air white
                        }
                        else
                        {
                            IntLoc loc = blockSpaceTileLoc + new IntLoc(i, j, k);
                            b.color.R = (byte)MathHelper.Clamp(b.color.R - Globals.random.Next(GlobalSettings.BlockColorJitter), 0, 255);
                            b.color.G = (byte)MathHelper.Clamp(b.color.G - Globals.random.Next(GlobalSettings.BlockColorJitter), 0, 255);
                            b.color.B = (byte)MathHelper.Clamp(b.color.B - Globals.random.Next(GlobalSettings.BlockColorJitter / 2), 0, 255);
                            m.set(loc, b);
                        }
                    }
                }
            }
            
        }

        private void decide(IntLoc tileSpaceLoc)
        {
            Domain d = getDistributionAt(tileSpaceLoc);
            double[] weights = Enumerable.Range(0, tileSet.size()).Select(i => tileSet.getTile(i).weight()).ToArray();
            int tileIndex = d.getRandomTrueIndex(weights);
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
                        if (!decided(location) && (location.j == 0 || !assumeFlatWorld) && (location.i == 0 || !WorldGenParamaters.onlyOneVerticalLevel))
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
            foreach (IntLoc loc in getQueueFromBFS(WorldGenParamaters.decideTilesWithinWidth))
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

            /*float urgency = -d.sum() * 100;


            urgency += -IntLoc.EuclideanDistance(perspectiveTileLoc, tileLoc);
            */
            float dist = IntLoc.EuclideanDistance(perspectiveTileLoc, tileLoc);
            float distance_bell_curve = (float)Math.Exp(-(Math.Pow(dist / (WorldGenParamaters.decideTilesWithinWidth * 2), 2)));
            float normalized_domain_knowledge = 1 - d.sum() * 1.0f / d.k();

            float extra_close_factor = (dist < WorldGenParamaters.decideTilesWithinWidth / 2)?2:0;


            return normalized_domain_knowledge + distance_bell_curve - 1 + extra_close_factor + (float)Globals.random.NextDouble() * .1f;
            //return (1.0f - (float)Math.Pow(dist/width,1)) + offset - a;

        }

        private bool isWorldFlat()
        {
            return (assumeFlatWorld || WorldGenParamaters.onlyOneVerticalLevel);
        }

        private IntLoc? lowestEntropyUndecidedLocation()
        {
            IntLoc snapped_player_loc = new IntLoc(TileMap.playerPerspectiveLoc / tileSet.getTileWidth());
            if (isWorldFlat())
            {
                snapped_player_loc.j = 0;
            }
            if (!decided(snapped_player_loc) && !isWorldFlat())
            {
                return snapped_player_loc;
            }
            double greatestUrgency = -1;
            IntLoc? best = null;
            if (undecidedTilesAreInTheAlwaysRegion())
            {
                foreach (IntLoc loc in distributions.Keys)
                {
                    Domain d = getDistributionAt(loc);
                    double e = decisionUrgency(d, loc, snapped_player_loc);
                    bool isBetter = e > greatestUrgency;
                    if (!best.HasValue || isBetter)
                    {
                        greatestUrgency = e;
                        best = loc;
                        if (d.sum() == 1)
                        {
                            // there's only 1 possibility here, so place the tile
                            return best;
                        }
                    }
                }

            }
            if (greatestUrgency > 0)
            {
                return best;
            }
            return null;
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

        public bool canPlaceTile(ChunkManager m)
        {
            return lowestEntropyUndecidedLocation().HasValue;
            
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

            decide(new IntLoc());
            Thread worker = new Thread(() =>
            {
                // this is a hack to get these threads to quit when the application exits.
                while (Game1.gameRunning)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    if (!canPlaceTile(chunkManager))
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        while (stopwatch.ElapsedMilliseconds < 50)
                        {
                            placeATile(chunkManager);
                        }
                    }


                }
            });

            Thread worker2 = new Thread(() =>
            {
                // this is a hack to get these threads to quit when the application exits.
                while (Game1.gameRunning)
                {
                    bool didAnyRemeshing;
                    remeshAroundPlayer(out didAnyRemeshing);
                    if (!didAnyRemeshing)
                    {
                        Thread.Sleep(100);
                    }
                    chunkManager.unmeshOutsideRange(alwaysUnmeshOutsideRange);

                }
            });
            worker.IsBackground = false;
            worker.Start();

            worker2.IsBackground = false;
            worker2.Start();


            return chunkManager;
        }

        public void report()
        {
            Console.WriteLine(chunkManager.getReport());
        }


        void remeshAroundPlayer(out bool didAnyWork)
        {
            int meshRadius = alwaysMeshWithinRange / Chunk.chunkWidth;
            ConcurrentQueue<IntLoc> chunksNearPlayer = getQueueFromBFS(meshRadius * 2);
            //m.remeshAllParallelizeableStep(decided);
            IntLoc centerChunkPos = new IntLoc(TileMap.playerPerspectiveLoc / Chunk.chunkWidth );
            IntLoc toMeshChunkLoc;

            IntLoc BFSloc;
            int maxremeshes = 50;
            int remeshesSoFar = 0;
            while (chunksNearPlayer.TryDequeue(out BFSloc))
            {
                toMeshChunkLoc = (BFSloc + centerChunkPos - new IntLoc(meshRadius)) * Chunk.chunkWidth;
                if (chunkManager.chunkNeedsMesh(toMeshChunkLoc))
                {
                    chunkManager.remesh(chunkManager, toMeshChunkLoc);
                    remeshesSoFar ++;
                    if(remeshesSoFar > maxremeshes)
                    {
                        break;
                    }
                }

            }
            didAnyWork = remeshesSoFar > 0;
        }




        public void notifyOfPlayerLocation(Vector3 l)
        {
            TileMap.playerPerspectiveLoc = l;
        }

    }
}

