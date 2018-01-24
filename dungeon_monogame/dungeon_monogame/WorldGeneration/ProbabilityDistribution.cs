using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class ProbabilityDistribution
    {
        double[] distribution;

        public ProbabilityDistribution(int k)
        {
            distribution = new double[k];
        }

        public ProbabilityDistribution(double[] d)
        {
            distribution = d;
            normalize();
        }

        public double get(int i){
            return distribution[i];
        }

        public void set(int i, double p)
        {
            distribution[i] = p;
            //normalize();
        }

        public double entropy()
        {
            return distribution.Select(num => num * Math.Log(num + .0001)).Sum();
        }

        public int argmax()
        {
            return distribution.ToList().IndexOf(distribution.Max());
        }

        public int sample()
        {
            normalize();

            if (distribution.Sum() == 0)
            {
                ProbabilityDistribution p = new ProbabilityDistribution(k());
                p.setEvenOdds();
                return p.sample();
            }

            double power = 27.0 / (Math.Pow(WorldGenParamaters.tileWidth, 3));
            power = 1.0 / 2;
            var newVals = distribution.Select(x => Math.Pow(x, power));
            double[] tempAdjustedDistribution = newVals.ToArray();
            double sum = newVals.Sum();

            double r = Globals.random.NextDouble() * sum;
            int i = 0;
            while (r > 0)
            {
                r -= tempAdjustedDistribution[i];
                if (r <= 0)
                {
                    return i;
                }
                i++;
            }
            return k() - 1;
        }

        internal bool isZero()
        {
            return distribution.Sum() == 0;
        }

        public int k()
        {
            return distribution.Length;
        }

        public void setEvenOdds()
        {
            for (int i = 0; i < distribution.Length; i++)
            {
                distribution[i] = 1.0 / k();
            }
        }

        public void normalize()
        {
            double s = distribution.Sum();
            if (s != 0)
            {
                for (int i = 0; i < distribution.Length; i++)
                {
                    distribution[i] /= s;
                }
            }
        }

        public static ProbabilityDistribution oneHot(int length, int hot)
        {
            ProbabilityDistribution result = new ProbabilityDistribution(length);
            result.set(hot, 1.0);
            return result;
        }

        public static ProbabilityDistribution evenOdds(int length)
        {
            ProbabilityDistribution result = new ProbabilityDistribution(length);
            result.setEvenOdds();
            return result;
        }

        public static ProbabilityDistribution operator +(ProbabilityDistribution a, ProbabilityDistribution b)
        {
            ProbabilityDistribution result = new ProbabilityDistribution(a.k());
            for (int i = 0; i < result.k(); i++)
            {
                result.set(i, a.get(i) + b.get(i));
            }
            result.normalize();
            return result;
        }


        public static ProbabilityDistribution operator *(ProbabilityDistribution a, ProbabilityDistribution b)
        {
            ProbabilityDistribution result = new ProbabilityDistribution(a.k());
            for (int i = 0; i < result.distribution.Length; i++)
            {
                result.distribution[i] = a.distribution[i] * b.distribution[i];
            }
            //result.normalize();
            return result;
        }


        public double[] toDoubleArray()
        {
            double[] result = new double[k()];
            for (int i = 0; i < k(); i++)
            {
                result[i] = distribution[i];
            }
            return result;
        }
    }
}
