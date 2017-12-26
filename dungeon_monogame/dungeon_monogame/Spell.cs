﻿using Microsoft.Xna.Framework;
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
            : base(new AABB(.1f, .1f, .1f))
        {
            setLocation(_location);
            this.addVelocity(velocity);
            gravityFactor = 0f;
            addChild(new Light(.9f, Color.White));

            GameObject model = new GameObject(MagicaVoxel.Read(@"spell.vox"), new Vector3(-.5f-.5f-.5f) * -.0f, Vector3.One * .1f);
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
                result.Add(new SpawnAction(new Flash(getLocation() - .1f * Vector3.Normalize(getVelocity()))));
                for (int i = 0; i < 25; i++)
                {
                    result.Add(new SpawnAction(new Spark(getLocation() - .1f * Vector3.Normalize(getVelocity()),
                        new Vector3((float)Globals.random.NextDouble() - .5f, (float)Globals.random.NextDouble() - .5f, (float)Globals.random.NextDouble() - .5f) * 10f)));

                }

            }
            return result;
        }
    }

    class Spark : Actor
    {
        bool dead = false;

        public Spark(Vector3 _location, Vector3 velocity)
           
        {
            setLocation(_location);
            this.addVelocity(velocity);
            bounciness = .8f;
            gravityFactor = .6f;
            addChild(new Light(.3f, Color.LightGreen));
            ChunkManager model = MagicaVoxel.Read(@"spell.vox");
            Vector3 offset = model.getCenter();
            this.scale = Vector3.One * .1f;
            GameObject obj = new GameObject(model, -offset, Vector3.One);
            this.aabb = model.getAaabbFromModelExtents();
            addChild(obj);
        }

        protected override void onCollision()
        {
            if (Globals.random.NextDouble() < 0)
            {
                dead = true;
            }
        }

        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();
            result.Add(new RequestPhysicsUpdate(this));
            if (dead)
            {
                 result.Add(new DissapearAction(this));
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
