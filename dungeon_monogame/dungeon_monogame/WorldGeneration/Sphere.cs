using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dungeon_monogame.WorldGeneration
{
    class Sphere
    {
        Domain[, ,] sphere;

        public Domain get(int i, int j, int k)
        {
            return sphere[i, j, k];
        }

        public Sphere(TileSet set, int tileIndex, string name)
        {

            sphere = new Domain[WorldGenParamaters.sphereWidth, WorldGenParamaters.sphereWidth, WorldGenParamaters.sphereWidth];

            for (int i = 0; i < WorldGenParamaters.sphereWidth; i++)
            {
                for (int j = 0; j < WorldGenParamaters.sphereWidth; j++)
                {
                    for (int k = 0; k < WorldGenParamaters.sphereWidth; k++)
                    {
                        sphere[i, j, k] = new Domain(set.size());
                        sphere[i, j, k].setAllTrue();
                    }
                }
            }
            sphere[WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2] = Domain.oneHot(set.size(), tileIndex);
            List<IntLoc> bfsOrder = Globals.gridBFS(WorldGenParamaters.sphereWidth).AsEnumerable().ToList();
            bfsOrder.RemoveAt(0); // don't consider center of sphere


            while (true)
            {
                bool changedOne = false;
                foreach (IntLoc queryLoc in bfsOrder)
                {
                    foreach (IntLoc neighbor in Globals.neighbors(queryLoc, WorldGenParamaters.sphereWidth))
                    {
                        Domain probFrom = sphere[neighbor.i, neighbor.j, neighbor.k];
                        IntLoc delta = neighbor - queryLoc;
                        DomainMatrix trans = set.getTransitionMatrix(delta);

                        Domain old = sphere[queryLoc.i, queryLoc.j, queryLoc.k];

                        bool[] probTo = DomainMatrix.dot(trans, probFrom.toBoolArray());
                        Domain mask = new Domain(probTo);
                        Domain result = old * mask;
                        if (result.sum() == 0)
                        {
                            throw new Exception("Sphere contains zero domain: " + name);
                        }

                        sphere[queryLoc.i, queryLoc.j, queryLoc.k] = result;

                        changedOne = !(old.Equals(result)) || changedOne;

                    }
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
