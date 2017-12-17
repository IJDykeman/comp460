using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class Actor
    {
        private AABB aabb;
        private Vector3 velocity;
        private Vector3 instantaneuousMovement;
        private bool currentlyOnGround = false;

        public Actor(AABB aabb)
        {
            this.aabb = aabb;
        }

        public void physicsUpdate(float deltaTime, ChunkManager space)
        {
            currentlyOnGround = false;
            this.velocity.Y -= Globals.G * deltaTime;
            Vector3 desiredMovement = deltaTime * this.velocity +instantaneuousMovement * deltaTime;
            

            Vector3 currentLocation = aabb.getCenter();
            Vector3 desiredFinalLocation = aabb.getCenter() + desiredMovement;
            Vector3 finalLocation = new Vector3();
            foreach (Globals.axes axis in Globals.allAxes)
            {
                Vector3 prospectiveLocation = desiredFinalLocation * Globals.unit(axis);
                Tuple<Globals.axes, Globals.axes> otherAxes = Globals.OtherAxes(axis);
                Globals.axes otherAxis1 = otherAxes.Item1;
                Globals.axes otherAxis2 = otherAxes.Item2;
                prospectiveLocation += currentLocation * Globals.unit(otherAxis1);
                prospectiveLocation += currentLocation * Globals.unit(otherAxis2);
                for (float a1 = aabb.axisMin(otherAxis1); a1 <= aabb.axisMax(otherAxis1); a1 += .5f)
                {
                    for (float a2 = aabb.axisMin(otherAxis2); a2 <= aabb.axisMax(otherAxis2); a2 += .5f)
                    {

                        if (Globals.along(desiredMovement, axis) < 0)
                        {
                            Vector3 queryLocation = Globals.unit(axis) * aabb.axisMin(axis) +
                                                    Globals.unit(otherAxis1) * a1 +
                                                    Globals.unit(otherAxis2) * a2 +
                                                    prospectiveLocation;
                            if (space.solid(new IntLoc(queryLocation)))
                            {
                                Vector3 move = Globals.unit(axis) * (1-(Globals.along(queryLocation, axis) - (float)Math.Floor(Globals.along(queryLocation, axis)))) + Globals.unit(axis) * .001f;
                                prospectiveLocation += move;
                                desiredMovement -= Globals.along(desiredMovement, axis) * Globals.unit(axis);
                                if (axis == Globals.axes.y)
                                {
                                    currentlyOnGround = true;
                                }
                                velocity -= Globals.unit(axis) * Globals.along(velocity, axis);
                                
                            }
                        }
                        else if (Globals.along(desiredMovement, axis) > 0)
                        {
                            Vector3 queryLocation = Globals.unit(axis) * aabb.axisMax(axis) +
                                                    Globals.unit(otherAxis1) * a1 +
                                                    Globals.unit(otherAxis2) * a2 +
                                                    prospectiveLocation;
                            if (space.solid(new IntLoc(queryLocation)))
                            {
                                float moveAmount = (Globals.along(queryLocation, axis) - (float)Math.Floor(Globals.along(queryLocation, axis))) + .001f;
                                prospectiveLocation -= Globals.unit(axis) * moveAmount;
                                desiredMovement -= Globals.along(desiredMovement, axis) * Globals.unit(axis);
                                velocity -= Globals.unit(axis) * Globals.along(velocity, axis);


                            }
                        }
                    }
                }
                finalLocation += prospectiveLocation * Globals.unit(axis);
            }
            aabb.setCenter(finalLocation);
            
        }

        public void setVelocity(Vector3 v)
        {
            velocity = v;
        }

        internal Vector3 getCenterLocation()
        {
            return aabb.getCenter();
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
    }
}
