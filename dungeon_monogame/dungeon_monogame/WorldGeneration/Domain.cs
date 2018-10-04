using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class Domain
    {
        protected Domain() { }

        protected bool[] distribution;

        public bool[] getDistributionCopy()
        {
            return distribution.ToArray();
        }

        public Domain(int k)
        {
            distribution = new bool[k];
            for (int i=0; i < k; i++)
            {
                distribution[i] = false;
            }
        }

        public Domain(bool[] d)
        {
            distribution = d;
        }

        public bool get(int i){
            return distribution[i];
        }

        public int sum()
        {
            int result = 0;
            for (int i = 0; i < distribution.Count(); i++)
            {
                if (distribution[i])
                {
                    result++;
                }
            }
            return result;

        }
        public int getRandomTrueIndex()
        {
            List<int> trueIndices = new List<int>();
            for (int i=0; i < distribution.Count(); i++)
            {
                if (distribution[i])
                {
                    trueIndices.Add(i);
                }
            }
            return trueIndices[Globals.random.Next(trueIndices.Count())];

        }

        public void set(int i, bool p)
        {
            distribution[i] = p;
        }

        public bool[] toBoolArray()
        {
            return (bool[])distribution.Clone();
        }

        public void setAll(bool[] p)
        {
            distribution = p;
        }

        public int k()
        {
            return distribution.Length;
        }

        public void setAllTrue()
        {
            for (int i = 0; i < distribution.Length; i++)
            {
                distribution[i] = true;
            }
        }

        public static Domain oneHot(int length, int hot)
        {
            Domain result = new Domain(length);
            result.set(hot, true);
            return result;
        }

        public static Domain allTrue(int length)
        {
            Domain result = new Domain(length);
            result.setAllTrue();
            return result;
        }



        public static Domain operator *(Domain a, Domain b)
        {
            Domain result = new Domain(a.k());
            for (int i = 0; i < result.distribution.Length; i++)
            {
                result.distribution[i] = a.distribution[i] && b.distribution[i];
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            var p = (Domain)obj;
            return Enumerable.SequenceEqual(p.distribution, distribution);
        }

        public override string ToString()
        {
            return distribution.ToString();
        }

    }
}
