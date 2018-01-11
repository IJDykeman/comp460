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

        public TileSet(string folderPath)
        {

            string[] files = Directory.GetFiles(MagicaVoxel.tileRoot, "*.vox");
            List<Tile> tilesList = new List<Tile>();
            for (int i=0;i<files.Length; i++){
                tilesList.Add(new Tile(MagicaVoxel.blocksFromVox(files[i])));
                tilesList.Add(new Tile(MagicaVoxel.blocksFromVox(files[i])).getRotated90());
                tilesList.Add(new Tile(MagicaVoxel.blocksFromVox(files[i])).getRotated90().getRotated90());
                tilesList.Add(new Tile(MagicaVoxel.blocksFromVox(files[i])).getRotated90().getRotated90().getRotated90());
            }
            tiles = tilesList.ToArray();
        }

        public Tile getTile(int i)
        {
            return tiles[i];
        }

        public int size()
        {
            return tiles.Length;
        }
    }
}
