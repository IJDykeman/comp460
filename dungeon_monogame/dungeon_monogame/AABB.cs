﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dungeon_monogame
{
    class AABB
    {
        Vector3 centerPosition;
        public float height, xWidth, zWidth;

        public Vector3 getCenter()
        {
            return centerPosition;
        }

        public AABB(Vector3 _centerPosition, float _height, float _xWidth, float _zWidth)
        {
            centerPosition = _centerPosition;
            height = _height;
            xWidth = _xWidth;
            zWidth = _zWidth;
        }

        Vector3 center()
        {
            return centerPosition;
        }

        public float axisMin(Globals.axes axis){
            switch (axis){
                case Globals.axes.x:
                    return -xWidth / 2f;
                case Globals.axes.y:
                    return -height / 2f;
                default:
                    return -zWidth / 2f;
            }
        }

        public float axisMax(Globals.axes axis)
        {
            switch (axis)
            {
                case Globals.axes.x:
                    return xWidth / 2f;
                case Globals.axes.y:
                    return height / 2f;
                default:
                    return zWidth / 2f;
            }
        }




        internal void setCenter(Vector3 prospectiveLocation)
        {
            centerPosition = prospectiveLocation;
        }
    }
}
