using dungeon_monogame.WorldGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class ChunkManager
    {
        ConcurrentDictionary<IntLoc, Chunk> chunks;
        public float ambient_light { get; set; }
        private int xmin, xmax, ymin, ymax, zmin, zmax;

        public ChunkManager()
        {
            chunks = new ConcurrentDictionary<IntLoc, Chunk>();

        }


        public void createMesh()
        {

            StringBuilder verticesSb = new StringBuilder();
            StringBuilder textureCoordsSb = new StringBuilder();
            StringBuilder normalsSb = new StringBuilder();
            StringBuilder indicesSb = new StringBuilder();
            int totalIndicesInPreviousChunks = 0;
            foreach (IntLoc loc in chunks.Keys)
            {
                Chunk c = chunks[loc];
                if(c.vertices == null)
                {
                    continue;
                }
                for (int i = 0; i < c.vertices.Length; i++)
                {

                    var pos = c.vertices[i].Position;
                    pos += loc.toVector3();
                    //vertices.Add(c.vertices[i].Position);
                    verticesSb.Append("v "
                        + pos.X.ToString() + " "
                        + pos.Y.ToString() + " "
                        + pos.Z.ToString() + "\n");

                    /*normalsSb.Append("vn "
                        + c.vertices[i].Normal.X.ToString() + " "
                        + c.vertices[i].Normal.Y.ToString() + " "
                        + c.vertices[i].Normal.Z.ToString() + "\n");*/

                    textureCoordsSb.Append("vt "
                        + 1.ToString() + " "
                        + 1.ToString() + "\n");
                }
                for (int i = 0; i < c.indices.Length; i += 3)
                {
                    indicesSb.Append("f "
                         + (1+c.indices[i + 2] + totalIndicesInPreviousChunks).ToString() + " "
                         + (1+c.indices[i + 1] + totalIndicesInPreviousChunks).ToString() + " "
                         + (1+c.indices[i] + totalIndicesInPreviousChunks).ToString() + "\n");
                }
                totalIndicesInPreviousChunks += c.indices.Length;


            }
            StringBuilder file = new StringBuilder(10000);
            file.Append(verticesSb);
            file.Append(textureCoordsSb);
            //file.Append(normalsSb);
            file.Append(indicesSb);
            var result = file.ToString();
            System.IO.File.WriteAllText(@"C:\Users\Isaac\Desktop\scratch\obj.obj", result);



        }

        public bool chunkNeedsMesh(IntLoc chunksLoc)
        {
            Chunk c;
            if (chunks.TryGetValue(chunksLoc, out c))
            {
                return c.needsRemesh();
            }
            return false;
        }

        public void remeshAllSerial()
        {

            foreach (IntLoc l in chunks.Keys)
            {
                chunks[l].remeshParallelStep(this, l.toVector3());
            }

        }

        public string getReport()
        {
            return "Number of chunks: " + chunks.Count;
        }
        

        public void remesh(ChunkManager m, IntLoc chunksLoc)
        {
            chunks[chunksLoc].remeshParallelStep(m, chunksLoc.toVector3());
        }


        public bool solid(IntLoc l)
        {
            Chunk c;
            if (chunks.TryGetValue(locToChunkLoc(l), out c))
            {
                return c.solid(l % Chunk.chunkWidth);
            }
            return false;
            //if (!withinChunk(l))
            // {
            //    return false;
            //}
            //return chunks[locToChunkLoc(l)].solid(l % Chunk.chunkWidth);
        }

        public Block getBlock(IntLoc l)
        {
            return chunks[locToChunkLoc(l)].getBlock(l % Chunk.chunkWidth);
        }

        public void set(IntLoc loc, Block val)
        {

            IntLoc chunkLoc = locToChunkLoc(loc);
            lock (this) // Lock to prevent race condition where two threads initliaze the same chunk
            {
                if (!ChunkLocWithinChunk(chunkLoc))
                {
                    chunks[chunkLoc] = new Chunk();
                }
            }
            Chunk c = chunks[chunkLoc];
            IntLoc setLoc = loc % Chunk.chunkWidth;
            c.setBlock(setLoc, val);

        }

        public void setExtents(int _xmin, int _xmax, int _ymin, int _ymax, int _zmin, int _zmax)
        {
            xmin = _xmin; xmax = _xmax;
            ymin = _ymin; ymax = _ymax;
            zmin = _zmin; zmax = _zmax;
        }

        public Vector3 getCenter()
        {
            return new Vector3(
                (xmax - xmin) / 2f + .5f,
                (ymax - ymin) / 2f + .5f,
                (zmax - zmin) / 2f + .5f
                );
        }

        public AABB getAaabbFromModelExtents()
        {
            return new AABB(
                (xmax - xmin) + 1,
                (ymax - ymin) + 1,
                (zmax - zmin) + 1);
        }

        public static IntLoc locToChunkLoc(IntLoc l)
        {
            return l - (l % Chunk.chunkWidth);
        }

        private bool ChunkLocWithinChunk(IntLoc loc)
        {
            return chunks.ContainsKey(loc);
        }

        bool IsLocked(object o)
        {
            if (!Monitor.TryEnter(o))
                return true;
            Monitor.Exit(o);
            return false;
        }

        public void draw(Effect effect, Matrix transform, Color emission,  BoundingFrustum frustum = null)
        {
            foreach (KeyValuePair<IntLoc, Chunk> p in chunks)
            {
                Chunk c = p.Value;
                IntLoc loc = p.Key;

                if (IsLocked(c))
                {
                    continue; //someone else is working on this chunk.  let's not wait for them.
                }

                Stopwatch watch = new Stopwatch();
                watch.Start();
                lock (c)
                {
                    if(c.indices == null || c.vertices ==null)
                    {
                        //TODO remove this hack and make it so that the chunk has some reasonable behavior
                        continue;
                    }
                    if (c.vertexBuffer == null)
                    {
                        if (c.empty() || !c.readyToDraw())
                        {
                            continue;
                        }

                        Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


                        c.indexBuffer = new IndexBuffer(Game1.graphics.GraphicsDevice, typeof(int), c.indices.Length, BufferUsage.WriteOnly);
                        c.indexBuffer.SetData(c.indices);

                        c.vertexBuffer = new VertexBuffer(Game1.graphics.GraphicsDevice, typeof(VertexPostitionColorPaintNormal), c.vertices.Length, BufferUsage.WriteOnly);
                        c.vertexBuffer.SetData<VertexPostitionColorPaintNormal>(c.vertices);
                    }
                    Matrix oldWorldMat = effect.Parameters["xWorld"].GetValueMatrix();
                    Matrix worldMatrix = Matrix.Multiply(oldWorldMat, Matrix.CreateTranslation(loc.toVector3()) * transform);
                    effect.Parameters["xWorld"].SetValue(worldMatrix);
                    effect.Parameters["xEmissive"].SetValue(emission.ToVector4());

                    //BoundingBox box = new BoundingBox(Vector3.Transform(new Vector3(), worldMatrix),
                    //                Vector3.Transform(Vector3.One * Chunk.chunkWidth, worldMatrix));
                    BoundingBox box = new BoundingBox(loc.toVector3(), loc.toVector3() + Vector3.One * Chunk.chunkWidth);

                    if (frustum != null)
                    {
                        if (frustum.Intersects(box))
                        {


                            Game1.graphics.GraphicsDevice.SetVertexBuffer(c.vertexBuffer);
                            Game1.graphics.GraphicsDevice.Indices = c.indexBuffer;
                            foreach (var pass in effect.CurrentTechnique.Passes)
                            {
                                pass.Apply();

                                try // if the chunk has been unmeshed since the beginning of the draw call, the call will fail
                                {

                                    Game1.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                        c.indices.Length / 3);
                                }
                                catch (Exception e)
                                {

                                }
                                finally { }
                            }



                            //}
                        }
                    }
                    effect.Parameters["xWorld"].SetValue(oldWorldMat);
                }
                watch.Stop();
                if(watch.ElapsedMilliseconds > 0){

                }
            }
        }

        public void unmeshOutsideRange(int unmeshRadius)
        {
            foreach (IntLoc l in chunks.Keys)
            {
                if(IntLoc.EuclideanDistance(l, new IntLoc(TileMap.playerPerspectiveLoc)) > unmeshRadius)
                {
                    chunks[l].forgetMesh();
                }
            }
        }
    }
}
