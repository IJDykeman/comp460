using Microsoft.Xna.Framework;
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

        public Player()
        {
            Vector3 cameraPosition = new Vector3(0, 10, 0);
            playerActor = new Actor(new AABB(cameraPosition, .8f, .8f, .8f));
            Mouse.SetPosition(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
        }

        public void handleInput()
        {
            KeyboardState newState = Keyboard.GetState();
            // cameraLookAtVector = Vector3.Transform(cameraLookAtVector, Matrix.CreateRotationX(.01f));
            // Is the SPACE key down?
            float speed = 5f;
            Vector3 velocity = playerActor.getVelocity();
            if (newState.IsKeyDown(Keys.W))
            {
                velocity += getFacingVector() * speed;
            }
            if (newState.IsKeyDown(Keys.S))
            {
                velocity -= getFacingVector() * speed;
            }
            if (newState.IsKeyDown(Keys.A))
            {
                velocity += Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * speed;
            }
            if (newState.IsKeyDown(Keys.D))
            {
                velocity -= Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * speed;
            }

            if (newState.IsKeyDown(Keys.Space))
            {
                velocity += Vector3.UnitY * speed;
            }
            if (newState.IsKeyDown(Keys.LeftShift))
            {
                velocity -= Vector3.UnitY * speed;
            }
            if (justHit(Keys.Tab, newState))
            {
                mouseEngaged = !mouseEngaged;
            }

            playerActor.setVelocity(velocity);

            MouseState newMouseState = Mouse.GetState();
            if (mouseEngaged)
            {
                leftRightRot += -(newMouseState.X - oldMouseState.X) * .005f;
                upDownRot +=    -(newMouseState.Y - oldMouseState.Y) * .005f;
                upDownRot = MathHelper.Clamp(upDownRot, (float)(-4.5f), (float)(-1.6f));
                Console.WriteLine(upDownRot);
                Mouse.SetPosition(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
            }
            oldMouseState = Mouse.GetState();
            oldKeyboardState = Keyboard.GetState();
        }

        private bool justHit(Keys key, KeyboardState newState)
        {
            return oldKeyboardState.IsKeyDown(key) && !newState.IsKeyDown(key);
        }

        public void update(float dt, ChunkManager chunkManager) {
            playerActor.physicsUpdate(dt, chunkManager);
        }

        public Matrix getViewMatrix()
        {
            return Matrix.CreateLookAt(
              playerActor.getCenterLocation(), playerActor.getCenterLocation() + getFacingVector(), cameraUpVector);
        }

        private Vector3 getFacingVector()
        {
            return Vector3.Transform(Vector3.UnitZ, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot));
        }

    }
}
