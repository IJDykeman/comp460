using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace dungeon_monogame
{
    struct Block
    {
        int type;
        byte r, g, b;
    }

    class Chunk
    {
        public static readonly int chunkWidth = 16;
        Block[,,] blocks;
        //public short[] indices; //having this be a short could be causing the chunk complexity limit issue
        public VertexPositionColor[] vertices;
        public void remesh()
        {
            vertices = getMesh();
        }

        Vector3[] getVertices()
        {
            Vector3[] back = getXYFace();
            Vector3[] front =  getTransformedXYFace(Matrix.Identity, Vector3.UnitZ);
            Vector3[] left =   getTransformedXYFace(Matrix.CreateRotationY((float)(-Math.PI / 2.0)), Vector3.Zero);
            Vector3[] right =  getTransformedXYFace(Matrix.CreateRotationY((float)(-Math.PI / 2.0)), Vector3.UnitX);
            Vector3[] bottom = getTransformedXYFace(Matrix.CreateRotationX((float)(Math.PI / 2.0)), Vector3.Zero);
            Vector3[] top =    getTransformedXYFace(Matrix.CreateRotationX((float)(Math.PI / 2.0)), Vector3.UnitY);
            return back.Concat(front).Concat(top).Concat(bottom).Concat(left).Concat(right).Concat(bottom).Concat(top).ToArray();
            //return bottom;
        }

        Vector3[] getXYFace()
        {
            Vector3[] result = new Vector3[6];
            result[0] = new Vector3(0, 0, 0);
            result[1] = new Vector3(0, 1, 0);
            result[2] = new Vector3(1, 0, 0);
            result[3] = result[1];
            result[4] = new Vector3(1, 1, 0);
            result[5] = result[2];
            return result;
        }
          
        Vector3[] getTransformedXYFace(Matrix m, Vector3 b)
        {
            Vector3[] f = getXYFace();
            Vector3[] result = new Vector3[6];
            foreach (int i in Enumerable.Range(0, 6))
            {
                result[i] = Vector3.Transform(f[i], m) + b;
            }
            return result;
        }


        VertexPositionColor[] getMesh()
        {
            Vector3[] faces = getVertices();
            VertexPositionColor[] result = new VertexPositionColor[faces.Length];
            
            foreach (int i in Enumerable.Range(0, result.Length))
            {
                result[i].Position = faces[i];
                result[i].Color = Color.Red;
            }
            return result;
        }



    }
}
