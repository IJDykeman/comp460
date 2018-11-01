using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class Creature : Actor
    {
        protected float health = 1;
        protected bool onFire = false;
        protected Stopwatch lifeStopwatch;

        public override void takeDamage(float damage)
        {
            health -= damage;
        }

        protected float getAgeSeconds()
        {
            return lifeStopwatch.ElapsedMilliseconds / 1000.0f;
        }

        public float getSpeed()
        {
            return 5f;
        }

        public override void burn()
        {
            addChild(new Fire(this));
            onFire = true;
        }

        protected List<Action> basicCreatureUpdate()
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
    }
}
