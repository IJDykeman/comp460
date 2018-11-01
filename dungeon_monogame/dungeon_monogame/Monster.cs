using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class Monster : Creature
    {

        Vector3 targetLocation = new Vector3();



        public Monster()
        {
            lifeStopwatch = new Stopwatch();
            lifeStopwatch.Start();
        }



        public void setTargetLocation(Vector3 loc)
        {
            targetLocation = loc;
        }

        public Vector3 getTargetLocation()
        {
            return targetLocation;
        }


    }
}
