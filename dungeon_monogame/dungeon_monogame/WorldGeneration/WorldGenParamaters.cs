using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    static class WorldGenParamaters
    {
        public static readonly int tileWidth = 15;
        public static readonly int sphereWidth = 9;
        public static readonly int decideTilesWithinWidth = 25;
        public static readonly int doNotGenerateOutsideRadius = 25;

        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_basic_test_11\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\15_dungeon\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\21_dungeon\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_spacious_15\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_spacious_21_dungeon\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_3_hills\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_town5\";
        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_woods11\";
        public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_vertical_town_15\";
        internal static bool onlyOneHorizontalLevel = false;
        internal static bool onlyOneVerticalLevel = true;

        public static int MeshWithinBlockRange = 150;

        //public static string tileRelativePath = @"..\..\..\..\..\..\..\..\tiles_biased_fantasy_towers_11\";

    }
}
