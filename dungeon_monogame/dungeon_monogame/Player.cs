﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class Player
    {
        
        Vector3 cameraLookAlongVector = -Vector3.UnitZ;
        Vector3 cameraUpVector = Vector3.UnitY;
        MouseState oldMouseState = Mouse.GetState();
        bool mouseEngaged = true;

        Actor playerActor;
        private KeyboardState oldKeyboardState;
        private float upDownRot = -2;
        private float leftRightRot = 0;
        float speed = 10f;
        float height = 3.2f;
        float width = 1.5f;
        private bool flying = false;
        Light torchLight;

        public Player()
        {
            Vector3 cameraPosition = new Vector3(7,5,7);
            playerActor = new Actor(new AABB(height, width, width));

            playerActor.setLocation(cameraPosition);
            Mouse.SetPosition(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
            //GameObject sword = new GameObject(MagicaVoxel.Read(@"simple_sword.vox"), new Vector3(1,20,0), Vector3.One * .1f);
            //playerActor.addChild(sword);
            var torch = new GameObject(MagicaVoxel.ChunkManagerFromVox(@"torch.vox"), new Vector3(.2f, 1.4f, -.3f), Vector3.One * .03f);
            //torchLight = new Light(1f, Color.LightGoldenrodYellow);
            torchLight = new FireLight();
            torchLight.setLocation(new Vector3(3, 8, 3));
            torch.addChild(torchLight);
            //torch.addChild(new GameObject(MagicaVoxel.Read(@"torch.vox"), new Vector3(-3, 0, 0), Vector3.One * .5f));
            playerActor.addChild(torch);
            //playerActor.addChild(new Light());
        }

        public List<Action> handleInput()
        {
            List<Action> result = new List<Action>();
            KeyboardState newState = Keyboard.GetState();
            // cameraLookAtVector = Vector3.Transform(cameraLookAtVector, Matrix.CreateRotationX(.01f));
            // Is the SPACE key down?
            
            Vector3 movement = playerActor.getVelocity();
            if (newState.IsKeyDown(Keys.W))
            {
                movement -= Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.Pi)) * speed;

               // movement += getFacingVector() * speed;
            }
            if (newState.IsKeyDown(Keys.S))
            {
                //movement -= getFacingVector() * speed;
                movement -= Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(0)) * speed;

            }
            if (newState.IsKeyDown(Keys.A))
            {
                movement += Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * speed;
            }
            if (newState.IsKeyDown(Keys.D))
            {
                movement -= Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * speed;
            }

            if (newState.IsKeyDown(Keys.Space))
            {
                if (flying)
                {
                    playerActor.addVelocity(Vector3.UnitY * speed);
                    //velocity += ;
                }
                else if (playerActor.isOnGround())
                {
                    playerActor.addVelocity(Vector3.UnitY * 5.5f);
                    //Console.WriteLine("jump");
                    //Console.WriteLine(playerActor.getVelocity().Y);
                    
                }
            }
            if (newState.IsKeyDown(Keys.LeftShift))
            {
                if (flying)
                {
                    movement -= Vector3.UnitY * speed;
                }
            }
            if (justHit(Keys.Tab, newState))
            {
                mouseEngaged = !mouseEngaged;
            }

            if (!flying && (movement * new Vector3(1,0,1)).Length() !=0)
            {
               // movement.Y = 0;
               // movement.Normalize();
               // movement *= speed;
                if (movement.Y != 0)
                {

                }
            }

            

            playerActor.setInstantaneousMovement(movement);

            MouseState newMouseState = Mouse.GetState();

            if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
            {
                Spell spell = new Spell(getCameraLocation(), Vector3.Normalize(getFacingVector()) * 35f);
                result.Add(new SpawnAction(spell));
            } 

            if (mouseEngaged)
            {
                leftRightRot += -(newMouseState.X - oldMouseState.X) * .005f;
                upDownRot +=    -(newMouseState.Y - oldMouseState.Y) * .005f;
                upDownRot = MathHelper.Clamp(upDownRot, (float)(-4.5f), (float)(-1.6f));
                Mouse.SetPosition(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
            }
            oldMouseState = Mouse.GetState();
            oldKeyboardState = Keyboard.GetState();

            //torchLight.setIntensity(MathHelper.Min(torchLight.getIntensity(), 1.3f));
            //torchLight.setIntensity(MathHelper.Max(torchLight.getIntensity(), .7f));
            //torchLight.setIntensity(torchLight.getIntensity() +(float) (Globals.random.NextDouble()-.5f) * .06f);

            playerActor.setRotation(Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(leftRightRot)));
            return result;
        }

        private bool justHit(Keys key, KeyboardState newState)
        {
            return oldKeyboardState.IsKeyDown(key) && !newState.IsKeyDown(key);
        }

        public Matrix getViewMatrix()
        {
            return Matrix.CreateLookAt(
              getCameraLocation(), getCameraLocation() + getFacingVector(), cameraUpVector);
        }

        public Vector3 getCameraLocation()
        {
            return playerActor.getAabb().axisMax(Globals.axes.y, Vector3.One) * Vector3.UnitY - Vector3.UnitY * .2f + playerActor.getLocation();
        }

        private Vector3 getFacingVector()
        {
            return Vector3.Transform(Vector3.UnitZ, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot));
        }


        internal void draw(Effect effect)
        {
            playerActor.drawFirstPass(effect, Matrix.Identity);
        }

        internal GameObject getActor()
        {
            return playerActor;
        }
    }
}
