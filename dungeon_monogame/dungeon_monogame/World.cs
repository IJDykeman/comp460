using dungeon_monogame.WorldGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class World : GameObject
    {
        public TileMap map;
        List<IntLoc> tileSpaceLocsNeedingPostprocess;
        AmbientLight ambientLight1, ambientLight2;
        float totalAmbientPower;

        public World(TileSet _tiles)
        {
            resetTileMap(_tiles);
            tileSpaceLocsNeedingPostprocess = new List<IntLoc>();
            ambientLight1 = new AmbientLight(0.2f, Color.White, new Vector3(1,2,3));
            addChild(ambientLight1);
            ambientLight2 = new AmbientLight(0.1f, Color.White, new Vector3(-6, -1, -7));
            addChild(ambientLight2);
            addChild(map);
            setTotalAmbientPower(.3f);
        }

        public void resetTileMap(TileSet tiles)
        {
            map = new TileMap(tiles, Vector3.Zero, Vector3.One);

        }

        internal TileMap getMap()
        {
            return map;
        }

        public void changeTotalAmbientPower(float delta)
        {
            float nPower = delta + totalAmbientPower;
            nPower = Math.Min(nPower, 1);
            nPower = Math.Max(nPower, 0);
            setTotalAmbientPower(nPower);
        }

        public void setTotalAmbientPower(float power)
        {
            totalAmbientPower = power;
            ambientLight1.setIntensity(totalAmbientPower / 3 * 2);
            ambientLight2.setIntensity(totalAmbientPower / 3);
        }

        protected void postprocess(IntLoc tileSpacePos)
        {
            tileSpaceLocsNeedingPostprocess.Add(tileSpacePos);
        }

        public void update()
        {
            // place actors in tiles that need postprocessing   
        }


    }
}
