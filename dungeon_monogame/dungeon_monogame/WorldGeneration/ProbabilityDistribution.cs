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
        }

        public double get(int i){
            return distribution[i];
        }

        public void set(int i, double p)
        {
            distribution[i] = p;
            normalize();
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

        public static ProbabilityDistribution operator +(ProbabilityDistribution a, ProbabilityDistribution b)
        {
            ProbabilityDistribution result = new ProbabilityDistribution(a.k());
            for (int i = 0; i < result.k(); i++)
            {
                result.set(i, a.get(i) + b.get(i));
            }
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
