using Microsoft.Xna.Framework;
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
        
        static float walkingSpeed = 12f;
        static float flyingSpeed = walkingSpeed * 2.5f;
        static float speed = 12f;

        float height = 3.5f;
        float width = 1.5f;
        private bool flying = false;
        Light torchLight;

        float mouseSensitivty = .002f;
        private float jumpVelocity = 6.5f;

        public Player()
        {
            
            Vector3 cameraPosition = new Vector3(7,5,7);
            playerActor = new Actor(new AABB(height, width, width), true, 1f);

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
                    movement += (Vector3.UnitY * speed);
                    //velocity += ;
                }
                else if (playerActor.isOnGround())
                {
                    playerActor.addVelocity(Vector3.UnitY * jumpVelocity);
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

            if (justHit(Keys.Up, newState))
            {
                Rendering.adjustAmbientLight(+0.1f);
            }

            if (justHit(Keys.Down, newState))
            {
                Rendering.adjustAmbientLight(-0.1f);
            }

            if (justHit(Keys.LeftControl, newState))
            {
                flying = !flying;
                speed = (flying ? flyingSpeed : walkingSpeed);
                playerActor.setGravityFactor(flying ? 0f : 1f);
                playerActor.setCollides(flying ? false : true);

            }

            if (flying)
            {
                playerActor.setVelocity(Vector3.Zero);
            }

            

            playerActor.setInstantaneousMovement(movement);

            MouseState newMouseState = Mouse.GetState();

            if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
            {
                FireBall spell = new FireBall(getCameraLocation(), Vector3.Normalize(getFacingVector()) * 45f);
                result.Add(new SpawnAction(spell));
            }

            if (newMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
            {
                StickingLight spell = new StickingLight(getCameraLocation(), Vector3.Normalize(getFacingVector()) * 45f);
                result.Add(new SpawnAction(spell));
            }

            if (mouseEngaged)
            {
                leftRightRot += -(newMouseState.X - oldMouseState.X) * mouseSensitivty;
                upDownRot +=    -(newMouseState.Y - oldMouseState.Y) * mouseSensitivty;
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
            Vector3 backup = getFacingVector() * 0;
            return Matrix.CreateLookAt(
              getCameraLocation() + backup, getCameraLocation() + getFacingVector() + backup, cameraUpVector);
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
            //playerActor.drawFirstPass(effect, Matrix.Identity);
        }

        internal GameObject getActor()
        {
            return playerActor;
        }
    }
}
