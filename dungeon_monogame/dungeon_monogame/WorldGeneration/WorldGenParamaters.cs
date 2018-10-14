using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    static class WorldGenParamaters
    {
        public static readonly int tileWidth = 21;
        public static readonly int sphereWidth = 9;
        public static readonly int decideTilesWithinWidth = 5;
        public static readonly int doNotGenerateOutsideRadius = 5;

        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_basic_test_11\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\15_dungeon\";
        public static string tileRelativePath = @"..\..\..\..\..\..\..\..\21_dungeon\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_3_hills\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_town5\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_woods11\";
        internal static bool onlyOneLevel = false;

        public static int MeshWithinBlockRange = 150;

        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_biased_fantasy_towers_11\";

    }
}
