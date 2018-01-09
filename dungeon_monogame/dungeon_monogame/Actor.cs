using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class Actor : GameObject
    {
        protected AABB aabb;
        protected Vector3 velocity;
        protected Vector3 previousVelocity;
        private Vector3 instantaneuousMovement;
        private bool currentlyOnGround = false;
        protected float gravityFactor = 1.0f;
        public float bounciness { get; set; }




        public Actor() { }

        public Actor(AABB aabb)
        {
            this.aabb = aabb;

        }

        public void physicsUpdate(GameTime time, ChunkManager space)
        {
            previousVelocity = velocity;
            float deltaTime = time.ElapsedGameTime.Milliseconds / 1000f;
            currentlyOnGround = false;
            this.velocity.Y -= Globals.G * deltaTime * gravityFactor;
            Vector3 desiredMovement = deltaTime * this.velocity +instantaneuousMovement * deltaTime;

            setLocation(collide(space, desiredMovement));
            
        }


        private Vector3 collide(ChunkManager space, Vector3 desiredMovement)
        {

            Vector3 currentLocation = getLocation();
            Vector3 desiredFinalLocation = getLocation() + desiredMovement;
            Vector3 finalLocation = new Vector3();
            currentlyOnGround = false;
            foreach (Globals.axes axis in Globals.allAxes)
            {
                Vector3 prospectiveLocation = desiredFinalLocation * Globals.unit(axis);
                Tuple<Globals.axes, Globals.axes> otherAxes = Globals.OtherAxes(axis);
                Globals.axes otherAxis1 = otherAxes.Item1;
                Globals.axes otherAxis2 = otherAxes.Item2;
                prospectiveLocation += currentLocation * Globals.unit(otherAxis1);
                prospectiveLocation += currentLocation * Globals.unit(otherAxis2);
                bool collided = false;
                if (scale.X < 1)
                {

                }
                for (float a1 = aabb.axisMin(otherAxis1, scale); a1 <= aabb.axisMax(otherAxis1, scale); a1 += .5f)
                {
                    for (float a2 = aabb.axisMin(otherAxis2, scale); a2 <= aabb.axisMax(otherAxis2, scale); a2 += .5f)
                    {

                        if (Globals.along(desiredMovement, axis) < 0)
                        {
                            
                            Vector3 queryLocation = Globals.unit(axis) * aabb.axisMin(axis, scale) +
                                                    Globals.unit(otherAxis1) * a1 +
                                                    Globals.unit(otherAxis2) * a2;

                            queryLocation += prospectiveLocation;
                            if (space.solid(new IntLoc(queryLocation)))
                            {
                                Vector3 move = Globals.unit(axis) * (1 - (Globals.along(queryLocation, axis) - (float)Math.Floor(Globals.along(queryLocation, axis)))) + Globals.unit(axis) * .001f;
                                prospectiveLocation += move;
                                desiredMovement -= Globals.along(desiredMovement, axis) * Globals.unit(axis);
                                if (axis == Globals.axes.y)
                                {
                                    currentlyOnGround = true;
                                }
                                velocity -= (1 + bounciness) * Globals.unit(axis) * Globals.along(velocity, axis);

                                collided = true;

                            }
                        }
                        else if (Globals.along(desiredMovement, axis) > 0)
                        {
                            Vector3 queryLocation = Globals.unit(axis) * aabb.axisMax(axis, scale) +
                                                    Globals.unit(otherAxis1) * a1 +
                                                    Globals.unit(otherAxis2) * a2 +
                                                    prospectiveLocation;
                            if (space.solid(new IntLoc(queryLocation)))
                            {
                                float moveAmount = (Globals.along(queryLocation, axis) - (float)Math.Floor(Globals.along(queryLocation, axis))) + .001f;
                                prospectiveLocation -= Globals.unit(axis) * moveAmount;
                                desiredMovement -= Globals.along(desiredMovement, axis) * Globals.unit(axis);
                                velocity -= (1 + bounciness) * Globals.unit(axis) * Globals.along(velocity, axis);
                                collided = true;
                            }
                        }
                    }
                }
                if (collided)
                {
                    onCollision();
                }
                finalLocation += prospectiveLocation * Globals.unit(axis);
            }
            return finalLocation;
        }

        public void setVelocity(Vector3 v)
        {
            velocity = v;
        }

        internal Vector3 getVelocity()
        {
            return velocity;
        }

        public void addVelocity(Vector3 v)
        {
            velocity += v;
        }

        public void setInstantaneousMovement(Vector3 v)
        {
            instantaneuousMovement = v;
        }

        public bool isOnGround()
        {
            return currentlyOnGround;
        }

        protected override List<Action> update()
        {
            List<Action> result = new List<Action>();
            result.Add(new RequestPhysicsUpdate(this));
            return result;
        }

        protected virtual void onCollision() 
        {
        }

        public AABB getAabb()
        {
            // to be truly correct, this method would have to take the scale of the parent into account
            return new AABB(aabb.height * scale.Y, aabb.xWidth * scale.X, aabb.zWidth * scale.Z);
        }




    }
}
