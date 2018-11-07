using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace dungeon_monogame
{
    internal class GameObjectModel : GameObject
    {
        private ChunkManager chunkManager;
        private Vector3 vector31;
        private Vector3 vector32;

        public GameObjectModel(ChunkManager chunkManager, Vector3 vector31, Vector3 vector32) : base(vector31, vector32)
        {
            this.chunkManager = chunkManager;
        }

        public override void drawFirstPass(Effect effect, Matrix transform, BoundingFrustum frustum, Predicate<GameObject> whetherDraw)
        {

            transform = getTransform(ref transform);
            if (whetherDraw(this))
            {
                chunkManager.draw(effect, transform, this.emissiveness, frustum);
            }

            foreach (GameObject child in children)
            {
                child.drawFirstPass(effect, transform, frustum, whetherDraw);
            }
        }
    }
}