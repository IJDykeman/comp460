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

        protected override List<Action> onCollision()
        {
            List<Action> result = new List<Action>();

            result.Add(new DissapearAction(this));
            result.Add(new SpawnAction(new Flash(getLocation() - .1f * Vector3.Normalize(previousVelocity))));
            for (int i = 0; i < Globals.random.Next(15, 30); i++)
            {
                result.Add(new SpawnAction(new Spark(getLocation() - .2f * Vector3.Normalize(previousVelocity)
                    ,
                    Globals.randomVectorOnUnitSphere() * Globals.standardGaussianSample() * 8f,
                    lightColor

                    )));

            }
            return result;

        }

        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();
            result.Add(new RequestPhysicsUpdate(this));
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

        protected override List<Action> onCollision()
        {
            hasCollided = true;
            lantern.setIntensity(2.0f);
            lantern.setStability(MagicLantern.MEDIUM_STABILITY);
            return new List<Action>();

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

        protected override List<Action> onCollision()
        {
            if (Globals.random.NextDouble() < dissapear_on_collide_probability)
            {
                dead = true;
            }
            return new List<Action>();
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
        protected Color lightColor;
        protected MagicLantern light;
        GameObject model;

        public FireBall(Vector3 _location, Vector3 velocity)
            : base(new AABB(1.1f, 1.1f, 1.1f))
        {
            setLocation(_location);
            this.addVelocity(velocity);
            gravityFactor = 1f;

            lightColor = Globals.ColorFromHSV(23, .8, .9);

            
            light = new MagicLantern(1.6f, lightColor);
            light.setStability(MagicLantern.LOW_STABILITY);
            light.setFlickerIntensity(MagicLantern.HIGH_FLICKER_INTENSITY);
            addChild(light);

            //model = new GameObject(MagicaVoxel.ChunkManagerFromVox(@"spell.vox"), new Vector3(), Vector3.One * .1f);
            //addChild(model);
           // model.setEmissiveness(lightColor);
        }

        protected override List<Action> onCollision()
        {
            List<Action> result = new List<Action>();
            result.Add(new DissapearAction(this));
            result.Add(new SpawnAction(new Flash(getLocation() - .1f * Vector3.Normalize(previousVelocity))));
            for (int i = 0; i < Globals.random.Next(100, 150); i++)
            {
                result.Add(new SpawnAction(new FireParticle(getLocation()
                    ,
                    Globals.randomVectorOnUnitSphere() * Globals.standardGaussianSample() * 20f ,
                    lightColor

                    )));

            }
            return result;
        }

        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();
            result.Add(new RequestPhysicsUpdate(this));
            //model.scale = Vector3.One * .1f * light.getIntensity();
            result.Add(new SpawnAction(new FireParticle(getLocation(), Globals.randomVectorOnUnitSphere() * Globals.standardGaussianSample() * 3f,lightColor

    )));

            return result;
        }
    }

    class FireParticle : Actor
    {
        bool dead = false;

        MagicLantern light;
        GameObject shape;


        float temperature = 1f;
        float mass;
        float age = 0;

        public FireParticle(Vector3 _location, Vector3 velocity, Color color)

        {
            aabb = new AABB(.001f, .001f, .001f);
            setLocation(_location);
            this.addVelocity(velocity);
            gravityFactor = -1;
            light = new MagicLantern(0f, color);
            light.setStability(MagicLantern.HIGH_STABILITY);
            light.setFlickerIntensity(MagicLantern.MEDIUM_FLICKER_INTENSITY);
            mass = (float)Globals.random.NextDouble();
            if (Globals.random.NextDouble() > .7f)
            {
                addChild(light);
            }
            ChunkManager model = MagicaVoxel.ChunkManagerFromVox(@"smoke.vox");
            Vector3 offset = model.getCenter();
            
            shape = new GameObject(model, offset*0, Vector3.One);
            shape.scale = Vector3.One * 6 * (.08f + (float)(Globals.random.NextDouble() - .5) * .03f);
            shape.setEmissiveness(color);
            addChild(shape);
            mass = (float)Globals.random.NextDouble();
            collidesWithWorld = true;
            age = 2;
        }

        protected override List<Action> update()
        {
            age += .3f + (1 - mass);


            List<Action> result = new List<Action>();
            
            result.Add(new RequestPhysicsUpdate(this));


            //temperature *= (float)Globals.random.NextDouble() * (1-decay_factor) + decay_factor;
            float clock = age / 20;
            temperature =  ((float)(Math.Exp(clock / Math.Exp(clock))) -1)/1.2f;
            bounciness = temperature;

            light.setIntensity((float)(temperature ) * 4);
            shape.scale = Vector3.One * (float)(temperature);
            gravityFactor = (-temperature*4 + mass);
            this.velocity -= .0001f * velocity.LengthSquared() / mass * velocity;

            Color emit = Globals.ColorFromHSV(23, (1 - temperature), light.getIntensity() * light.getIntensity() );
            shape.setEmissiveness(emit);// light.getIntensity() * light.getIntensity()/3008));
            light.updateWithChildren();

            if (temperature < .01f && age > 10)
            {
                result.Add(new DissapearAction(this));
            }

            return result;
        }
    }
}
