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
        Vector3 cameraPosition = new Vector3(0, .5f, 10);
        Vector3 cameraLookAlongVector = -Vector3.UnitZ;
        Vector3 cameraUpVector = Vector3.UnitY;
        MouseState oldMouseState = Mouse.GetState();

        public void handleInput()
        {
            KeyboardState newState = Keyboard.GetState();
            // cameraLookAtVector = Vector3.Transform(cameraLookAtVector, Matrix.CreateRotationX(.01f));
            // Is the SPACE key down?
            float speed = .05f;
            if (newState.IsKeyDown(Keys.W))
            {
                cameraPosition += cameraLookAlongVector * speed;
            }
            if (newState.IsKeyDown(Keys.S))
            {
                cameraPosition -= cameraLookAlongVector * speed;
            }
            if (newState.IsKeyDown(Keys.A))
            {
                cameraPosition += Vector3.Transform(Vector3.Normalize(cameraLookAlongVector * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * speed;
            }
            if (newState.IsKeyDown(Keys.D))
            {
                cameraPosition -= Vector3.Transform(Vector3.Normalize(cameraLookAlongVector * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * speed;
            }
            if (newState.IsKeyDown(Keys.Q))
            {
                cameraLookAlongVector = Vector3.Transform(cameraLookAlongVector, Matrix.CreateRotationY(.01f));
            }
            if (newState.IsKeyDown(Keys.E))
            {
                cameraLookAlongVector = Vector3.Transform(cameraLookAlongVector, Matrix.CreateRotationY(-.01f));
            }
            if (newState.IsKeyDown(Keys.Space))
            {
                cameraPosition += Vector3.UnitY * speed;
            }
            if (newState.IsKeyDown(Keys.LeftShift))
            {
                cameraPosition -= Vector3.UnitY * speed;
            }
        }

        public Matrix getViewMatrix()
        {
            return Matrix.CreateLookAt(
              cameraPosition, cameraPosition + cameraLookAlongVector, cameraUpVector);
        }
    }
}
