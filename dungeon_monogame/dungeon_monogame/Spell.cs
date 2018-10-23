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
        protected bool dead = false;
        protected Color lightColor;

        public Spell(Vector3 _location, Vector3 velocity)
            : base(new AABB(.1f, .1f, .1f))
        {
            setLocation(_location);
            this.addVelocity(velocity);
            gravityFactor = 0f;

            lightColor = Globals.ColorFromHSV(Globals.random.NextDouble() * 255, 1, 255);

            addChild(new Light(.6f, lightColor));

            GameObject model = new GameObject(MagicaVoxel.ChunkManagerFromVox(@"spell.vox"), new Vector3(-.5f-.5f-.5f) * -.0f, Vector3.One * .1f);
            addChild(model);
            model.setEmissiveness(lightColor);
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
                result.Add(new SpawnAction(new Flash(getLocation() - .1f * Vector3.Normalize(previousVelocity))));
                for (int i = 0; i < Globals.random.Next(15,30); i++)
                {
                    result.Add(new SpawnAction(new Spark(getLocation() - .2f * Vector3.Normalize(previousVelocity)
                        ,
                        Globals.randomVectorOnUnitSphere() * Globals.standardGaussianSample() * 8f,
                        lightColor

                        )));

                }

            }
            return result;
        }
    }



    class StickingLight : Actor
    {
        protected bool hasCollided = false;
        protected Color lightColor;
        protected MagicLantern lantern;


        public StickingLight(Vector3 _location, Vector3 velocity)
            : base(new AABB(1.0f, 1.0f, 1.0f))
        {
            setLocation(_location);
            this.addVelocity(velocity);
            gravityFactor = 0f;

            lightColor = Globals.ColorFromHSV(Globals.random.NextDouble() * 150 + 40, 1, 1);

            lantern = new MagicLantern(0.45f, lightColor);
            lantern.setStability(MagicLantern.LOW_STABILITY);
            addChild(lantern);

            GameObject model = new GameObject(MagicaVoxel.ChunkManagerFromVox(@"spell.vox"), new Vector3(-1.5f -1.5f -1.5f) * -.0f, Vector3.One * .1f);
            addChild(model);
            model.setEmissiveness(lightColor);
        }

        protected override void onCollision()
        {
            hasCollided = true;
            lantern.setIntensity(2.0f);
            lantern.setStability(MagicLantern.MEDIUM_STABILITY);

        }



        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();

            if (hasCollided)
            {
                this.velocity *= 0;
                if (Globals.random.NextDouble() > .995f)
                {
                    result.Add(new SpawnAction(new Spark(getLocation() - .2f * Vector3.Normalize(previousVelocity)
                        ,
                        Globals.randomVectorOnUnitSphere() * Globals.standardGaussianSample() * 2f,
                        lightColor)));
                }

            }
            else
            {
                result.Add(new RequestPhysicsUpdate(this));
            }
            return result;
        }
    }

    class Spark : Actor
    {
        bool dead = false;
        float dissapear_on_collide_probability = .2f;
        Light light;
        float scaleFactor = .99f;

        public Spark(Vector3 _location, Vector3 velocity, Color color)
           
        {
            setLocation(_location);
            this.addVelocity(velocity);
            bounciness = .7f + (float)(Globals.random.NextDouble() -.5) *.2f;
            gravityFactor = .6f + (float)(Globals.random.NextDouble() -.5) *.1f;
            light = new Light(.4f + (float)(Globals.random.NextDouble() - .5) * .1f, color);
            addChild(light);
            ChunkManager model = MagicaVoxel.ChunkManagerFromVox(@"spell.vox");
            Vector3 offset = model.getCenter();
            this.scale = Vector3.One * (.08f + (float)(Globals.random.NextDouble() -.5) *.03f);
            GameObject obj = new GameObject(model, -offset, Vector3.One);
            obj.setEmissiveness(color);
            this.aabb = model.getAaabbFromModelExtents();
            addChild(obj);
        }

        protected override void onCollision()
        {
            if (Globals.random.NextDouble() < dissapear_on_collide_probability)
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
                scaleFactor = .90f;

            }
            this.scale *= scaleFactor;
            light.setIntensity(light.getIntensity() * .99995f);
            if (this.scale.Length() < .05f)
            {
               // light.setIntensity(light.getIntensity() * .9995f);

            }
            if (this.scale.Length() < .005f)
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

    class FireBall : Actor
    {
        protected bool dead = false;
        protected Color lightColor;

        public FireBall(Vector3 _location, Vector3 velocity)
            : base(new AABB(.1f, .1f, .1f))
        {
            setLocation(_location);
            this.addVelocity(velocity);
            gravityFactor = 0f;

            lightColor = Globals.ColorFromHSV(23, .8, .9);

            addChild(new Light(.6f, lightColor));

            GameObject model = new GameObject(MagicaVoxel.ChunkManagerFromVox(@"spell.vox"), new Vector3(-.5f - .5f - .5f) * -.0f, Vector3.One * .1f);
            addChild(model);
            model.setEmissiveness(lightColor);
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
                result.Add(new SpawnAction(new Flash(getLocation() - .1f * Vector3.Normalize(previousVelocity))));
                for (int i = 0; i < Globals.random.Next(100, 150); i++)
                {
                    result.Add(new SpawnAction(new FireParticle(getLocation() - 5f * Vector3.Normalize(previousVelocity)
                        ,
                        Globals.randomVectorOnUnitSphere() * Globals.standardGaussianSample() * 5f,
                        lightColor

                        )));

                }

            }
            return result;
        }
    }

    class FireParticle : Actor
    {
        bool dead = false;
        
        Light light;
        GameObject shape;


        float temperature = 1f;

        public FireParticle(Vector3 _location, Vector3 velocity, Color color)

        {
            setLocation(_location);
            this.addVelocity(velocity);
            bounciness = 1f;
            gravityFactor = -1;
            light = new Light(0f, color);

            if (Globals.random.NextDouble() > .9f)
            {
                addChild(light);
            }
            ChunkManager model = MagicaVoxel.ChunkManagerFromVox(@"spell.vox");
            Vector3 offset = model.getCenter();
            
            shape = new GameObject(model, -offset, Vector3.One);
            shape.scale = Vector3.One * 6 * (.08f + (float)(Globals.random.NextDouble() - .5) * .03f);
            shape.setEmissiveness(color);
            this.aabb = model.getAaabbFromModelExtents();
            addChild(shape);
            velocity += Vector3.UnitY * 10f;
        }

        protected override void onCollision()
        {

        }

        protected override List<Action> update()
        {
            
            List<Action> result = new List<Action>();
            
            result.Add(new RequestPhysicsUpdate(this));

            temperature *= (float)Globals.random.NextDouble() * .08f + .92f;
            light.setIntensity((float)Math.Sqrt(temperature ));
            shape.scale = Vector3.One * temperature;
            gravityFactor = -1;// temperature;
            this.velocity *= .98f;
            shape.setEmissiveness(Globals.ColorFromHSV(23, temperature, temperature));

            if (shape.scale.Length() < .005f)
            {
                result.Add(new DissapearAction(this));
            }
            return result;
        }
    }
}
