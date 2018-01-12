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
        public static readonly float G = 18f;

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

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            //https://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new Color(v, t, p);
            else if (hi == 1)
                return new Color(q, v, p);
            else if (hi == 2)
                return new Color(p, v, t);
            else if (hi == 3)
                return new Color(p, q, v);
            else if (hi == 4)
                return new Color(t, p, v);
            else
                return new Color(v, p, q);
        }

        public static List<IntLoc> neighbors(IntLoc center, int width)
        {
            IntLoc[] neighbors = new IntLoc[] { new IntLoc(-1, 0, 0), new IntLoc(1, 0, 0), new IntLoc(0, -1, 0), new IntLoc(0, 1, 0), new IntLoc(0, 0, -1), new IntLoc(0, 0, 1) };
            List<IntLoc> result = new List<IntLoc>();
            for (int iter = 0; iter < neighbors.Length; iter++)
            {
                neighbors[iter] += center;
                if (neighbors[iter].i >= 0 && neighbors[iter].i < width
                    && neighbors[iter].j >= 0 && neighbors[iter].j < width
                    && neighbors[iter].k >= 0 && neighbors[iter].k < width)
                {
                    result.Add(neighbors[iter]);
                }
            }
            return result;
        }


        public static List<IntLoc> gridBFS(int starti, int startj, int startl, int width)
        {
            List<IntLoc> result = new List<IntLoc>();
            Queue<IntLoc> queue = new Queue<IntLoc>();
            queue.Enqueue(new IntLoc(starti, startj, startl));
            while (queue.Count > 0)
            {
                IntLoc loc = queue.Dequeue();
                result.Add(loc);
                List<IntLoc> nextSteps = neighbors(loc, width);
                foreach (IntLoc next in nextSteps)
                {
                    if (!queue.Contains(next) && !result.Contains(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }
            return result;
        }

    }

    


}



