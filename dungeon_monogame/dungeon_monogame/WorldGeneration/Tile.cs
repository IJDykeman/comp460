using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class Tile
    {
        Block[,,] blocks;
        public int tileWidth;
        public Tile(Block[,,] _blocks, int _tileWidth)
        {
            tileWidth = _tileWidth;
            blocks = _blocks;
        }

        public static bool potential(Tile a, Tile b, int di, int dj, int dl)
        {
            bool matchVal = match(a, b, di, dj, dl);
            if (matchVal)
            {
                return true;
            }
            return false;
        }

        public static bool match(Tile a, Tile b, int di, int dj, int dl)
        {
            if (a.tileWidth != b.tileWidth)
            {
                return false;
            }
            int w = a.tileWidth;

            for (int i = 0; i < a.tileWidth; i++)
            {
                for (int j = 0; j < a.tileWidth; j++)
                {
                    if (di == 0 && dj == 1 && dl == 0)
                    {
                        if (!(a.get(i, w - 1, j).Equals(b.get(i, 0, j))))
                        {
                            return false;
                        }
                    }
                    else if (di == 0 && dj == -1 && dl == 0)
                    {
                        if (!(a.get(i, 0, j).Equals(b.get(i, w - 1, j))))
                        {
                            return false;
                        }
                    }

                    else if (di == 1 && dj == 0 && dl == 0)
                    {
                        if (!(a.get(w - 1, i, j).Equals(b.get(0, i, j))))
                        {
                            return false;
                        }
                    }
                    else if (di == -1 && dj == 0 && dl == 0)
                    {
                        if (!(a.get(0, i, j).Equals(b.get(w - 1, i, j))))
                        {
                            return false;
                        }
                    }

                    else if (di == 0 && dj == 0 && dl == 1)
                    {
                        if (!(a.get(i, j, w - 1).Equals(b.get(i, j, 0))))
                        {
                            return false;
                        }
                    }

                    else if (di == 0 && dj == 0 && dl == -1)
                    {
                        if (!(a.get(i, j, 0).Equals(b.get(i, j, w - 1))))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static DomainMatrix buildTransitionMatrix(int di, int dj, int dl, TileSet tiles)
        {
            /*
            double p = Tile.potential(tiles.getTile(0), tiles.getTile(1), 0, 1, 0);
            Console.Write("");
            p = Tile.potential(tiles.getTile(0), tiles.getTile(1), 1, 0, 0);
            Console.Write("");
            p = Tile.potential(tiles.getTile(0), tiles.getTile(1), -1, 0, 0);
            Console.Write("");
            p = Tile.potential(tiles.getTile(0), tiles.getTile(1), 0, 0, 1);
            Console.Write("");*/
            Boolean[,] result = new bool[tiles.size(), tiles.size()];
            for (int i = 0; i < tiles.size(); i++)
            {
                for (int j = 0; j < tiles.size(); j++)
                {
                    Tile a = tiles.getTile(i);
                    Tile b = tiles.getTile(j);

                    if (di == 0 && dj == 1 && dl == 0)
                    {
                        result[i, j] = potential(a, b, di, dj, dl);
                    }
                    else if (di == 0 && dj == -1 && dl == 0)
                    {
                        result[i, j] = potential(a, b, di, dj, dl);
                    }

                    else if (di == 1 && dj == 0 && dl == 0)
                    {
                        result[i, j] = potential(a, b, di, dj, dl);
                    }
                    else if (di == -1 && dj == 0 && dl == 0)
                    {
                        result[i, j] = potential(a, b, di, dj, dl);
                    }

                    else if (di == 0 && dj == 0 && dl == 1)
                    {
                        result[i, j] = potential(a, b, di, dj, dl);
                    }

                    else if (di == 0 && dj == 0 && dl == -1)
                    {
                        result[i, j] = potential(a, b, di, dj, dl);
                    }
                }
            }
            return new DomainMatrix(result);
        }

        public Block get(int i, int p, int j)
        {
            return blocks[i, p, j];
        }

        public List<Tuple<int, int>> ringOrder(int ring)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            // 0 1 2
            // 7   3
            // 6 5 4
            for (int x = 0; x < ring; x++)
            {
                result.Add(new Tuple<int, int>(0, x));
            }
            for (int x = 1; x < ring - 1; x++)
            {
                result.Add(new Tuple<int, int>(x, ring - 1));
            }
            for (int x = ring - 1; x >= 0; x--)
            {
                result.Add(new Tuple<int, int>(ring - 1, x));
            }
            for (int x = ring - 2; x >= 1; x--)
            {
                result.Add(new Tuple<int, int>(x, 0));
            }
            return result;

        }

        public Tile getRotated90()
        {
            List<Tuple<int, int>> l = ringOrder(5);

            Block[,,] newBlocks = new Block[tileWidth, tileWidth, tileWidth];

            for (int ring = 1; ring <= tileWidth; ring += 2)
            {
                List<Tuple<int, int>> ringIndices = ringOrder(ring);
                for (int y = 0; y < tileWidth; y++)
                {
                    for (int i = 0; i < ringIndices.Count; i++)
                    {
                        int offset = (tileWidth - ring) / 2;
                        newBlocks[ringIndices[i].Item1 + offset, y,
                                  ringIndices[i].Item2 + offset] =
                            blocks[ringIndices[(i + ring - 1) % ringIndices.Count].Item1 + offset, y,
                                   ringIndices[(i + ring - 1) % ringIndices.Count].Item2 + offset];
                    }
                    newBlocks[tileWidth / 2, y, tileWidth / 2] = blocks[tileWidth / 2, y, tileWidth / 2];
                }

            }

            return new Tile(newBlocks, tileWidth);
        }

        public override bool Equals(Object obj)
        {
            Tile o = (Tile)obj;
            for (int i = 0; i < tileWidth; i++)
            {
                for (int j = 0; j < tileWidth; j++)
                {
                    for (int k = 0; k < tileWidth; k++)
                    {
                        if (this.get(i, j, k).color != o.get(i, j, k).color)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;

        }
    }
}
