using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class WorldTileDistribution : ProbabilityDistribution
    {
        public WorldTileDistribution(ProbabilityDistribution p)
        {
            distribution = p.getDistributionCopy();
            normalize();
        }

        public ProbabilityDistribution asProbabilityDistribution()
        {
            ProbabilityDistribution result = ProbabilityDistribution.evenOdds(k());
            result.setAll(distribution);
            return result;
        }

        public override void normalize()
        {

            double s = distribution.Sum();
            if (s != 0)
            {
                for (int i = 0; i < distribution.Length; i++)
                {
                    distribution[i] /= s;
                }
            }
            else
            { 
                // i think this is critical.  It means ignoring all placements before and including an eronous one.
                setEvenOdds();
            }
            updateEntropy();
        }
        
    }
}
