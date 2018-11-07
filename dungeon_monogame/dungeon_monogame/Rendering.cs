using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    static class Rendering
    {

        static RenderTargets cameraTargets;
        public static Texture2D vignette;
        static int backBufferWidth, backBufferHeight;
        static Color bgColor = Color.Black;
        static int scale_factor = 1;


        static Effect createGBufferEffect, renderSceneEffect;
        static Vector2 halfPixel;
        private static float ambientLightLevel = 0.0f;

        static GraphicsDeviceManager graphics;
        static GameObjectModel worldRootObject;

        public static Texture2D debugTexture;

        public static void LoadContent(ContentManager Content, GraphicsDeviceManager _graphics, GameObjectModel _worldRoot)
        {
            graphics = _graphics;
            worldRootObject = _worldRoot;
            backBufferHeight = graphics.PreferredBackBufferHeight;
            backBufferWidth  = graphics.PreferredBackBufferWidth;
            createGBufferEffect = Content.Load<Effect>("DeferredRender");
            renderSceneEffect = Content.Load<Effect>("RenderSceneFromGBuffer");
            vignette = Content.Load<Texture2D>("vignette");

            cameraTargets = new RenderTargets(backBufferWidth, backBufferHeight, graphics.GraphicsDevice);



            halfPixel.X = 0.5f / (float)graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

        }

        public static void adjustAmbientLight(float delta)
        {
            ambientLightLevel += delta;
            ambientLightLevel = Math.Min(ambientLightLevel, 1.4f);
            ambientLightLevel = Math.Max(ambientLightLevel, 0.0f);
        }

        public static void renderWorld(Player player)
        {
            GraphicsDevice GraphicsDevice = graphics.GraphicsDevice;
            using (SpriteBatch sprite = new SpriteBatch(GraphicsDevice))
            {
                sprite.Begin(depthStencilState: DepthStencilState.Default);
                sprite.End();
            }

            

            GBuffer buffer = DrawGBuffer(player.getViewMatrix(), projection(graphics), cameraTargets, a=>true);

            worldRootObject.drawAlternateGBufferFirstPass(Matrix.Identity);

            //GBuffer LightPerspectiveGBuffer = renderDirectionalLightFirstPass(graphics, rootObject, player, GraphicsDevice);
            setupParams(player, buffer.diffuseTex, buffer.norm, buffer.depth, buffer.emissive);

            renderSceneEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(player.getViewMatrix() * projection(graphics)));
            renderSceneEffect.Parameters["halfPixel"].SetValue(halfPixel);
            new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
            GraphicsDevice.BlendState = BlendState.Additive;
            worldRootObject.drawSecondPass(renderSceneEffect, Matrix.Identity, GraphicsDevice);

            // render light coming directly from emmissize objects
            renderSceneEffect.CurrentTechnique = renderSceneEffect.Techniques["EmmissiveMaterialsTechnique"];
            new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
            
            //renderDirectionalLightSecondPass(graphics, rootObject, player, GraphicsDevice, LightPerspectiveGBuffer);

            using (SpriteBatch sprite = new SpriteBatch(graphics.GraphicsDevice))
            {
                sprite.Begin(depthStencilState: DepthStencilState.None);
                sprite.Draw(vignette, new Vector2(0, 0), null, Color.Black * .7f, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 1);

                float squareScale = .2f;
                int w = 800;
                int h = w / 5 * 3;
                if (debugTexture != null)
                {
                    sprite.Draw(debugTexture, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), squareScale, SpriteEffects.None, 1);
                }
                //sprite.Draw(LightPerspectiveGBuffer.diffuseTex, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), squareScale, SpriteEffects.None, 1);
                //sprite.Draw(LightPerspectiveGBuffer.norm, new Vector2(w, 0), null, Color.White, 0, new Vector2(0, 0), squareScale, SpriteEffects.None, 1);
                //sprite.Draw(LightPerspectiveGBuffer.depth, new Vector2(0, h), null, Color.White, 0, new Vector2(0, 0), squareScale, SpriteEffects.None, 1);
                //sprite.Draw(LightPerspectiveGBuffer.emissive, new Vector2(w, h), null, Color.White, 0, new Vector2(0, 0), squareScale, SpriteEffects.None, 1);

                sprite.End();
            }

        
            using (SpriteBatch sprite = new SpriteBatch(graphics.GraphicsDevice))
            {
                sprite.Begin(depthStencilState: DepthStencilState.Default);

                sprite.End();
            }


        }

        public static GBuffer DrawGBuffer(Matrix viewMatrix, Matrix projectionMatrix, RenderTargets targets, Predicate<GameObject> predicate)
        {

            GraphicsDevice device = graphics.GraphicsDevice;
            targets.setTargets(device);
            // render options
            graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            device.RasterizerState = RasterizerState.CullNone;
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, bgColor, 1.0f, 0);

            createGBufferEffect.CurrentTechnique = createGBufferEffect.Techniques["RenderGBuffer"];
            createGBufferEffect.Parameters["xProjection"].SetValue(projectionMatrix);
            createGBufferEffect.Parameters["xView"].SetValue(viewMatrix);
            createGBufferEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            worldRootObject.drawFirstPass(createGBufferEffect, Matrix.Identity,  new BoundingFrustum(viewMatrix * projectionMatrix), predicate );

            GBuffer buffer = targets.getTextures();
            device.SetRenderTargets(null);
            return buffer;
        }


        private static void setupParams(Player player, Texture2D diffuseTex, Texture2D norm, Texture2D depth, Texture2D emissive)
        {
            //render global directional light
            renderSceneEffect.CurrentTechnique = renderSceneEffect.Techniques["PointLightTechnique"];
            //set all parameters
            renderSceneEffect.Parameters["lightIntensity"].SetValue(1f);
            renderSceneEffect.Parameters["colorMap"].SetValue(diffuseTex);
            renderSceneEffect.Parameters["normalMap"].SetValue(norm);
            renderSceneEffect.Parameters["depthMap"].SetValue(depth);
            renderSceneEffect.Parameters["emissiveMap"].SetValue(emissive);
            renderSceneEffect.Parameters["lightRadius"].SetValue(0f);
            renderSceneEffect.Parameters["lightPosition"].SetValue(player.getCameraLocation());
        }

        public static Matrix projection(GraphicsDeviceManager graphics)
        {
            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = MathHelper.ToRadians(80f) ;
            float nearClipPlane = .1f;
            float farClipPlane = 1500;
            //return Matrix.CreateOrthographic(15, 15, .001f, 20);
            return Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
        }

        internal static RenderTargets getEmptyTargets(int backBufferWidth1, int backBufferWidth2)
        {
            return new RenderTargets(backBufferWidth1, backBufferWidth2, graphics.GraphicsDevice);
        }
    }
}

