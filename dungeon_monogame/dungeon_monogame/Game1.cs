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

        Effect effect;
        Chunk c;
        Player player = new Player();

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
        //Content.RootDirectory = "Content";
            // use C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools pipeline tool for this
            Content.Load<Texture2D>("dfg");
            effect = Content.Load<Effect>("Effect");
            // TODO: use this.Content to load your game content here

            c = new Chunk();
            c.remesh();
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
            player.handleInput();
        }

        void simpleDrawTest()
        {
            
            //BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);
     
            //effect.View = Matrix.CreateLookAt(
            //    cameraPosition, cameraPosition + cameraLookAlongVector, cameraUpVector);

            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
            float nearClipPlane = .1f;
            float farClipPlane = 100;

            //effect.Projection = Matrix.CreatePerspectiveFieldOfView(
            //   fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            effect.CurrentTechnique = effect.Techniques["Colored"];
            effect.Parameters["xProjection"].SetValue(Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane));
            effect.Parameters["xView"].SetValue(player.getViewMatrix());
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xAmbient"].SetValue(.3f);
            effect.Parameters["xLightDirection"].SetValue(Vector3.Normalize(new Vector3(-4,-2,-1)));
            effect.Parameters["xEnableLighting"].SetValue(true);
            //effect.Parameters["xOpacity"].SetValue(1.0f);
            

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                /*graphics.GraphicsDevice.DrawUserPrimitives(
                 //We’ll be rendering two trinalges
                    PrimitiveType.TriangleList,
                 //The array of verts that we want to render
                    c.vertices,
                 //The offset, which is 0 since we want to start 
                 //at the beginning of the floorVerts array
                    0,
                 //The number of triangles to draw
                    c.vertices.Length / 3);*/
                
                VertexBuffer vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, VertexPostitionColorPaintNormal.VertexDeclaration, c.vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData<VertexPostitionColorPaintNormal>(c.vertices);
                short[] indices = new short[c.vertices.Length];
                for (short i =0; i < c.vertices.Length; i++)
                {
                    indices[i] = i;
                }

                IndexBuffer indexBuffer = new IndexBuffer(graphics.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices);
                graphics.GraphicsDevice.Indices = indexBuffer;
                graphics.GraphicsDevice.SetVertexBuffer(vertexBuffer);
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    indexBuffer.IndexCount / 3);
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
