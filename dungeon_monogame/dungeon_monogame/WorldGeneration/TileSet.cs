using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace dungeon_monogame.WorldGeneration
{
    public class InvalidTilesetException : Exception
    {
        public InvalidTilesetException(string message)
    : base(message)
        {
        }
    }

    internal class TileSet
    {
        Tile[] tiles;
        Sphere[] spheres; 
        Dictionary<IntLoc, DomainMatrix> deltaToTransitionMatrix;
        public bool worldAssumedFlat;


        public TileSet(string[] files, bool exampleBased, bool flatWorld)
        {
            worldAssumedFlat = flatWorld;
            ConcurrentBag<Tile> allTilesInFile = new ConcurrentBag<Tile>();


            //for (int i = 0; i < files.Length; i++)
            int tileWidth = 0;
            if (!exampleBased)
            {
                tileWidth = checkTilesetValidity(files);
            }
            Parallel.For(0, files.Length, i => {
            
                List<Tile> tilesFromThisFile;
                if (exampleBased)
                {
                    tilesFromThisFile = MagicaVoxel.TilesFromExampleModel(files[i], 3);
                }
                else
                {
                    tilesFromThisFile = MagicaVoxel.TilesFromPath(files[i], tileWidth);
                }

                foreach (Tile blockSubArray in tilesFromThisFile)
                {
                    if (!allTilesInFile.Contains((blockSubArray)))
                    {
                        allTilesInFile.Add((blockSubArray));
                    }
                    if (!files[i].Contains("norotation"))
                    {
                        Tile t;
                        t = ((blockSubArray).getRotated90());
                        if (!allTilesInFile.Contains(t))
                        {
                            allTilesInFile.Add(t);
                        }
                        t = t.getRotated90();
                        if (!allTilesInFile.Contains(t))
                        {
                            allTilesInFile.Add(t);
                        }
                        t = t.getRotated90();
                        if (!allTilesInFile.Contains(t))
                        {
                            allTilesInFile.Add(t);
                        }
                    }
                }
            });
            /*tiles = allTilesInFile.ToArray();
            buildTransitionMatrices();
            Console.WriteLine("transition matrices built");
            var sphereList = new List<Sphere>();
            var validTilesList = new List<Tile>();
            Parallel.For(0, tiles.Length, i =>
            {
                try
                {
                    var sphere = new Sphere(this, i, fileNameList[i]);
                    sphereList.Add(sphere);
                    validTilesList.Add(allTilesInFile[i]);

                }
                catch (InvalidTilesetException e)
                {
                    if (!exampleBased)
                    {
                        throw e;
                    }
                }
            });
            spheres = sphereList.ToArray();
            tiles = validTilesList.ToArray();*/
            tiles = allTilesInFile.ToArray();
            buildTransitionMatrices();
            Console.WriteLine("transition matrices built");
            spheres = new Sphere[tiles.Length];

            Stopwatch watch = new Stopwatch();
            watch.Start();
            try
            {
                Parallel.For(0, tiles.Length, i =>
                {
                    spheres[i] = new Sphere(this, i, flatWorld);
                });
            }
            catch (AggregateException ae)
            {
                var ignoredExceptions = new List<Exception>();
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    if (e is InvalidTilesetException)
                        throw e;
                    else
                        ignoredExceptions.Add(e);
                }
                if (ignoredExceptions.Count > 0)
                    throw new AggregateException(ignoredExceptions);
            }


        }

        private static int checkTilesetValidity(string[] files)
        {
            if( files.Where(f => f.ToLower().EndsWith(".vox")).Count() == 0)
            {
                throw new InvalidTilesetException("There are no .vox files in this folder.  \n\nTile sets are folders of .vox files all of the same size that fit together to form a world.  You can download some example tile sets on our website, generateworlds.com .");
            }
            string smallestSidedTileName = "";
            int smallestTileSide = -1;
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = files[i].Split('\\').Last();
                try
                {
                    List<int> dimensions = MagicaVoxel.getVoxelModelSize(files[i]);
                    int minSide = dimensions.Min();
                    if (smallestTileSide == -1)
                    {
                        smallestSidedTileName = fileName;
                        smallestTileSide = minSide;
                    }

                    List<int> invalidDimensions = dimensions.Where(x => x % smallestTileSide != 0).ToList();
                    if (invalidDimensions.Count > 0)
                    {
                        throw new InvalidTilesetException("The tile " + fileName + " has a side of length " + invalidDimensions[0].ToString() + ", but the tile " + smallestSidedTileName
                                                            + "has a side of length " + smallestTileSide.ToString() + ".  This won't work, becuase all your tiles need to be of the same size.  "
                                                            + "For example, if you have one 5 by 5 by 5 .vox model in your tile set, all the tiles must be 5 by 5 by 5.");
                    }
                }
                catch(Exception e)
                {
                    // this was probably just not a vox file
                }

            }
            return smallestTileSide; // the tile set tilewidth;
        }

        public int getTileWidth()
        {
            return tiles[0].tileWidth;
        }

        void buildTransitionMatrices()
        {
            deltaToTransitionMatrix = new Dictionary<IntLoc, DomainMatrix>();
            IntLoc[] directions = new IntLoc[]{
                new IntLoc(-1,0,0),
                new IntLoc(1,0,0),
                new IntLoc(0,1,0),
                new IntLoc(0,-1,0),
                new IntLoc(0,0,1),
                new IntLoc(0,0,-1),
            };
            foreach (IntLoc d in directions)
            {
                DomainMatrix m = Tile.buildTransitionMatrix(d.i, d.j, d.k, this);
                deltaToTransitionMatrix[d] = m;
            }
        }

        public DomainMatrix getTransitionMatrix(IntLoc delta)
        {
            return deltaToTransitionMatrix[delta];
        }

        public Tile getTile(int i)
        {
            return tiles[i];
        }

        public Sphere getSphere(int i)
        {
            return spheres[i];
        }

        public int size()
        {
            return tiles.Length;
        }
    }
}