struct GBuffer
{
    public Texture2D diffuseTex;
    public Texture2D norm;
    public Texture2D depth;
    public Texture2D position;
    public Texture2D emissive;
    public GBuffer(Texture2D _diffuseTex, Texture2D _norm, Texture2D _depth, Texture2D _position, Texture2D _emissive)
    {
        diffuseTex = _diffuseTex;
        norm = _norm;
        depth = _depth;
        position = _position;
        emissive = _emissive;
    }
}

class RenderTargets
{
    public RenderTarget2D colorRT;
    public RenderTarget2D normalRT;
    public RenderTarget2D depthRT;
    public RenderTarget2D positionRT;
    public RenderTarget2D emissiveRT;

    public RenderTargets(int width, int height, GraphicsDevice device)
    {
        colorRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        normalRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None);
        depthRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);
        positionRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Vector4, DepthFormat.None);
        emissiveRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None);
    }

    public void setTargets(GraphicsDevice device)
    {
        device.SetRenderTargets(colorRT, normalRT, depthRT, emissiveRT);
    }

    public GBuffer getTextures()
    {
        Texture2D diffuseTex = ((Texture2D)colorRT);
        Texture2D norm = (Texture2D)normalRT;
        Texture2D depth = (Texture2D)depthRT;
        Texture2D position = (Texture2D)positionRT;
        Texture2D emissive = (Texture2D)emissiveRT;
        return new GBuffer(diffuseTex, norm, depth, position, emissive);
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