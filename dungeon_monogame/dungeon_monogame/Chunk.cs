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

    class Chunk
    {
        public static readonly int chunkWidth = 8;
        private Block[,,] blocks;
        //public short[] indices; //having this be a short could be causing the chunk complexity limit issue
        private VertexPostitionColorPaintNormal[] vertices;
        private short[] indices;
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;
        bool meshValid = false;

        private static readonly Vector3[] probes = new Vector3[]{
                new Vector3(.5f, .5f, .5f),
                new Vector3(.5f, .5f, -.5f),
                new Vector3(.5f, -.5f, .5f),
                new Vector3(.5f, -.5f, -.5f)
            };


        public Chunk()
        {
            
            blocks = new Block[chunkWidth, chunkWidth, chunkWidth];
            remesh();
            for (int i=0; i<chunkWidth; i++)
            {
                for (int j = 0; j < chunkWidth; j++)
                {
                    for (int k = 0; k < chunkWidth; k++)
                    {
                            blocks[i, j, k] = new Block(0, 0, 0, 0);
                    }
                }
            }
        }

        public Block getBlock(IntLoc loc)
        {
            return blocks[loc.i, loc.j, loc.k];
        }

        public void setBlock(IntLoc loc, Block val)
        {
            blocks[loc.i, loc.j, loc.k] = val;
            meshValid = false;
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
            vertices = getChunkMesh();
            if (vertices.Length == 0){
                return;
            }
            indices = new short[vertices.Length];
            for (short i = 0; i < vertices.Length; i++)
            {
                indices[i] = i;
            }
            vertexBuffer = new VertexBuffer(Game1.graphics.GraphicsDevice, VertexPostitionColorPaintNormal.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPostitionColorPaintNormal>(vertices);
            indexBuffer = new IndexBuffer(Game1.graphics.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
            meshValid = true;
        }

        VertexPostitionColorPaintNormal[] getVertices(Vector3 offset, Color c)
        {
            IEnumerable< VertexPostitionColorPaintNormal> list = new VertexPostitionColorPaintNormal[0];
            //Color c =;
            //Color c = Color.LightGray;
            if (!solid(new IntLoc(offset - Vector3.UnitZ))){
                VertexPostitionColorPaintNormal[] back = getTransformedXYFace(Matrix.CreateRotationY((float)(-Math.PI)), Vector3.UnitX,
                    Matrix.CreateRotationY((float)Math.PI / 2f), offset, c);
                list = list.Concat(back); 
            }
            if (!solid(new IntLoc(offset + Vector3.UnitZ)))
            {
                VertexPostitionColorPaintNormal[] front = getTransformedXYFace(Matrix.Identity, Vector3.UnitZ,
                    Matrix.CreateRotationY(-(float)Math.PI / 2f), offset, c);
                list = list.Concat(front);
            }
            if (!solid(new IntLoc(offset - Vector3.UnitX))){
                VertexPostitionColorPaintNormal[] left = getTransformedXYFace(Matrix.CreateRotationY((float)(-Math.PI / 2.0)), Vector3.Zero,
                    Matrix.CreateRotationY((float)Math.PI), offset, c);
                list = list.Concat(left);
            }
            if (!solid(new IntLoc(offset + Vector3.UnitX)))
            {
                VertexPostitionColorPaintNormal[] right = getTransformedXYFace(Matrix.CreateRotationY((float)(Math.PI / 2.0)), Vector3.UnitX + Vector3.UnitZ,
                    Matrix.Identity, offset, c);
                list = list.Concat(right);
            }
            if (!solid(new IntLoc(offset - Vector3.UnitY)))
            {
                VertexPostitionColorPaintNormal[] bottom = getTransformedXYFace(Matrix.CreateRotationX((float)(Math.PI / 2.0)), Vector3.Zero,
                    Matrix.CreateRotationZ(-(float)Math.PI/2f), offset, c);
                list = list.Concat(bottom);
            }
            if (!solid(new IntLoc(offset + Vector3.UnitY)))
            {
                VertexPostitionColorPaintNormal[] top = getTransformedXYFace(Matrix.CreateRotationX((float)(-Math.PI / 2.0)), Vector3.UnitY + Vector3.UnitZ,
                    Matrix.CreateRotationZ((float)Math.PI/2f), offset, c);
                list = list.Concat(top);
            }
            VertexPostitionColorPaintNormal[] result = list.ToArray();

            return result;
        }

        void populateXYFace(VertexPostitionColorPaintNormal[] vertices)
        {
            vertices[0].Position = new Vector3(0, 0, 0);
            vertices[1].Position = new Vector3(0, 1, 0);
            vertices[2].Position = new Vector3(1, 0, 0);
            vertices[3].Position = vertices[1].Position;
            vertices[4].Position = new Vector3(1, 1, 0);
            vertices[5].Position = vertices[2].Position;
        }

        VertexPostitionColorPaintNormal[] getTransformedXYFace(Matrix m, Vector3 b, Matrix unitXToNormal, Vector3 offset, Color c)
        {
            Vector3 normal = Vector3.Transform(Vector3.UnitX, unitXToNormal);

            VertexPostitionColorPaintNormal[] result = new VertexPostitionColorPaintNormal[6];
            populateXYFace(result);
            foreach (int i in Enumerable.Range(0, 6))
            {

                result[i].Position = Vector3.Transform(result[i].Position, m) + b + offset;
                result[i].Normal = normal;
                result[i].Color = c;
                Vector3 indirectLighting = Color.White.ToVector3();
                foreach (Vector3 p in probes)
                {
                    Vector3 probe = Vector3.Transform(p, unitXToNormal);
                    IntLoc loc = new IntLoc(result[i].Position + probe);
                    if (solid(loc))
                    {
                        float colorTransmissionFactor = .5f;
                        indirectLighting *=  colorTransmissionFactor * getBlock(loc).color.ToVector3()
                                                + (1-colorTransmissionFactor) * Color.White.ToVector3();
                    }
                }
                result[i].IndirectLightColor = new Color(indirectLighting);

            }
            return result;
        }


        VertexPostitionColorPaintNormal[] getChunkMesh()
        {
            List< VertexPostitionColorPaintNormal> result = new List<VertexPostitionColorPaintNormal>();
            for (int i = 0; i < chunkWidth; i++)
            {
                for (int j = 0; j < chunkWidth; j++)
                {
                    for (int k = 0; k < chunkWidth; k++)
                    {
                        if (blocks[i, j, k].type == 1){
                            var verts = getVertices(new Vector3(i, j, k), blocks[i,j,k].color);
                            result.AddRange(verts);
                        }
                    }
                }
            }
            return result.ToArray();
            
        }

        internal bool empty()
        {
            return vertices.Length == 0;
        }
    }
}
