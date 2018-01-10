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

        private static RenderTarget2D colorRT;
        private static RenderTarget2D normalRT, emissiveRT;
        private static RenderTarget2D depthRT, positionRT;
        public static Texture2D vignette;
        static int backBufferWidth, backBufferHeight;


        static Effect createGBufferEffect, renderSceneEffect;
        static Vector2 halfPixel;

        public static void LoadContent(ContentManager Content, GraphicsDeviceManager graphics)
        {
            backBufferHeight = graphics.PreferredBackBufferHeight;
            backBufferWidth  = graphics.PreferredBackBufferWidth;
            createGBufferEffect = Content.Load<Effect>("DeferredRender");
            renderSceneEffect = Content.Load<Effect>("RenderSceneFromGBuffer");
            vignette = Content.Load<Texture2D>("vignette");

            int scale_factor = 1;
            colorRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth * scale_factor, backBufferHeight * scale_factor, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth * scale_factor, backBufferHeight * scale_factor, false, SurfaceFormat.Color, DepthFormat.None);
            depthRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth * scale_factor, backBufferHeight * scale_factor, false, SurfaceFormat.Single, DepthFormat.None);
            positionRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth * scale_factor, backBufferHeight * scale_factor, false, SurfaceFormat.Vector4, DepthFormat.None);
            emissiveRT = new RenderTarget2D(graphics.GraphicsDevice, backBufferWidth * scale_factor, backBufferHeight * scale_factor, false, SurfaceFormat.Color, DepthFormat.None);

            halfPixel.X = 0.5f / (float)graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

        }

        public static void renderWorld(GraphicsDeviceManager graphics, GameObject landscape, Player player)
        {
            using (SpriteBatch sprite = new SpriteBatch(graphics.GraphicsDevice))
            {
                sprite.Begin(depthStencilState: DepthStencilState.Default);
                sprite.End();
            }
            GraphicsDevice GraphicsDevice = graphics.GraphicsDevice;
            //BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);

            //effect.View = Matrix.CreateLookAt(
            //    cameraPosition, cameraPosition + cameraLookAlongVector, cameraUpVector);

            //graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT, emissiveRT);
            // render options
            //graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);


            createGBufferEffect.CurrentTechnique = createGBufferEffect.Techniques["RenderGBuffer"];
            createGBufferEffect.Parameters["xProjection"].SetValue(projection(graphics));
            createGBufferEffect.Parameters["xView"].SetValue(player.getViewMatrix());
            createGBufferEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            // worldChunkManager.draw(createGBufferEffect, Matrix.Identity);
            landscape.drawFirstPass(createGBufferEffect, Matrix.Identity);
            //player.draw(createGBufferEffect);

            Texture2D diffuseTex = (Texture2D)colorRT;
            Texture2D norm = (Texture2D)normalRT;
            Texture2D depth = (Texture2D)depthRT;
            Texture2D position = (Texture2D)positionRT;
            Texture2D emissive = (Texture2D)emissiveRT;
            GraphicsDevice.SetRenderTargets(null);


            using (SpriteBatch sprite = new SpriteBatch(graphics.GraphicsDevice))
            {
                //effect.CurrentTechnique = effect.Techniques["ClearGBuffer"];
                sprite.Begin(depthStencilState: DepthStencilState.Default);
                //sprite.Begin(0, BlendState.Opaque, null, null, null, effect);
                sprite.Draw(diffuseTex, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
                sprite.Draw(norm, new Vector2(500, 0), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
                sprite.Draw(depth, new Vector2(0, 300), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
                sprite.Draw(emissive, new Vector2(500, 300), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
                sprite.End();
            }
            if (true)
            {
                renderSceneEffect.CurrentTechnique = renderSceneEffect.Techniques["PointLightTechnique"];

                //set all parameters
                renderSceneEffect.Parameters["lightIntensity"].SetValue(1f);
                renderSceneEffect.Parameters["colorMap"].SetValue(diffuseTex);
                renderSceneEffect.Parameters["normalMap"].SetValue(norm);
                renderSceneEffect.Parameters["depthMap"].SetValue(depth);
                renderSceneEffect.Parameters["emissiveMap"].SetValue(emissive);
                //renderSceneEffect.Parameters["positionMap"].SetValue(position);
                renderSceneEffect.Parameters["lightDirection"].SetValue(new Vector3(1, -2, 3));
                // renderSceneEffect.Parameters["Color"].SetValue(new Vector3(0,.5f, .5f));
                //renderSceneEffect.Parameters["lightDirection"].SetValue(new Vector3(1, -2, 3));
                renderSceneEffect.Parameters["lightRadius"].SetValue(0f);
                renderSceneEffect.Parameters["lightPosition"].SetValue(player.getCameraLocation());
                //renderSceneEffect.Parameters["cameraPosition"].SetValue(player.getCameraLocation());
                renderSceneEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(player.getViewMatrix() * projection(graphics)));
                renderSceneEffect.Parameters["halfPixel"].SetValue(halfPixel);
                GraphicsDevice.BlendState = BlendState.Opaque;

                new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
                GraphicsDevice.BlendState = BlendState.Additive;
                landscape.drawDeferredPass(renderSceneEffect, Matrix.Identity, GraphicsDevice);


                new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
                renderSceneEffect.Parameters["lightIntensity"].SetValue(.5f);
                renderSceneEffect.CurrentTechnique = renderSceneEffect.Techniques["DirectionalLightTechnique"];
                //GraphicsDevice.BlendState = BlendState.Opaque;

                new QuadRenderer().Render(renderSceneEffect, GraphicsDevice);
                using (SpriteBatch sprite = new SpriteBatch(graphics.GraphicsDevice))
                {
                    sprite.Begin(depthStencilState: DepthStencilState.None);
                    sprite.Draw(vignette, new Vector2(0, 0), null, Color.White * .6f, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 1);
                    sprite.End();
                }
            }


        }


        static Matrix projection(GraphicsDeviceManager graphics)
        {
            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = MathHelper.ToRadians(70f) ;
            float nearClipPlane = .1f;
            float farClipPlane = 200;

            return Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
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