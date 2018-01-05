﻿using Microsoft.Xna.Framework;
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

        public static Vector3 randomVectorOnUnitSphere()
        {
            // based on http://corysimon.github.io/articles/uniformdistn-on-sphere/
            double theta = 2 * Math.PI * random.NextDouble();
            double phi = Math.PI * random.NextDouble();
            double x = Math.Sin(phi) * Math.Cos(theta);
            double y = Math.Sin(phi) * Math.Sin(theta);
            double z = Math.Cos(phi);
            return new Vector3((float)x, (float)y, (float)z);
        }

        public static float standardGaussianSample() {
            // from https://stackoverflow.com/questions/218060/random-gaussian-variables
            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = randStdNormal; //random normal(mean,stdDev^2)
            return (float)randNormal;
        }
    }


}



