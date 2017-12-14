using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;


namespace dungeon_monogame
{
    struct Block
    {
        public int type;
        public byte r, g, b;

        public Block(int t, byte _r, byte _g, byte _b)
        {
            type = t;
            r = _r;
            g = _g;
            b = _b;
        }
    }

    struct IntLoc
    {
        public int i, j, k;

        public IntLoc(int _i, int _j, int _k)
        {
            i = _i;
            j = _j;
            k = _k;
        }
        public IntLoc(Vector3 v)
        {
            i = (int)v.X;
            j = (int)v.Y;
            k = (int)v.Z;
        }
    }

    class Chunk
    {
        public static readonly int chunkWidth = 8;
        Block[,,] blocks;
        //public short[] indices; //having this be a short could be causing the chunk complexity limit issue
        public VertexPostitionColorPaintNormal[] vertices;

        public Chunk()
        {
            blocks = new Block[chunkWidth, chunkWidth, chunkWidth];
            for (int i=0; i<chunkWidth; i++)
            {
                for (int j = 0; j < chunkWidth; j++)
                {
                    for (int k = 0; k < chunkWidth; k++)
                    {
                        if (Globals.r.Next(0,10) > 5)
                        {
                            blocks[i, j, k] = new Block(1, 100, 200, 100);
                        }
                        else {
                            blocks[i, j, k] = new Block(0, 100, 200, 100);
                        }

                    }
                }
            }
        }

        public Block getBlock(IntLoc loc)
        {
            return blocks[loc.i, loc.j, loc.k];
        }

        public bool withinBounds(IntLoc loc)
        {
            return !(loc.i < 0 || loc.i >= chunkWidth || loc.j < 0 || loc.j >= chunkWidth || loc.k < 0 || loc.k >= chunkWidth);
        }

        public bool solid(IntLoc loc)
        {
            if (!withinBounds(loc))
            {
                return false;
            }
            return getBlock(loc).type != 0;
        }

        public void remesh()
        {
            vertices = getMesh();
        }

        VertexPostitionColorPaintNormal[] getVertices(Vector3 offset)
        {
            IEnumerable< VertexPostitionColorPaintNormal> list = new VertexPostitionColorPaintNormal[0];
            Color c = new Color(Globals.r.Next(0, 256), Globals.r.Next(0, 256), Globals.r.Next(0, 256));
            if (!solid(new IntLoc(offset - Vector3.UnitZ))){
                VertexPostitionColorPaintNormal[] back = getTransformedXYFace(Matrix.Identity, Vector3.Zero, new Vector3(0, 0, -1), c);
                list = list.Concat(back); 
            }
            if (!solid(new IntLoc(offset + Vector3.UnitZ)))
            {
                VertexPostitionColorPaintNormal[] front = getTransformedXYFace(Matrix.Identity, Vector3.UnitZ, new Vector3(0, 0, 1), c);
                list = list.Concat(front);
            }
            if (!solid(new IntLoc(offset - Vector3.UnitX))){
                VertexPostitionColorPaintNormal[] left = getTransformedXYFace(Matrix.CreateRotationY((float)(-Math.PI / 2.0)), Vector3.Zero, new Vector3(-1, 0, 0), c);
                list = list.Concat(left);
            }
            if (!solid(new IntLoc(offset + Vector3.UnitX)))
            {
                VertexPostitionColorPaintNormal[] right = getTransformedXYFace(Matrix.CreateRotationY((float)(-Math.PI / 2.0)), Vector3.UnitX, new Vector3(1, 0, 0), c);
                list = list.Concat(right);
            }
            if (!solid(new IntLoc(offset - Vector3.UnitY)))
            {
                VertexPostitionColorPaintNormal[] bottom = getTransformedXYFace(Matrix.CreateRotationX((float)(Math.PI / 2.0)), Vector3.Zero, new Vector3(0, -1, 0), c);
                list = list.Concat(bottom);
            }
            if (!solid(new IntLoc(offset + Vector3.UnitY)))
            {
                VertexPostitionColorPaintNormal[] top = getTransformedXYFace(Matrix.CreateRotationX((float)(Math.PI / 2.0)), Vector3.UnitY, new Vector3(0, 1, 0), c);
                list = list.Concat(top);
            }
            VertexPostitionColorPaintNormal[] result = list.ToArray();
            for (int i=0; i<result.Length; i++)
            {
                result[i].Position += offset;
            }
            return result;
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

        VertexPostitionColorPaintNormal[] getTransformedXYFace(Matrix m, Vector3 b, Vector3 normal, Color c)
        {
            VertexPostitionColorPaintNormal[] result = new VertexPostitionColorPaintNormal[6];
            Vector3[] f = getXYFace();
            foreach (int i in Enumerable.Range(0, 6))
            {
                result[i].Position = Vector3.Transform(f[i], m) + b;
                result[i].Normal = normal;
                result[i].Color = c;
                //result[i].PaintColor = new Color(Globals.r.Next(0, 256), Globals.r.Next(0, 256), Globals.r.Next(0, 256)); //Color.Red;
            }
            return result;
        }


        VertexPostitionColorPaintNormal[] getMesh()
        {
            //return getVertices(new Vector3(0, 0, 0));
            List< VertexPostitionColorPaintNormal> result = new List<VertexPostitionColorPaintNormal>();
            for (int i = 0; i < chunkWidth; i++)
            {
                for (int j = 0; j < chunkWidth; j++)
                {
                    for (int k = 0; k < chunkWidth; k++)
                    {
                        if (blocks[i, j, k].type == 1){
                            var verts = getVertices(new Vector3(i, j, k));
                            result.AddRange(verts);
                            //return result.ToArray();
                        }
                    }
                }
            }
            return result.ToArray();
            
        }



    }
}
