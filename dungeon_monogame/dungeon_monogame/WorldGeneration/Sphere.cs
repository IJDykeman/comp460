using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dungeon_monogame.WorldGeneration
{
    class Sphere
    {

        List<IntLoc> neighbors(IntLoc center, int width)
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


        List<IntLoc> gridBFS(int starti, int startj, int startl, int width){
            List<IntLoc> result = new List<IntLoc>();
            Queue<IntLoc> queue = new Queue<IntLoc>();
            queue.Enqueue(new IntLoc(starti, startj, startl));
            while (queue.Count > 0){
                IntLoc loc = queue.Dequeue();
                result.Add(loc);
                List<IntLoc> nextSteps = neighbors(loc, width);
                foreach(IntLoc next in nextSteps){
                    if(!queue.Contains(next)){
                        queue.Enqueue(next);
                    }
                }
            }
            return result;
        }

        ProbabilityDistribution[, ,] sphere;

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
            List<IntLoc> bfsOrder = gridBFS(WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth / 2, WorldGenParamaters.sphereWidth);
            foreach (IntLoc queryLoc in bfsOrder)
            {
                foreach (IntLoc neighbor in neighbors(queryLoc, WorldGenParamaters.sphereWidth))
                {
                    ProbabilityDistribution probFrom = sphere[neighbor.i, neighbor.j, neighbor.k];
                    IntLoc delta = neighbor - queryLoc;
                    MyMatrix trans = Tile.buildTransitionMatrix(delta.i, delta.j, delta.k, set);
                    Double[] probTo = MyMatrix.dot(trans, probFrom.toDoubleArray());
                    sphere[queryLoc.i, queryLoc.j, queryLoc.k] += new ProbabilityDistribution(probTo);
                }
                sphere[queryLoc.i, queryLoc.j, queryLoc.k].normalize();

            }


        }
    }
}
