using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    static class Globals
    {
        public static Random random = new Random();

        public static int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public enum axes {x,y,z}

        public static readonly axes[] allAxes = new axes[]{axes.x, axes.y, axes.z};
        public static readonly float G = 9.8f;

        public static Vector3 unit(Globals.axes axis)
        {
            switch (axis)
            {
                case Globals.axes.x:
                    return Vector3.UnitX;
                case Globals.axes.y:
                    return Vector3.UnitY;
                default:
                    return Vector3.UnitZ;
            }
        }

        public static float along(Vector3 v, Globals.axes axis)
        {
            switch (axis)
            {
                case Globals.axes.x:
                    return v.X;
                case Globals.axes.y:
                    return v.Y;
                default:
                    return v.Z;
            }
        }

        public static Tuple<axes, axes> OtherAxes(Globals.axes axis)
        {
            switch (axis)
            {
                case Globals.axes.x:
                    return new Tuple<axes, axes>(axes.y, axes.z);
                case Globals.axes.y:
                    return new Tuple<axes, axes>(axes.x, axes.z);
                default:
                    return new Tuple<axes, axes>(axes.x, axes.y);
            }
        }
    }


}



