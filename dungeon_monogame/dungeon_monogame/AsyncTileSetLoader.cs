using dungeon_monogame.WorldGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class TileSetLoader : Actor
    {
        TileSet tileSet;
        Thread thread;
        public TileSetLoader(bool isExampleBased, string path=null)
        {
            bool userWantsFlatWorld = false;

            string[] files = getTilesetPathFromUser(isExampleBased, path:path);
            if (isExampleBased)
            {
                userWantsFlatWorld = FileManagement.AskWhetherWorldIsFlat();
            }
            thread = new Thread(() =>
            {
                generateTileSet(files, isExampleBased, userWantsFlatWorld);
            });
            thread.Start();
    
        }

        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();
            if (!thread.IsAlive)
            {
                // thread is done
                if(tileSet == null)
                {
                    return (new Action[] { new DissapearAction(this) }).ToList();
                }
                else
                {
                    return (new Action[] { new RequestWorldResetWithTileset(tileSet), new DissapearAction(this) }).ToList();
                }
            }
            return result;
        }


        private void generateTileSet(string[] files, bool isExampleBased, bool userWantsFlatWorld)
        {
            try
            {
                WorldGeneration.TileSet tiles = new WorldGeneration.TileSet(files, isExampleBased, userWantsFlatWorld);
                tileSet = tiles;
            }
            catch (InvalidTilesetException e)
            {

                if (!userWantsFlatWorld)
                {
                    WorldGeneration.TileSet tiles = new WorldGeneration.TileSet(files, isExampleBased, true);
                    // this tileset generation succeeds with a flat world where it failed with a deep one
                    // This means we can report to the user that they should be setting their world to flat. 

                    string errorForFlatWorld = "This tile set isn't valid for a world that is more than one tile high.  " +
                                                      "Perhaps you tried to create a landscape tile set without tiles to go below the ground or above the surface tiles.  " +
                                                      "If you intend to have a world that is only one tile in height, please select \"one tile high world\" when you are selecting the tile set folder to load.";
                    FileManagement.ShowDialog(errorForFlatWorld, "This isn't a valid tile set.");
                }
                else
                {
                    FileManagement.ShowDialog(e.Message, "This isn't a valid tile set.");
                }
            }
        }

        private static string[] getTilesetPathFromUser(bool isExampleBased, string path=null)
        {
            if (isExampleBased)
            {
                if (path == null)
                {
                    path = FileManagement.OpenFileDialog();
                }
                return new string[] { path };
            }
            else
            {
                if (path == null)
                {
                    path = FileManagement.getDirectoryFromDialogue();
                }
                return getVoxFiles(path);
            }
        }

        public  static string[] getVoxFiles(string root)
        {
            List<string> allFiles = new List<string>(Directory.GetFiles(root));
            return allFiles.Where(x => x.ToLower().EndsWith(".vox")).ToArray();
        }
    }
}
