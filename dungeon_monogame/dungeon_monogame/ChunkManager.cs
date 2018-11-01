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


        public bool chunkNeedsMesh(IntLoc chunksLoc)
        {
            Chunk c;
            if (chunks.TryGetValue(chunksLoc, out c))
            {
                return c.needsRemesh();
            }
            return false;
        }

        public void remeshAllSerial(int centeri = 0, int centerj = 0, int centerk = 0)
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

        public void remeshAllParallelizeableStep(Func<IntLoc, bool> decided)
        {
            /*IntLoc l;
            while (chunkLocationsNeedingRemesh.TakeMinInLinearTime(a => (Math.Abs(a.i - TileMap.playerPerspectiveLoc.X) 
                                                                                + Math.Abs(a.j - TileMap.playerPerspectiveLoc.Y) * 5 
                                                                                + Math.Abs(a.k - TileMap.playerPerspectiveLoc.Z)), out l))
            {
                float d = IntLoc.EuclideanDistance(l, new IntLoc(TileMap.playerPerspectiveLoc));
                if (d < TileMap.alwaysMeshWithinRange)
                {
                    chunks[l].remeshParallelStep(this, l.toVector3());
                }
                else
                {
                    chunkLocationsNeedingRemesh.Add(l);
                }
            }*/
            IntLoc centerTilePos = new IntLoc(TileMap.playerPerspectiveLoc);
            IntLoc toMeshTileLoc;
            foreach (IntLoc BFSloc in Globals.gridBFS(WorldGenParamaters.decideTilesWithinWidth))
            {
                toMeshTileLoc = new IntLoc(-WorldGenParamaters.decideTilesWithinWidth / 2) + BFSloc + centerTilePos;
                if (decided(toMeshTileLoc))
                {
                    IntLoc ToMeshChunkLoc = locToChunkLoc(toMeshTileLoc * WorldGenParamaters.tileWidth);
                    chunks[ToMeshChunkLoc].remeshParallelStep(this, ToMeshChunkLoc.toVector3());
                    break;

                }

            }
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
            lock (this)
            {
                if (!withinChunk(loc))
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

        private bool withinChunk(IntLoc loc)
        {
            return chunks.ContainsKey(locToChunkLoc(loc));
        }

        bool IsLocked(object o)
        {
            if (!Monitor.TryEnter(o))
                return true;
            Monitor.Exit(o);
            return false;
        }

        public void draw(Effect effect, Matrix transform, Color emission, BoundingFrustum frustum)
        {
            bool alreadyPushedABuffer = false;
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
                    if (c.vertexBuffer == null)
                    {

                        if (alreadyPushedABuffer)
                        {
                            //continue;
                        }


                        if (c.empty() || !c.readyToDraw())
                        {
                            continue;
                        }

                        Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;


                        c.indexBuffer = new IndexBuffer(Game1.graphics.GraphicsDevice, typeof(int), c.indices.Length, BufferUsage.WriteOnly);
                        c.indexBuffer.SetData(c.indices);

                        c.vertexBuffer = new VertexBuffer(Game1.graphics.GraphicsDevice, typeof(VertexPostitionColorPaintNormal), c.vertices.Length, BufferUsage.WriteOnly);
                        c.vertexBuffer.SetData<VertexPostitionColorPaintNormal>(c.vertices);
;
                        alreadyPushedABuffer = true;
                    }
                    Matrix oldWorldMat = effect.Parameters["xWorld"].GetValueMatrix();
                    Matrix worldMatrix = Matrix.Multiply(oldWorldMat, Matrix.CreateTranslation(loc.toVector3()) * transform);
                    effect.Parameters["xWorld"].SetValue(worldMatrix);
                    effect.Parameters["xEmissive"].SetValue(emission.ToVector4());

                    BoundingBox box = new BoundingBox(Vector3.Transform(new Vector3(), worldMatrix),
                                    Vector3.Transform(Vector3.One * Chunk.chunkWidth, worldMatrix));

                    //if (!frustum.Intersects(box))
                    //{
                    //  continue;
                    //}

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
                    effect.Parameters["xWorld"].SetValue(oldWorldMat);
                }
                watch.Stop();
                if(watch.ElapsedMilliseconds > 0){

                }
            }
        }

        public void unmeshOutsideRange()
        {
            foreach (IntLoc l in chunks.Keys)
            {
                if(IntLoc.EuclideanDistance(l, new IntLoc(TileMap.playerPerspectiveLoc)) > TileMap.alwaysUnmeshOutsideRange)
                {
                    chunks[l].forgetMesh();
                }
            }
        }
    }
}
