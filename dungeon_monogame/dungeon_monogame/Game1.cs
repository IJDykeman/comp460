using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace dungeon_monogame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Vector3 cameraPosition = new Vector3(0, .5f, 10);
        Vector3 cameraLookAlongVector = -Vector3.UnitZ;
        Vector3 cameraUpVector = Vector3.UnitY;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            handleInput();
            base.Update(gameTime);
        }

        void handleInput()
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
                cameraPosition +=  Vector3.Transform(Vector3.Normalize(cameraLookAlongVector * new Vector3(1,0,1)), Matrix.CreateRotationY(MathHelper.PiOver2)) * speed;
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
        }

        void simpleDrawTest()
        {
            Chunk c = new Chunk();
            c.remesh();
            BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);

            effect.View = Matrix.CreateLookAt(
                cameraPosition, cameraPosition + cameraLookAlongVector, cameraUpVector);

            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
            float nearClipPlane = 1;
            float farClipPlane = 200;

            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);


            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                graphics.GraphicsDevice.DrawUserPrimitives(
                    // We’ll be rendering two trinalges
                    PrimitiveType.TriangleList,
                    // The array of verts that we want to render
                    c.vertices,
                    // The offset, which is 0 since we want to start 
                    // at the beginning of the floorVerts array
                    0,
                    // The number of triangles to draw
                    c.vertices.Length / 3);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            simpleDrawTest();

            base.Draw(gameTime);
        }
    }
}
