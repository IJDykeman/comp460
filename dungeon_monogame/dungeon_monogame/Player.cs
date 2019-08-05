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

        float scale = WorldGenParamaters.gameScale;

        static readonly float baseWalkingSpeed = 20f;
        static readonly float baseFlyingSpeed = baseWalkingSpeed * 2.5f;
        static readonly float baseGravityFactor = 1.0f;
        float gravityFactor;

        float baseHeight = 3.5f;
        float baseWidth = 1.5f;
        private bool flying = true;
        Light torchLight;

        float mouseSensitivty = .002f;

        internal void setScale(float _scale)
        {
            scale = _scale;
            playerActor.setAabb(getAABB());
        }

        private float baseJumpVelocity = 6.5f;

        public Player()
        {
            
            playerActor = new Actor(getAABB(), true, 1f);

            Vector3 startingPosition = new Vector3(7, 5, 7);
            playerActor.setLocation(startingPosition);
            setMouseToCenter();
            var torch = new GameObjectModel(MagicaVoxel.ChunkManagerFromResource(@"dungeon_monogame.Content.voxel_models.torch.vox"), new Vector3(.2f, 1.4f, -.3f), Vector3.One * .03f);
            torchLight = new FireLight();
            torchLight.setLocation(new Vector3(3, 8, 3));
            playerActor.addTag(ObjectTag.Player);
            updatePlayerActorState();
        }

        float getSpeed()
        {
            if (flying){
                return baseFlyingSpeed * scale;
            }
            else {
                return baseWalkingSpeed * scale;
            }
        }

        AABB getAABB()
        {
            return new AABB(baseHeight * scale, baseWidth * scale, baseWidth * scale);
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
                movement -= Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.Pi)) * getSpeed();

               // movement += getFacingVector() * speed;
            }
            if (newState.IsKeyDown(Keys.S))
            {
                //movement -= getFacingVector() * speed;
                movement -= Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(0)) * getSpeed();

            }
            if (newState.IsKeyDown(Keys.A))
            {
                movement += Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * getSpeed();
            }
            if (newState.IsKeyDown(Keys.D))
            {
                movement -= Vector3.Transform(Vector3.Normalize(getFacingVector() * new Vector3(1, 0, 1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * getSpeed();
            }


            if (newState.IsKeyDown(Keys.Space))
            {
                if (flying)
                {
                    movement += (Vector3.UnitY * getSpeed());
                }
                else if (playerActor.isOnGround())
                {
                    playerActor.addVelocity(Vector3.UnitY * baseJumpVelocity * scale);
                    
                }
            }
            if (newState.IsKeyDown(Keys.LeftShift))
            {
                if (flying)
                {
                    movement -= Vector3.UnitY * getSpeed();
                }
            }
            if (justHit(GlobalSettings.OpenMainMenuKey, newState))
            {
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

            if(justHit(Keys.M, newState))
            {
                result.Add(new SpawnAction(new Slime(getCameraLocation())));
            }


            playerActor.setInstantaneousMovement(movement);

            MouseState newMouseState = Mouse.GetState();

            if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
            {
                Actor spell = new Spell(getCameraLocation(), Vector3.Normalize(getFacingVector()) * 45f);//new FireBall(getCameraLocation(), Vector3.Normalize(getFacingVector()) * 45f);
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

        internal void setLocationToOrigin()
        {
            playerActor.setLocation(Vector3.Zero);
        }

        public void update()
        {
            updatePlayerActorState();
        }

        private void updatePlayerActorState()
        {
            playerActor.setGravityFactor(flying ? 0f : 1f * scale);
            playerActor.setCollides(flying ? false : true);
            if (flying)
            {
                playerActor.setVelocity(Vector3.Zero);
            }
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

        public Vector3 getWorldGenerationPerspectiveLocation()
        {
            if (oldKeyboardState.IsKeyDown(Keys.V))
            {
                return new Vector3();
            }
            else
            {
                return getCameraLocation();
            }
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
