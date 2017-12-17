using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class ChunkManager
    {
        Dictionary<IntLoc, Chunk> chunks;

        public ChunkManager()
        {
            chunks = new Dictionary<IntLoc, Chunk>();
            for (int i = 0; i < 1000; i++)
            {
                int s = 25;
                //var loc = new IntLoc(Globals.random.Next(-s, s), Globals.random.Next(-s, s), Globals.random.Next(-s, s));
                var loc = new IntLoc(Globals.random.Next(0, s), Globals.random.Next(0, s), Globals.random.Next(0, s));
                set(loc , new Block(1, new Color(Globals.random.Next(0, 256), Globals.random.Next(0, 256), Globals.random.Next(0, 256))));
            }
            for (int x = -25; x<25; x++){
                for (int z = -25; z < 25; z++)
                {
                    for (int y = -2; y < 2; y++)
                    {
                        set(new IntLoc(x,y,z), new Block(1, new Color(Globals.random.Next(0, 256), Globals.random.Next(0, 256), Globals.random.Next(0, 256))));
                    }
                }

            }
            remeshAll();
        }

        void remeshAll()
        {
            foreach (Chunk c in chunks.Values)
            {
                c.remesh();
            }
        }

        public bool solid(IntLoc l)
        {
            if (!withinChunk(l))
            {
                return false;
            }
            return chunks[locToChunkLoc(l)].solid(l % Chunk.chunkWidth);
        }

        public Block getBlock(IntLoc l)
        {
            return chunks[locToChunkLoc(l)].getBlock(l % Chunk.chunkWidth);
        }

        public void set(IntLoc loc, Block val)
        {
            if (!withinChunk(loc))
            {
                chunks[locToChunkLoc(loc)] = new Chunk();
            }
            IntLoc chunkLoc = locToChunkLoc(loc);
            Chunk c = chunks[chunkLoc];
            IntLoc setLoc = loc % Chunk.chunkWidth;
            c.setBlock(setLoc, val);
        }

        private IntLoc locToChunkLoc(IntLoc l)
        {
            return l - (l % Chunk.chunkWidth);
        }

        private bool withinChunk(IntLoc loc)
        {
            return chunks.ContainsKey(locToChunkLoc(loc));
        }

        public void draw(Effect effect)
        {
            foreach (KeyValuePair<IntLoc, Chunk> p in chunks)
            {
                
                Matrix oldWorldMat = effect.Parameters["xWorld"].GetValueMatrix();
                Chunk c = p.Value;
                if (c.empty())
                {
                    continue;
                }
                IntLoc loc = p.Key;
                Game1.graphics.GraphicsDevice.Indices = c.indexBuffer;
                Game1.graphics.GraphicsDevice.SetVertexBuffer(c.vertexBuffer);
                Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                effect.Parameters["xWorld"].SetValue(Matrix.Multiply(oldWorldMat, Matrix.CreateTranslation(loc.toVector3())));
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
