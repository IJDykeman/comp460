using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dungeon_monogame.WorldGeneration
{
    class Sphere
    {
        Domain[, ,] sphere;
        string name;

        public Domain get(int i, int j, int k)
        {
            return sphere[i, j, k];
        }

        public Sphere(TileSet set, int tileIndex, bool flatWorld)
        {
            name = set.getTile(tileIndex).name;
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

            List<IntLoc> worklist = bfsOrder;
            while (true)
            {
                HashSet<IntLoc> newWorkList = new HashSet<IntLoc>();
                bool changedOne = false;
                foreach (IntLoc queryLoc in worklist)
                {
                    if (flatWorld && queryLoc.j != WorldGenParamaters.sphereWidth / 2)
                    {
                        continue;
                    }
                    if(queryLoc.Equals(new IntLoc(WorldGenParamaters.sphereWidth / 2)))
                    {
                        continue;
                    }
                    foreach (IntLoc neighbor in Globals.neighbors(queryLoc, WorldGenParamaters.sphereWidth))
                    {
                        if (flatWorld && neighbor.j != WorldGenParamaters.sphereWidth / 2)
                        {
                            continue;
                        }
                        Domain probFrom = sphere[neighbor.i, neighbor.j, neighbor.k];
                        IntLoc delta = neighbor - queryLoc;
                        DomainMatrix trans = set.getTransitionMatrix(delta);

                        Domain old = sphere[queryLoc.i, queryLoc.j, queryLoc.k];

                        bool[] probTo = DomainMatrix.dot(trans, probFrom.toBoolArray());
                        Domain mask = new Domain(probTo);
                        Domain result = old * mask;

                        if (result.sum() == 0)
                        {
                            throw new InvalidTilesetException("The tile " + name + " can't fit together with one or more of the other tiles in this tile set.  "
                                                              +   "To fix this problem, carefully review your tiles, and look for places where blocks may not line up along the tiles' sides.  "
                                                              +   "It may be helpful to remove some tiles from the tile set temporarily to narrow the problem down.");
                        }

                        sphere[queryLoc.i, queryLoc.j, queryLoc.k] = result;
                        bool changedThisLocation = old.Equals(result);
                        changedOne = !(changedThisLocation) || changedOne;
                        if (changedThisLocation)
                        {
                            newWorkList.Add(queryLoc);
                            newWorkList.Add(neighbor);
                        }

                    }
                }
                worklist = newWorkList.ToList();
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
