using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    interface Action
    {


        void act(GameObject world, GameTime dt);
    }
    
    class RequestPhysicsUpdate : Action
    {
        private Actor actor;

        public RequestPhysicsUpdate(Actor a)
        {
            actor = a;
        }
        

        public void act(GameObject world, GameTime dt)
        {
            foreach(Action act in actor.physicsUpdate(dt, world.getChunkSpace()))
            {
                act.act(world, dt);
            }
        }
    }

    class SpawnAction : Action
    {
        private Actor actor;

        public SpawnAction(Actor toSpawn)
        {
            actor = toSpawn;
        }


        public void act(GameObject world, GameTime dt)
        {
            world.addChild(actor);
        }
    }
    
    class DissapearAction : Action
    {
        private Actor actor;

        public DissapearAction(Actor toRemove)
        {
            actor = toRemove;
        }


        public void act(GameObject world, GameTime dt)
        {
            world.recursiveRemove(actor);
        }
    }

    class EngulfInFlameAction : Action
    {
        Vector3 center;
        float radius;

        public EngulfInFlameAction(Vector3 _center, float _radius)
        {
            center = _center;
            radius = _radius;
        }


        public void act(GameObject world, GameTime dt)
        {
            List<GameObject> burnedObjects = world.recursiveGetWithinSphere(center, radius);
            foreach(GameObject obj in burnedObjects)
            {
                obj.burn();
            }

        }
    }
}


