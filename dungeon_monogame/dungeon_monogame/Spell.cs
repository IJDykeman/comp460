using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class Spell : Actor
    {
        bool dead = false;

        public Spell(Vector3 _location, Vector3 velocity)
            : base(new AABB(.4f, .4f, .4f))
        {
            setLocation(_location);
            this.addVelocity(velocity);
            gravityFactor = 0f;
            addChild(new Light(.9f, Color.LightGreen));

            GameObject model = new GameObject(MagicaVoxel.Read(@"spell.vox"), new Vector3(-.5f-.5f-.5f) * .1f, Vector3.One * .1f);
            addChild(model);
        }

        protected override void onCollision()
        {
            dead = true;
        }

        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();
            result.Add(new RequestPhysicsUpdate(this));
            if (dead)
            {
                result.Add(new DissapearAction(this));
                result.Add(new SpawnAction(new Flash(getLocation() - .5f * Vector3.Normalize(getVelocity()))));
            }
            return result;
        }
    }

    class Flash : Actor
    {
        Light light;
        public Flash(Vector3 _location)
            : base(new AABB(.4f, .4f, .4f))
        {
            setLocation(_location);
            gravityFactor = 0f;
            light = new Light(3, Color.BlueViolet);
            addChild(light);

        }

        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();
            light.setIntensity(light.getIntensity() * .95f - .1f);
            if (light.getIntensity() < .01f)
            {
                result.Add(new DissapearAction(this));
            }
            return result;
        }
    }
}
