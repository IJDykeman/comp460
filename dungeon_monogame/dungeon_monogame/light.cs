using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class Light : GameObject
    {
        float lightIntesity = .6f;
        protected Color color = Color.LightYellow;

        public Light() { }

        public Light(float _intensity, Color _color)
        {
            lightIntesity = _intensity;
            color = _color;
        }

        public override void drawSecondPass(Effect effect, Matrix transform, GraphicsDevice device)
        {
            transform = getTransform(ref transform);
            effect.CurrentTechnique = effect.Techniques["PointLightTechnique"];
            effect.Parameters["lightIntensity"].SetValue(lightIntesity);
            effect.Parameters["lightRadius"].SetValue(20f * lightIntesity);
            effect.Parameters["lightColor"].SetValue(color.ToVector4());
            Vector3 position = Vector3.Transform(getLocation(), transform);
            effect.Parameters["lightPosition"].SetValue(position);
            new QuadRenderer().Render(effect, device);

            foreach (GameObject child in children)
            {
                child.drawSecondPass(effect, transform, device);
            }
        }

        public float getIntensity()
        {
            return lightIntesity;
        }


        public virtual void setIntensity(float intensity)
        {
            intensity = Math.Max(intensity, 0);
            lightIntesity = intensity;
        }

    }

    class MagicLantern : Light
    {
        float targetIntensity;
        float stability = MEDIUM_STABILITY;
        float flickerIntensity = MEDIUM_FLICKER_INTENSITY;
        public static readonly float MEDIUM_STABILITY = .93f;
        public static readonly float HIGH_STABILITY = .96f;
        public static readonly float LOW_STABILITY = .9f;
        public static readonly float MEDIUM_FLICKER_INTENSITY = 1.0f;
        public static readonly float HIGH_FLICKER_INTENSITY = 4.0f;
        public MagicLantern(float _intensity, Color _color)
        {
            targetIntensity = _intensity;
            base.setIntensity(0);
            color = _color;
            stability = MEDIUM_STABILITY;
        }

        public void setStability(float s)
        {
            stability = s;
        }

        public void setFlickerIntensity(float s)
        {
            flickerIntensity = s;
        }

        protected override List<Action> update()
        {
            base.setIntensity(stability * getIntensity() + (1 - stability) * (targetIntensity + flickerIntensity * (float)(Globals.random.NextDouble() - .5) * targetIntensity));
            return new List<Action>();
        }

        public override void setIntensity(float intensity)
        {
            targetIntensity = intensity;
        }
    }

    class FireLight : MagicLantern
    {
        public FireLight() : base(1.2f, Color.LightYellow)
        {
            setFlickerIntensity(MagicLantern.MEDIUM_FLICKER_INTENSITY);
            setStability(MagicLantern.HIGH_STABILITY);
        }
    }

    class DirectionalLight : Light
    {
        RenderTargets shadowTargets;
        int backBufferWidth = 3200;
        Vector3 target = new Vector3(0, -1, 0);
        Vector3 up = new Vector3(0, 0, 1);
        Matrix rotation = Matrix.CreateFromYawPitchRoll(.3f, .75f, 0);

        public DirectionalLight(GraphicsDevice device)
        {
            target = Vector3.Transform(target, rotation);
            shadowTargets = new RenderTargets(backBufferWidth, backBufferWidth, device);

        }


        Matrix getViewMatrix()
        {

            
            Vector3 location = new Vector3(0, 75, 0);
            return Matrix.CreateLookAt(location, target + location, Vector3.Transform(up, rotation));
        }


        private Matrix getProjectionMatrix()
        {
            return Matrix.CreateOrthographic(200, 200, .1f, 100f);
        }

        public override void drawAlternateGBufferFirstPass(Effect effect, Matrix transform, BoundingFrustum frustum, GraphicsDevice device)
        {
            renderDirectionalLightFirstPass();
            foreach (GameObject child in children)
            {
                child.drawAlternateGBufferFirstPass(effect, transform, frustum, device);
            }

        }


        private GBuffer renderDirectionalLightFirstPass()
        {
            Matrix viewMatrix = getViewMatrix();
            Matrix projection = getProjectionMatrix();
            GBuffer LightPerspectiveGBuffer = Rendering.DrawGBuffer(viewMatrix, projection, shadowTargets);
            return LightPerspectiveGBuffer;
        }


        public override void drawSecondPass(Effect effect, Matrix transform, GraphicsDevice device)
        {
            effect.Parameters["lightIntensity"].SetValue(getIntensity()*2);
            effect.Parameters["shadowDepthMap"].SetValue(shadowTargets.getTextures().depth);
            effect.Parameters["lightDirection"].SetValue(target);
            Matrix viewMatrix = getViewMatrix();
            Matrix projection = getProjectionMatrix();
            effect.Parameters["lightView"].SetValue(viewMatrix);
            effect.Parameters["lightProjection"].SetValue(projection);
            effect.CurrentTechnique = effect.Techniques["DirectionalLightTechnique"];
            new QuadRenderer().Render(effect, device);

            foreach (GameObject child in children)
            {
                child.drawSecondPass(effect, transform, device);
            }
        }
    }
}
