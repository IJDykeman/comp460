using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dungeon_monogame.WorldGeneration
{
    class Sphere
    {
        ProbabilityDistribution[, ,] sphere;

        public ProbabilityDistribution get(int i, int j, int k)
        {
            return sphere[i, j, k];
        }

        public Sphere(TileSet set, int tileIndex)
        {
            sphere = new ProbabilityDistribution[WorldGenParamaters.sphereWidth, WorldGenParamaters.sphereWidth, WorldGenParamaters.sphereWidth];
            for (int i = 0; i < WorldGenParamaters.sphereWidth; i++)
            {
                for (int j = 0; j < WorldGenParamaters.sphereWidth; j++)
                {
                    for (int k = 0; k < WorldGenParamaters.sphereWidth; k++)
                    {
                        sphere[i, j, k] = new ProbabilityDistribution(set.size());
                    }
                }
            }

            sphere[WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2] = ProbabilityDistribution.oneHot(set.size(), tileIndex);
            List<IntLoc> bfsOrder = Globals.gridBFS(WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth);
            foreach (IntLoc queryLoc in bfsOrder)
            {
                foreach (IntLoc neighbor in Globals.neighbors(queryLoc, WorldGenParamaters.sphereWidth))
                {
                    ProbabilityDistribution probFrom = sphere[neighbor.i, neighbor.j, neighbor.k];
                    IntLoc delta = neighbor - queryLoc;
                    MyMatrix trans = set.getTransitionMatrix(delta);
                    if (trans.toDoubleArray()[1,0] == 1 && queryLoc.j==2)
                    {

                    }
                    Double[] probTo = MyMatrix.dot(trans, probFrom.toDoubleArray());
                    //sphere[queryLoc.i, queryLoc.j, queryLoc.k] += new ProbabilityDistribution(probTo);

                    ProbabilityDistribution old = sphere[queryLoc.i, queryLoc.j, queryLoc.k];
                    ProbabilityDistribution toAdd = new ProbabilityDistribution(probTo);
                    ProbabilityDistribution result = old + toAdd;
                    sphere[queryLoc.i, queryLoc.j, queryLoc.k] = result;
                }
                sphere[queryLoc.i, queryLoc.j, queryLoc.k].normalize();

            }


        }
    }
}
