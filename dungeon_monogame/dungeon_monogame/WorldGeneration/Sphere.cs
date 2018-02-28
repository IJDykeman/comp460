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
                        sphere[i, j, k].setEvenOdds();
                    }
                }
            }
            sphere[WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2] = ProbabilityDistribution.oneHot(set.size(), tileIndex);
            List<IntLoc> bfsOrder = Globals.gridBFS(WorldGenParamaters.sphereWidth).AsEnumerable().ToList();
            bfsOrder.RemoveAt(0); // don't consider center of sphere


            while (true)
            {
                bool changedOne = false;
                foreach (IntLoc queryLoc in bfsOrder)
                {
                    foreach (IntLoc neighbor in Globals.neighbors(queryLoc, WorldGenParamaters.sphereWidth))
                    {
                        ProbabilityDistribution probFrom = sphere[neighbor.i, neighbor.j, neighbor.k];
                        IntLoc delta = neighbor - queryLoc;
                        MyMatrix trans = set.getTransitionMatrix(delta);

                        ProbabilityDistribution old = sphere[queryLoc.i, queryLoc.j, queryLoc.k];

                        Double[] probTo = MyMatrix.dot(trans, probFrom.toDoubleArray()).Select(x => { if (x > 0) { return 1d; } return 0d; }).ToArray();
                        ProbabilityDistribution mask = new ProbabilityDistribution(probTo);
                        ProbabilityDistribution result = old * mask;

                        sphere[queryLoc.i, queryLoc.j, queryLoc.k] = result;

                        changedOne = !(Enumerable.SequenceEqual(
                            old.toDoubleArray().Select(x => { if (x > 0) { return 1d; } return 0d; }),
                            result.toDoubleArray().Select(x => { if (x > 0) { return 1d; } return 0d; })
                        )) || changedOne;

                    }
                    sphere[queryLoc.i, queryLoc.j, queryLoc.k].normalize();
                }
                if (!changedOne)
                {
                    break;
                }
                else
                {
                    //Console.WriteLine(sphere[2, 2, 2].ToString());
                }
            }
            Console.WriteLine("did a sphere");


        }
    }
}
