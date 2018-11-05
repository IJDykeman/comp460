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
        public static readonly int chunkWidth = 32; // 14
        private Block[,,] blocks;
        //public short[] indices; //having this be a short could be causing the chunk complexity limit issue
        public VertexPostitionColorPaintNormal[] vertices;
        public int[] indices;
        bool meshReflectsBlocks = false;
        public IndexBuffer indexBuffer;
        public VertexBuffer vertexBuffer;


        private static readonly Vector3[] probes = new Vector3[]{
                new Vector3(.5f, .5f, .5f),
                new Vector3(.5f, .5f, -.5f),
                new Vector3(.5f, -.5f, .5f),
                new Vector3(.5f, -.5f, -.5f)
            };

        public bool needsRemesh()
        {
            return !meshReflectsBlocks;
        }

        public Chunk()
        {
            
            blocks = new Block[chunkWidth, chunkWidth, chunkWidth];
            //remesh();
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
            lock (this)
            {
                blocks[loc.i, loc.j, loc.k] = val;
                meshReflectsBlocks = false;
            }
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


        public bool solid(IntLoc offset, IntLoc chunkLoc, ChunkManager m)
        {
            if (withinBounds(offset))
            {
                return solid(offset);
            }
            return m.solid(offset + chunkLoc);
        }

        public Block getBlock(IntLoc offset, IntLoc chunkLoc, ChunkManager m)
        {
            if (withinBounds(offset))
            {
                return getBlock(offset);
            }
            return m.getBlock(offset + chunkLoc);
        }


        public void remeshParallelStep(ChunkManager space, Vector3 chunkLoc)
        {

                if (!needsRemesh())
                {
                    return;
                }
                vertices = getChunkMesh(space, chunkLoc);
                if (vertices.Length == 0)
                {
                    meshReflectsBlocks = true;
                    return;
                }

                indices = new int[vertices.Length / 4 * 6];
                for (int i = 0; i < vertices.Length; i += 4)
                {
                    indices[i / 4 * 6 + 0] = i;
                    indices[i / 4 * 6 + 1] = (int)(i + 1);
                    indices[i / 4 * 6 + 2] = (int)(i + 2);
                    indices[i / 4 * 6 + 3] = (int)(i + 1);
                    indices[i / 4 * 6 + 4] = (int)(i + 3);
                    indices[i / 4 * 6 + 5] = (int)(i + 2);
                }
            //lock (this)
            //{
                vertexBuffer = null;
                indexBuffer = null;
                
                meshReflectsBlocks = true;
            //}

        }

        VertexPostitionColorPaintNormal[] getVertices(Vector3 offset, Vector3 chunkLoc, Color c, ChunkManager space)
        {
            IntLoc chunkLocInt = new IntLoc(chunkLoc);
            IEnumerable< VertexPostitionColorPaintNormal> list = new VertexPostitionColorPaintNormal[0];
            //Color c =;
            //Color c = Color.LightGray;
            if (!solid(new IntLoc(offset - Vector3.UnitZ), chunkLocInt, space)){
                VertexPostitionColorPaintNormal[] back = getTransformedXYFace(Matrix.CreateRotationY((float)(-Math.PI)), Vector3.UnitX,
                    Matrix.CreateRotationY((float)Math.PI / 2f), offset, c, space, chunkLocInt);
                list = list.Concat(back); 
            }
            if (!solid(new IntLoc(offset + Vector3.UnitZ), chunkLocInt, space))
            {
                VertexPostitionColorPaintNormal[] front = getTransformedXYFace(Matrix.Identity, Vector3.UnitZ,
                    Matrix.CreateRotationY(-(float)Math.PI / 2f), offset, c, space, chunkLocInt);
                list = list.Concat(front);
            }
            if (!solid(new IntLoc(offset - Vector3.UnitX), chunkLocInt, space)){
                VertexPostitionColorPaintNormal[] left = getTransformedXYFace(Matrix.CreateRotationY((float)(-Math.PI / 2.0)), Vector3.Zero,
                    Matrix.CreateRotationY((float)Math.PI), offset, c, space, chunkLocInt);
                list = list.Concat(left);
            }
            if (!solid(new IntLoc(offset + Vector3.UnitX), chunkLocInt, space))
            {
                VertexPostitionColorPaintNormal[] right = getTransformedXYFace(Matrix.CreateRotationY((float)(Math.PI / 2.0)), Vector3.UnitX + Vector3.UnitZ,
                    Matrix.Identity, offset, c, space, chunkLocInt);
                list = list.Concat(right);
            }
            if (!solid(new IntLoc(offset - Vector3.UnitY), chunkLocInt, space))
            {
                VertexPostitionColorPaintNormal[] bottom = getTransformedXYFace(Matrix.CreateRotationX((float)(Math.PI / 2.0)), Vector3.Zero,
                    Matrix.CreateRotationZ(-(float)Math.PI / 2f), offset, c, space, chunkLocInt);
                list = list.Concat(bottom);
            }
            if (!solid(new IntLoc(offset + Vector3.UnitY), chunkLocInt, space))
            {
                VertexPostitionColorPaintNormal[] top = getTransformedXYFace(Matrix.CreateRotationX((float)(-Math.PI / 2.0)), Vector3.UnitY + Vector3.UnitZ,
                    Matrix.CreateRotationZ((float)Math.PI / 2f), offset, c, space, chunkLocInt);
                list = list.Concat(top);
            }
            VertexPostitionColorPaintNormal[] result = list.ToArray();

            return result;
        }

        public bool readyToDraw()
        {
            return vertices != null;
        }

        void populateXYFace(VertexPostitionColorPaintNormal[] vertices)
        {
            vertices[0].Position = new Vector3(0, 0, 0);
            vertices[1].Position = new Vector3(0, 1, 0);
            vertices[2].Position = new Vector3(1, 0, 0);
            //vertices[3].Position = vertices[1].Position;
            vertices[3].Position = new Vector3(1, 1, 0);
            //vertices[5].Position = vertices[2].Position;
        }

        VertexPostitionColorPaintNormal[] getTransformedXYFace(Matrix m, Vector3 b, Matrix unitXToNormal, Vector3 offset, Color c, ChunkManager space, IntLoc chunkLoc)
        {
            Vector3 normal = Vector3.Transform(Vector3.UnitX, unitXToNormal);

            VertexPostitionColorPaintNormal[] result = new VertexPostitionColorPaintNormal[4];
            populateXYFace(result);
            foreach (int i in Enumerable.Range(0, result.Length))
            {

                result[i].Position = Vector3.Transform(result[i].Position, m) + b + offset;
                result[i].Normal = normal;
                result[i].Color = c;
                Vector3 indirectLighting = Color.White.ToVector3();
                foreach (Vector3 p in probes)
                {
                    Vector3 probe = Vector3.Transform(p, unitXToNormal);
                    if (solid(new IntLoc(result[i].Position + probe), chunkLoc, space))
                    {
                        float colorTransmissionFactor = 0f;
                        indirectLighting *=  colorTransmissionFactor * getBlock(new IntLoc(result[i].Position + probe), chunkLoc, space).color.ToVector3()
                                                + (1-colorTransmissionFactor) * Color.LightGray.ToVector3();
                    }
                }
                result[i].IndirectLightColor = new Color(indirectLighting);

            }
            return result;
        }


        VertexPostitionColorPaintNormal[] getChunkMesh(ChunkManager space, Vector3 chunkLoc)
        {
            List< VertexPostitionColorPaintNormal> result = new List<VertexPostitionColorPaintNormal>();
            for (int i = 0; i < chunkWidth; i++)
            {
                for (int j = 0; j < chunkWidth; j++)
                {
                    for (int k = 0; k < chunkWidth; k++)
                    {
                        if (blocks[i, j, k].type == 1){
                            var verts = getVertices(new Vector3(i, j, k), chunkLoc, blocks[i,j,k].color, space);
                            result.AddRange(verts);
                        }
                    }
                }
            }
            return result.ToArray();
            
        }

        internal bool empty()
        {
            lock (this)
            {
                if (vertices == null)
                {
                    return false;
                }
                return vertices.Length == 0;
            }
        }

        public void forgetMesh()
        {
            lock (this)
            {
                vertices = null;
                indices = null;
                meshReflectsBlocks = false;
                //vertexBuffer.Dispose();
                vertexBuffer = null;
                //indexBuffer.Dispose();
                indexBuffer = null;
            }
        }
    }
}
