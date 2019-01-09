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


        public void writeChunksToFile(IEnumerable<IntLoc> chunkLocsToExport, string fileTag)
        {

            StringBuilder verticesSb = new StringBuilder();
            StringBuilder textureCoordsSb = new StringBuilder();
            StringBuilder normalsSb = new StringBuilder();
            StringBuilder indicesSb = new StringBuilder();
            int totalVerticesInPreviousChunks = 1; //becasue obj indiexes vertices from 1
            foreach (IntLoc loc in chunkLocsToExport)
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
                Debug.Assert(c.indices.Length % 3 == 0);
                for (int i = 0; i < c.indices.Length; i += 3)
                {
                    indicesSb.Append("f "
                         + (c.indices[i + 2] + totalVerticesInPreviousChunks).ToString() + " "
                         + (c.indices[i + 1] + totalVerticesInPreviousChunks).ToString() + " "
                         + (c.indices[i] + totalVerticesInPreviousChunks).ToString() + "\n");
                }
                totalVerticesInPreviousChunks += c.vertices.Length;


            }
            StringBuilder file = new StringBuilder(10000);
            file.Append(verticesSb);
            file.Append(textureCoordsSb);
            //file.Append(normalsSb);
            file.Append(indicesSb);
            var result = file.ToString();
            System.IO.File.WriteAllText(@"C:\Users\Isaac\Desktop\scratch\obj" + fileTag + ".obj", result);
        }

        public void writeObjFile(IntLoc center)
        {
            var chunksToExport = chunks.Keys.OrderBy(loc => IntLoc.EuclideanDistance(loc, center)).Take(80);
            writeChunksToFile(chunksToExport, "_1");
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
            if (chunks.ContainsKey(chunksLoc))
            {
                lock (chunks[chunksLoc])
                {
                    chunks[chunksLoc].remeshParallelStep(m, chunksLoc.toVector3());
                }
            }
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

        Chunk lastModifiedChunk = null;
        IntLoc LastModifiedChunkLoc = new IntLoc();

        public void set(IntLoc loc, Block val)
        {

            IntLoc chunkLoc = locToChunkLoc(loc);
            Chunk c;
            lock (this) // Lock to prevent race condition where two threads initliaze the same chunk
            {
                
                if (chunkLoc.Equals(LastModifiedChunkLoc) && lastModifiedChunk != null)
                {
                    c = lastModifiedChunk;
                }
                else
                {
                    if (!ChunkLocWithinChunk(chunkLoc))
                    {
                        chunks[chunkLoc] = new Chunk();
                    }
                    c = chunks[chunkLoc];
                }
            }
             
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

                if (IsLocked(c.renderingLock))
                {
                    continue; //someone else is working on this chunk.  let's not wait for them.
                }

                Stopwatch watch = new Stopwatch();
                watch.Start();
                lock (c.renderingLock)
                {
                    if(c.indices == null || c.vertices ==null)
                    {
                        //TODO remove this hack and make it so that the chunk has some reasonable behavior
                        continue;
                    }
                    if (c.vertexBuffer == null)
                    {
                        if (c.hasVertices() || !c.readyToDraw())
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

                    // this should really be limitted to the true extents of the model witin the chunk, but this is a fine approximation,since it will never
                    // cull something that should be visible.
                    BoundingBox box = new BoundingBox(worldMatrix.Translation, worldMatrix.Translation + Vector3.One * Chunk.chunkWidth);

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
