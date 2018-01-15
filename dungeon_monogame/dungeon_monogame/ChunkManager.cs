using dungeon_monogame.WorldGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class ChunkManager
    {
        ConcurrentDictionary<IntLoc, Chunk> chunks;
        ConcurrentHashSet<IntLoc> chunkLocationsNeedingRemesh;
        ConcurrentHashSet<IntLoc> chunkLocationsNeedingBuffersSet;
        public float ambient_light { get; set; }
        private int xmin, xmax, ymin, ymax, zmin, zmax;
        

        public ChunkManager()
        {
            chunks = new ConcurrentDictionary<IntLoc, Chunk>();
            chunkLocationsNeedingRemesh = new ConcurrentHashSet<IntLoc>();
            chunkLocationsNeedingBuffersSet = new ConcurrentHashSet<IntLoc>();

        }



        public void remeshAllSerial(int centeri = 0, int centerj = 0, int centerk = 0)
        {

            IntLoc l;
            while (chunkLocationsNeedingRemesh.TakeMinInLinearTime(a => (Math.Abs(a.i - centeri) + Math.Abs(a.j - centerj) * 4 + Math.Abs(a.k - centerk)), out l))
            {

                chunks[l].remeshParallelStep(this, l.toVector3());
                chunks[l].remeshSerialStep();
            }
        }



        public void remeshAllParallelizeableStep(IntLoc center)
        {
            center = new IntLoc(TileMap.playerPerspectiveLoc);
            IntLoc l;

            while (chunkLocationsNeedingRemesh.TakeMinInLinearTime(a => (Math.Abs(a.i - center.i) + Math.Abs(a.j - center.j) + Math.Abs(a.k - center.k)), out l))
            {
                chunks[l].remeshParallelStep(this, l.toVector3());
                chunkLocationsNeedingBuffersSet.Add(l);
                center = new IntLoc(TileMap.playerPerspectiveLoc);
            }
        }

        public void remeshAllSerialStep(IntLoc center)
        {
            center = new IntLoc(TileMap.playerPerspectiveLoc);
            IntLoc l;
            while (chunkLocationsNeedingBuffersSet.TakeMinInLinearTime(a => (Math.Abs(a.i - center.i) + Math.Abs(a.j - center.j) + Math.Abs(a.k - center.k)), out l))
            {
                chunks[l].remeshSerialStep();
                center = new IntLoc(TileMap.playerPerspectiveLoc);
            }
        }



        public bool solid(IntLoc l)
        {
            Chunk c;
            if(chunks.TryGetValue(locToChunkLoc(l), out c))
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

            if (!withinChunk(loc))
            {
                chunks[chunkLoc] = new Chunk();
            }
            Chunk c = chunks[chunkLoc];
            IntLoc setLoc = loc % Chunk.chunkWidth;
            c.setBlock(setLoc, val);
            if (!chunkLocationsNeedingRemesh.Contains(chunkLoc))
            {
                chunkLocationsNeedingRemesh.Add(chunkLoc);
            }
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

        private IntLoc locToChunkLoc(IntLoc l)
        {
            return l - (l % Chunk.chunkWidth);
        }

        private bool withinChunk(IntLoc loc)
        {
            return chunks.ContainsKey(locToChunkLoc(loc));
        }

        public void draw(Effect effect, Matrix transform, Color emission)
        {
            foreach (KeyValuePair<IntLoc, Chunk> p in chunks)
            {
                
                Matrix oldWorldMat = effect.Parameters["xWorld"].GetValueMatrix();
                Chunk c = p.Value;
                if (c.empty() || !c.readyToDisplay())
                {
                    continue;
                }
                IntLoc loc = p.Key;
                Game1.graphics.GraphicsDevice.Indices = c.indexBuffer;
                Game1.graphics.GraphicsDevice.SetVertexBuffer(c.vertexBuffer);
                Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                
                effect.Parameters["xWorld"].SetValue(Matrix.Multiply(oldWorldMat, Matrix.CreateTranslation(loc.toVector3()) * transform));
                //effect.Parameters["xAmbient"].SetValue(ambient_light);
                //effect.Parameters["xEmissive"].SetValue(emission.ToVector4());
                effect.Parameters["xEmissive"].SetValue(emission.ToVector4());

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Game1.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                        c.indexBuffer.IndexCount / 3);
                }
                effect.Parameters["xWorld"].SetValue(oldWorldMat);
            }
        }

    }
}
