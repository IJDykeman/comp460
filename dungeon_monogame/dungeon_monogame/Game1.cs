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
        public static GraphicsDeviceManager graphics;

        private RenderTarget2D colorRT;
        private RenderTarget2D normalRT;
        private RenderTarget2D depthRT, positionRT;
        int backBufferWidth, backBufferHeight;

        SpriteBatch spriteBatch;

        Effect createGBufferEffect, renderSceneEffect;
        ChunkManager chunkManager;
        Player player;
        Vector2 halfPixel;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            backBufferHeight = graphics.PreferredBackBufferHeight;
            backBufferWidth  = graphics.PreferredBackBufferWidth;
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
            createGBufferEffect = Content.Load<Effect>("DeferredRender");
            renderSceneEffect = Content.Load<Effect>("RenderSceneFromGBuffer");
            // TODO: use this.Content to load your game content here

            chunkManager = new ChunkManager();
            player = new Player();
            colorRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            depthRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            positionRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth, backBufferHeight, false, SurfaceFormat.Vector4, DepthFormat.None);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            handleInput();

            player.update(gameTime.ElapsedGameTime.Milliseconds / 1000f, chunkManager);
            //player.update(0, chunkManager);

            base.Update(gameTime);
            halfPixel.X = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferHeight;

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
            
            //graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            graphics.GraphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
            // render options
            //graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            render("RenderGBuffer");

            Texture2D diffuseTex = (Texture2D)colorRT;
            Texture2D norm = (Texture2D)normalRT;
            Texture2D depth = (Texture2D)depthRT;
            Texture2D position = (Texture2D)positionRT;
            GraphicsDevice.SetRenderTargets(null);


            using (SpriteBatch sprite = new SpriteBatch(graphics.GraphicsDevice))
            {
                //effect.CurrentTechnique = effect.Techniques["ClearGBuffer"];
                
                sprite.Begin(depthStencilState:DepthStencilState.Default);
                //sprite.Begin(0, BlendState.Opaque, null, null, null, effect);
                sprite.Draw(diffuseTex, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
                sprite.Draw(norm, new Vector2(400, 0), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
                sprite.Draw(depth, new Vector2(0, 200), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
                sprite.End();
            }
            renderSceneEffect.CurrentTechnique = renderSceneEffect.Techniques["PointLightTechnique"];

            //set all parameters
            renderSceneEffect.Parameters["lightIntensity"].SetValue(1f);
            renderSceneEffect.Parameters["colorMap"].SetValue(diffuseTex);
            renderSceneEffect.Parameters["normalMap"].SetValue(norm);
            renderSceneEffect.Parameters["depthMap"].SetValue(depth);
            //renderSceneEffect.Parameters["positionMap"].SetValue(position);
            renderSceneEffect.Parameters["lightDirection"].SetValue(new Vector3(1,-2,3));
           // renderSceneEffect.Parameters["Color"].SetValue(new Vector3(0,.5f, .5f));
            //renderSceneEffect.Parameters["lightDirection"].SetValue(new Vector3(1, -2, 3));
            renderSceneEffect.Parameters["lightRadius"].SetValue(20f);
            renderSceneEffect.Parameters["lightPosition"].SetValue(player.getCameraLocation());
            
            //renderSceneEffect.Parameters["cameraPosition"].SetValue(player.getCameraLocation());
            renderSceneEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(player.getViewMatrix() * projection()));
            //renderSceneEffect.Parameters["InvertProjection"].SetValue(Matrix.Invert(projection()));

            //renderSceneEffect.Parameters["InvertView"].SetValue((player.getViewMatrix()));
            
            renderSceneEffect.Parameters["halfPixel"].SetValue(halfPixel);
            GraphicsDevice.BlendState = BlendState.Opaque;

            new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
            GraphicsDevice.BlendState = BlendState.Additive;
            renderSceneEffect.Parameters["lightPosition"].SetValue(player.getCameraLocation() + Vector3.UnitX * 5);
            new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
            renderSceneEffect.Parameters["lightPosition"].SetValue(player.getCameraLocation() + Vector3.UnitX * 10);
            new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
            renderSceneEffect.Parameters["lightPosition"].SetValue(player.getCameraLocation() + Vector3.UnitX * 40);
            renderSceneEffect.Parameters["lightRadius"].SetValue(30f);

            new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
            renderSceneEffect.Parameters["lightIntensity"].SetValue(.1f);
            renderSceneEffect.CurrentTechnique = renderSceneEffect.Techniques["DirectionalLightTechnique"];

            new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);

            //render("Diffuse");

            
        }

        public Matrix projection()
        {
            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
            float nearClipPlane = .1f;
            float farClipPlane = 100;

            return Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
        }

        private void render(string shader)
        {



            //effect.Projection = Matrix.CreatePerspectiveFieldOfView(
            //   fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            createGBufferEffect.CurrentTechnique = createGBufferEffect.Techniques[shader];


            createGBufferEffect.Parameters["xProjection"].SetValue(projection());
            createGBufferEffect.Parameters["xView"].SetValue(player.getViewMatrix());
            createGBufferEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            //createGBufferEffect.Parameters["xAmbient"].SetValue(.3f);
            //createGBufferEffect.Parameters["xLightDirection"].SetValue(Vector3.Normalize(new Vector3(-4,2,-1)));
            //effect.Parameters["xOpacity"].SetValue(1.0f);
            if (createGBufferEffect.CurrentTechnique == null)
            {

            }
            chunkManager.draw(createGBufferEffect);
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



internal sealed class QuadRenderer
{


    private VertexPositionTexture[] triangles;
    private short[] indexData = new short[] { 0, 1, 2, 2, 3, 0 };

    public QuadRenderer()
    {
        this.triangles = new VertexPositionTexture[] 
                     { 
                       new VertexPositionTexture(new Vector3(1, -1, 0), 
                                                 new Vector2(1, 1)),
                       new VertexPositionTexture(new Vector3(-1, -1, 0), 
                                                 new Vector2(0, 1)),
                       new VertexPositionTexture(new Vector3(-1, 1, 0), 
                                                 new Vector2(0, 0)),
                       new VertexPositionTexture(new Vector3(1, 1, 0), 
                                                 new Vector2(1, 0)) 
                     };
    }

    public void Render(Effect effect, GraphicsDevice device)
    {
        foreach (EffectPass p in effect.CurrentTechnique.Passes)
            p.Apply();

        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                           this.triangles, 0, 4,
                                           this.indexData, 0, 2);
    }


}