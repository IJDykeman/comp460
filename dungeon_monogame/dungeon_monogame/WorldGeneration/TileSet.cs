using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    public class InvalidTilesetException : Exception
    {
        public InvalidTilesetException(string message)
    : base(message)
        {
        }
    }

        class TileSet
    {
        Tile[] tiles;
        Sphere[] spheres; 
        Dictionary<IntLoc, DomainMatrix> deltaToTransitionMatrix;
        

        public TileSet(string folderPath, bool exampleBased)
        {
            string[] files = Directory.GetFiles(folderPath);
            List<String> fileNameList = new List<string>();
            List<Tile> tilesList = new List<Tile>();


            for (int i = 0; i < files.Length; i++)
            {
                List<Tile> tilesFromThisFile;
                if (exampleBased)
                {
                    tilesFromThisFile = MagicaVoxel.TilesFromExampleModel(files[i], 3);
                }
                else
                {
                    int tileWidth = checkTilesetValidity(files);
                    tilesFromThisFile = MagicaVoxel.TilesFromPath(files[i], tileWidth);
                }

                foreach (Tile blockSubArray in tilesFromThisFile)
                {
                    if (!tilesList.Contains((blockSubArray)))
                    {
                        tilesList.Add((blockSubArray));
                        fileNameList.Add(files[i]);
                    }
                    if (!files[i].Contains("norotation"))
                    {
                        Tile t;
                        t = ((blockSubArray).getRotated90());
                        if (!tilesList.Contains(t))
                        {
                            tilesList.Add(t);
                            fileNameList.Add(files[i]);
                        }
                        t = ((blockSubArray).getRotated90().getRotated90());
                        if (!tilesList.Contains(t))
                        {
                            tilesList.Add(t);
                            fileNameList.Add(files[i]);
                        }
                        t = ((blockSubArray).getRotated90().getRotated90().getRotated90());
                        if (!tilesList.Contains(t))
                        {
                            tilesList.Add(t);
                            fileNameList.Add(files[i]);
                        }
                    }
                }
            }
            tiles = tilesList.ToArray();
            buildTransitionMatrices();
            Console.WriteLine("transition matrices built");
            spheres = new Sphere[tiles.Length];
            Parallel.For(0, tiles.Length, i =>
            {
                spheres[i] = new Sphere(this, i, fileNameList[i]);
            });
        }

        private static int checkTilesetValidity(string[] files)
        {
            if( files.Where(f => f.ToLower().EndsWith(".vox")).Count() == 0)
            {
                throw new InvalidTilesetException("There are no .vox files in this folder.  \n\nTile sets are folders of .vox files all of the same size that fit together to form a world.  You can download some example tile sets on our website.");
            }
            string smallestSidedTileName = "";
            int smallestTileSide = -1;
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = files[i].Split('\\').Last();
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
