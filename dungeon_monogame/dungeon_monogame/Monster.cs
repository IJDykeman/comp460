using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class Monster : Actor
    {
        float health = 1;
        bool onFire = false;
        public Monster(Vector3 location)
        {
            setLocation(location);
            ChunkManager model = MagicaVoxel.ChunkManagerFromVox("goblin1.vox");
            scale = Vector3.One * .08f;
            Vector3 offset = model.getCenter();
            GameObject obj = new GameObject(model, -offset, Vector3.One);
            this.aabb = model.getAaabbFromModelExtents();
            addChild(obj);
        }

        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();
            result.Add(new RequestPhysicsUpdate(this));
            if (onFire)
            {
                health -= 1.0f / 250;
            }
            if (health <= 0)
            {
                result.Add(new DissapearAction(this));
            }
            return result;
        }

        public override void burn()
        {
            addChild(new Fire(this));
            onFire = true;
        }

        public override void takeDamage(int damage)
        {
            health -= damage;
        }
    }
}
