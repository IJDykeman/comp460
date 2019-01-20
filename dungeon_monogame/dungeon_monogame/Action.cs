using dungeon_monogame.WorldGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    abstract class Action
    {


        public virtual void actOnWorld(World world, GameTime dt) { }
        public virtual void actOnGame(Game1 game1) { }
    }


    class ToggleMainMenu : Action
    {
        public override void actOnGame(Game1 game1) {
            game1.toggleMainMenu();
        }
    }

    

    class ExportModelAction : Action
    {
        public override void actOnWorld(World world, GameTime dt)
        {
            world.exportModel();
        }
    }

    class SetPlayerScaleAction : Action
    {
        float scale;
        public SetPlayerScaleAction(float _scale)
        {
            scale = _scale;
        }
        public override void actOnGame(Game1 game1)
        {
            game1.setPlayerScale(scale);
        }
    }


    class ExitApplicationAction : Action
    {
        public override void actOnGame(Game1 game1)
        {
            game1.Exit();
        }
    }

    class RequestTilesetLoad : Action
    {
        bool exampleBased;

        public RequestTilesetLoad(bool _exampleBased)
        {
            exampleBased = _exampleBased;
        }

        public override void actOnGame(Game1 game1)
        {
            game1.loadNewTileset(exampleBased);
        }
    }

    class RequestWorldResetWithTileset : Action
    {
        TileSet tileset;

        public RequestWorldResetWithTileset(TileSet _tileset)
        {
            tileset = _tileset;
        }

        public override void actOnGame(Game1 game1)
        {
            game1.setTileset(tileset);
        }
    }

    class RequestPhysicsUpdate : Action
    {
        private Actor actor;

        public RequestPhysicsUpdate(Actor a)
        {
            actor = a;
        }


        public override void actOnWorld(World world, GameTime dt)
        {
            ChunkManager manager = world.getMap().getChunkManager();
            foreach (Action act in actor.physicsUpdate(dt, manager))
            {
                act.actOnWorld(world, dt);
            }
        }
    }

    class MoveTowardTaggedActorAction : Action
    {
        private Monster monster;
        private ObjectTag tag;
        float minDist, maxDist;

        public MoveTowardTaggedActorAction(Monster a, ObjectTag _tag, float _minDist, float _maxDist)
        {
            monster = a;
            tag = _tag;
            minDist = _minDist;
            maxDist = _maxDist;
        }


        public override void actOnWorld(World world, GameTime dt)
        {
            List<GameObject> tagged = world.getChildrenWithTag(tag);
            if (tagged.Count > 0)
            {
                float distance = (monster.getLocation() - tagged[0].getLocation()).Length();
                Vector3 targetCenter = tagged[0].getLocation();
                Vector3 targetDelta = targetCenter - monster.getLocation();
                float dist = targetDelta.Length();
                targetDelta.Normalize();
                targetDelta *= Math.Max(0, dist - minDist);
                if (minDist < distance && distance < maxDist)
                {
                    monster.setTargetLocation(targetDelta + monster.getLocation());
                }
            }
        }

    }

    class AofDamage : Action
    {
        Vector3 center;
        float radius;
        ObjectTag tag;
        float damage;

        public AofDamage(Vector3 _center, ObjectTag _tag, float _radius, float _damage)
        {
            center = _center;
            radius = _radius;
            tag = _tag;
            damage = _damage;
        }


        public override void actOnWorld(World world, GameTime dt)
        {
            foreach (Actor a in world.getChildrenWithTag(tag))
            {
                if((a.getLocation() - center).Length() < radius)
                {
                    a.takeDamage(damage);
                }
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


        public override void actOnWorld(World world, GameTime dt)
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


        public override void actOnWorld(World world, GameTime dt)
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


        public override void actOnWorld(World world, GameTime dt)
        {
            List<GameObject> burnedObjects = world.recursiveGetWithinSphere(center, radius);
            foreach(GameObject obj in burnedObjects)
            {
                obj.burn();
            }
        }
    }

    internal class AdjustAmbientLightAction : Action
    {
        private float v;

        public AdjustAmbientLightAction(float v)
        {
            this.v = v;
        }

        public override void actOnWorld(World world, GameTime dt)
        {
            world.changeTotalAmbientPower(v);

        }

    }
}


