using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class TileSet
    {
        Tile[] tiles;
        Sphere[] spheres;
        Dictionary<IntLoc, DomainMatrix> deltaToTransitionMatrix;


        public TileSet(string folderPath)
        {

            //string[] files = Directory.GetFiles(MagicaVoxel.tileRoot, "*.vox");
            string[] files = Directory.GetFiles(MagicaVoxel.tileRoot);
            List<String> fileNameList = new List<string>();
            List<Tile> tilesList = new List<Tile>();
            
            for (int i = 0; i < files.Length; i++)
            {
                List<Block[,,]> tiles = MagicaVoxel.TilesFromPath(files[i]);
                foreach (Block[,,] blockSubArray in tiles)
                {
                    if (!tilesList.Contains(new Tile(blockSubArray)))
                    {
                        tilesList.Add(new Tile(blockSubArray));
                        fileNameList.Add(files[i]);
                    }
                    if (!files[i].Contains("norotation"))
                    {
                        Tile t;
                        t = (new Tile(blockSubArray).getRotated90());
                        if (!tilesList.Contains(t))
                        {
                            tilesList.Add(t);
                            fileNameList.Add(files[i]);
                        }
                        t = (new Tile(blockSubArray).getRotated90().getRotated90());
                        if (!tilesList.Contains(t))
                        {
                            tilesList.Add(t);
                            fileNameList.Add(files[i]);
                        }
                        t = (new Tile(blockSubArray).getRotated90().getRotated90().getRotated90());
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
            //for (int i = 0; i < tiles.Length; i++)
            //{
            Parallel.For(0, tiles.Length, i=> {
                spheres[i] = new Sphere(this, i, fileNameList[i]);
            });
            //}

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
