using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    public class DomainMatrix
    {
        private bool[][] _matrix;

        public DomainMatrix(int dim1, int dim2)
        {
            _matrix = new bool[dim1][];
            for (int i = 0; i < dim2; i++)
            {
                _matrix[i] = new bool[dim2];
            }
        }

        public DomainMatrix(bool[,] m)
        {
            _matrix = new bool[m.GetLength(0)][];
            for (int i = 0; i < m.GetLength(1); i++)
            {
                _matrix[i] = new bool[m.GetLength(1)];
            }

            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    _matrix[i][j] = m[i, j];
                }
            }
        }

        public int Height { get { return _matrix.GetLength(0); } }
        public int Width { get { return _matrix[0].GetLength(0); } }

        public bool this[int x, int y]
        {
            get { return _matrix[x][y]; }
            set { _matrix[x][y] = value; }
        }

        public static DomainMatrix Multiply(DomainMatrix m1, DomainMatrix m2)
        {
            DomainMatrix resultMatrix = new DomainMatrix(m1.Height, m2.Width);
            for (int i = 0; i < resultMatrix.Height; i++)
            {
                for (int j = 0; j < resultMatrix.Width; j++)
                {
                    resultMatrix[i, j] = false;
                    for (int k = 0; k < m1.Width; k++)
                    {
                        resultMatrix[i, j] |= m1[i, k] && m2[k, j];
                    }
                }
            }
            return resultMatrix;
        }

        public static bool[] dot(DomainMatrix m1, bool[] v)
        {
            bool[] mRow;
            Boolean[] resultMatrix = new Boolean[v.Length];
            for (int i = 0; i < m1._matrix.Length; i++)
            {

                mRow = m1._matrix[i];
                resultMatrix[i] = false;
                for (int j = 0; j < mRow.Length; j++)
                {
                    resultMatrix[i] |= mRow[j] && v[j];
                }
            }
            return resultMatrix;
        }

        public bool[,] toBoolArray()
        {
            return (bool[,])_matrix.Clone();
        }
    }
}
