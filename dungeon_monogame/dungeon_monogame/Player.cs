using dungeon_monogame.WorldGeneration;
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
    abstract class InputHandler
    {
        protected KeyboardState oldKeyboardState;
        protected MouseState oldMouseState = Mouse.GetState();

        public abstract List<Action> handleInput();

        protected bool justHit(Keys key, KeyboardState newState)
        {
            return oldKeyboardState.IsKeyDown(key) && !newState.IsKeyDown(key);
        }

    }


    class Player : InputHandler
    {
        
        Vector3 cameraLookAlongVector = -Vector3.UnitZ;



        Vector3 cameraUpVector = Vector3.UnitY;
        bool mouseEngaged = true;

        Actor playerActor;
        private float upDownRot = -2;
        private float leftRightRot = 0;
        
        static float walkingSpeed = 12f * WorldGenParamaters.gameScale;
        static float flyingSpeed = walkingSpeed * 2.5f;
        static float speed = 12f;

        float height = 3.5f * WorldGenParamaters.gameScale;
        float width = 1.5f * WorldGenParamaters.gameScale;
        private bool flying = true;
        Light torchLight;

        float mouseSensitivty = .002f;
        private float jumpVelocity = 6.5f;

        public Player()
        {
            
            Vector3 cameraPosition = new Vector3(7,5,7);
            playerActor = new Actor(new AABB(height, width, width), true, 1f);

            playerActor.setLocation(cameraPosition);
            setMouseToCenter();
            var torch = new GameObjectModel(MagicaVoxel.ChunkManagerFromResource(@"dungeon_monogame.Content.voxel_models.torch.vox"), new Vector3(.2f, 1.4f, -.3f), Vector3.One * .03f);
            //torchLight = new Light(1f, Color.LightGoldenrodYellow);
            torchLight = new FireLight();
            torchLight.setLocation(new Vector3(3, 8, 3));
            // torch.addChild(torchLight); // behaves badly wrt shadows
            playerActor.addChild(torch);
            //playerActor.addChild(new Light());
            playerActor.addTag(ObjectTag.Player);
            doFlyingLogic();
        }

        public void setMouseToCenter()
        {
            Mouse.SetPosition(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
        }

        public override List<Action> handleInput()
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
                }
                else if (playerActor.isOnGround())
                {
                    playerActor.addVelocity(Vector3.UnitY * jumpVelocity);
                    
                }
            }
            if (newState.IsKeyDown(Keys.LeftShift))
            {
                if (flying)
                {
                    movement -= Vector3.UnitY * speed;
                }
            }
            if (justHit(GlobalSettings.OpenMainMenuKey, newState))
            {
                //mouseEngaged = !mouseEngaged;
                result.Add(new ToggleMainMenu());
            }

            if (newState.IsKeyDown(Keys.Up))
            {
                result.Add(new AdjustAmbientLightAction(+GlobalSettings.AmbientLightContinuousAdjustmentIncrement));
            }

            if (newState.IsKeyDown(Keys.Down))
            {
                result.Add(new AdjustAmbientLightAction(-GlobalSettings.AmbientLightContinuousAdjustmentIncrement));
            }

            if (justHit(Keys.LeftControl, newState))
            {
                flying = !flying;


            }
            doFlyingLogic();

            if(justHit(Keys.M, newState))
            {
                result.Add(new SpawnAction(new Slime(getCameraLocation())));
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

            playerActor.setRotation(Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(leftRightRot)));
            return result;
        }

        private void doFlyingLogic()
        {
            speed = (flying ? flyingSpeed : walkingSpeed);
            playerActor.setGravityFactor(flying ? 0f : 1f);
            playerActor.setCollides(flying ? false : true);
            if (flying)
            {
                playerActor.setVelocity(Vector3.Zero);
            }
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

        internal GameObject getActor()
        {
            return playerActor;
        }
    }


}
