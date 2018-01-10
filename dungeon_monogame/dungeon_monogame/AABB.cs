using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dungeon_monogame
{
    class AABB
    {
        public float height, xWidth, zWidth;



        public AABB(float _height, float _xWidth, float _zWidth)
        {
            height = _height;
            xWidth = _xWidth;
            zWidth = _zWidth;
        }

        public float axisMin(Globals.axes axis, Vector3 scale){
            switch (axis){
                case Globals.axes.x:
                    return -xWidth / 2f * Globals.along(scale, axis);
                case Globals.axes.y:
                    return -height / 2f * Globals.along(scale, axis);
                default:
                    return -zWidth / 2f * Globals.along(scale, axis);
            }
        }

        public float axisMax(Globals.axes axis, Vector3 scale)
        {
            switch (axis)
            {
                case Globals.axes.x:
                    return xWidth / 2f * Globals.along(scale, axis);
                case Globals.axes.y:
                    return height / 2f * Globals.along(scale, axis);
                default:
                    return zWidth / 2f * Globals.along(scale, axis);
            }
        }


    }
}
