using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class Domain
    {
        protected Domain() { }

        //protected bool[] distribution;

        protected BitArray distribution;
        //static Int32[] ints;



        public Domain(int k)
        {
            setup(k);
        }


        public Domain(bool[] d)
        {
            setup(d.Length);
            setAll(d);
        }

        private void setup(int k)
        {
            distribution = new BitArray(k);
            for (int i = 0; i < k; i++)
            {
                distribution[i] = false;
            }
            
        }

        public bool get(int i){
            return distribution[i];
        }

        static int sum(BitArray bitarray)
        {
            int[] ints = new Int32[(bitarray.Count >> 5) + 1];
            bitarray.CopyTo(ints, 0);

            Int32 count = 0;

            // fix for not truncated bits in last integer that may have been set to true with SetAll()
            ints[ints.Length - 1] &= ~(-1 << (bitarray.Count % 32));

            for (Int32 i = 0; i < ints.Length; i++)
            {

                Int32 c = ints[i];

                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    c = c - ((c >> 1) & 0x55555555);
                    c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                    c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                }

                count += c;

            }

            return count;
        }

        public int sum()
        {
            return Domain.sum(distribution);
        }

        public int getRandomTrueIndex(double[] weights)
        {
            double[] masked_weights = Enumerable.Range(0, weights.Length).Select(i => weights[i] * (distribution[i] ? 1 : 0)).ToArray();
            double choice = Globals.random.NextDouble() * masked_weights.Sum();
            for (int i=0; i < distribution.Length; i++)
            {
                if (distribution[i])
                {
                    choice -= masked_weights[i];
                    if (choice <= 0)
                    {
                        return i;
                    }
                }
            }

            return 0;

        }

        public void set(int i, bool p)
        {
            distribution[i] = p;
        }

        public bool[] toBoolArray()
        {

            bool[] result = new bool[distribution.Length];
            for (int i = 0; i < distribution.Length; i++)
            {
                result[i] = distribution[i];
            }
            return result;
        }

        public void setAll(bool[] p)
        {
            distribution = new BitArray(p);
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


        public override string ToString()
        {
            return distribution.ToString();
        }

        public override bool Equals(object obj)
        {
            var p = (Domain)obj;
            return Domain.sum(distribution.Xor(p.distribution)) == 0;
        }

    }
}
