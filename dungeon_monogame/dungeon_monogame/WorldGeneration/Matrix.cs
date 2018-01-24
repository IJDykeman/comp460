using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame.WorldGeneration
{
    class MyMatrix
    {
        //private double[,] _matrix;
        private double[][] _matrix;

        public MyMatrix(int dim1, int dim2)
        {
            //_matrix = new double[dim1, dim2];
            _matrix = new double[dim1][];
            for (int i = 0; i < dim2; i++)
            {
                _matrix[i] = new double[dim2];
            }
        }

        public MyMatrix(double[,] m)
        {
            _matrix = new double[m.GetLength(0)][];
            for (int i = 0; i < m.GetLength(1); i++)
            {
                _matrix[i] = new double[m.GetLength(1)];
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

        public double this[int x, int y]
        {
            get { return _matrix[x][y]; }
            set { _matrix[x][y] = value; }
        }

        public static MyMatrix Multiply(MyMatrix m1, MyMatrix m2)
        {
            MyMatrix resultMatrix = new MyMatrix(m1.Height, m2.Width);
            for (int i = 0; i < resultMatrix.Height; i++)
            {
                for (int j = 0; j < resultMatrix.Width; j++)
                {
                    resultMatrix[i, j] = 0;
                    for (int k = 0; k < m1.Width; k++)
                    {
                        resultMatrix[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return resultMatrix;
        }

        public static Double[] dot(MyMatrix m1, Double[] v)
        {
            double[] mRow;
            Double[] resultMatrix = new Double[v.Length];
            //for (int i = 0; i < m1._matrix.Length; i++)
           // {
            Parallel.For(0, m1._matrix.Length, i =>
             {
                 mRow = m1._matrix[i];
                 resultMatrix[i] = 0;
                 for (int j = 0; j < mRow.Length; j++)
                 {
                     resultMatrix[i] += mRow[j] * v[j];
                 }
             });
            //}
            return resultMatrix;
        }

        internal double[,] toDoubleArray()
        {
            return (double[,])_matrix.Clone();
        }
    }
}
