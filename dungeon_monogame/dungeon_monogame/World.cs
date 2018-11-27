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
            resetTileMap(_tiles, _tiles.worldAssumedFlat);
            tileSpaceLocsNeedingPostprocess = new List<IntLoc>();
            ambientLight1 = new AmbientLight(0.2f, Color.White, new Vector3(1,2,3));
            addChild(ambientLight1);
            ambientLight2 = new AmbientLight(0.1f, Color.White, new Vector3(-6, -1, -7));
            addChild(ambientLight2);
            
            setTotalAmbientPower(GlobalSettings.defaultAmbientLight);
        }

        internal void exportModel()
        {
            map.getChunkManager().createMesh();
        }

        public void resetTileMap(TileSet tiles, bool isFlatWorld)
        {
            recursiveRemove(map);
            map = new TileMap(tiles, Vector3.Zero, Vector3.One, isFlatWorld);
            addChild(map);

        }

        internal TileMap getMap()
        {
            return map;
        }

        public void changeTotalAmbientPower(float delta)
        { 
            totalAmbientPower += delta;
            totalAmbientPower = Math.Min(totalAmbientPower, GlobalSettings.maxAmbientLight);
            totalAmbientPower = Math.Max(totalAmbientPower, GlobalSettings.minAmbientLight);
            setTotalAmbientPower(totalAmbientPower);
        }

        void setTotalAmbientPower(float _totalAmbientPower)
        {
            totalAmbientPower = _totalAmbientPower;
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
