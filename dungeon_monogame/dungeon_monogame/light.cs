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

    class HemisphereLight : Light
    {

        Vector3 direction;
        public HemisphereLight(float _intensity, Color _color, Vector3 _direction) : base(_intensity, _color)
        {
            direction = _direction;
        }

        public override void drawSecondPass(Effect effect, Matrix transform, GraphicsDevice device)
        {
            transform = getTransform(ref transform);
            effect.CurrentTechnique = effect.Techniques["HemisphereLightTechnique"];
            effect.Parameters["lightIntensity"].SetValue(getIntensity());
            effect.Parameters["lightRadius"].SetValue(20f * getIntensity());
            effect.Parameters["lightDirection"].SetValue(direction);
            effect.Parameters["lightColor"].SetValue(color.ToVector4());
            Vector3 position = Vector3.Transform(getLocation(), transform);
            effect.Parameters["lightPosition"].SetValue(position);
            new QuadRenderer().Render(effect, device);

            foreach (GameObject child in children)
            {
                child.drawSecondPass(effect, transform, device);
            }
        }
    
    }

    class MagicLantern : PointLight
    {
        float targetIntensity;
        float stability = MEDIUM_STABILITY;
        float flickerIntensity = MEDIUM_FLICKER_INTENSITY;
        public static readonly float MEDIUM_STABILITY = .93f;
        public static readonly float HIGH_STABILITY = .96f;
        public static readonly float LOW_STABILITY = .9f;
        public static readonly float MEDIUM_FLICKER_INTENSITY = 1.0f;
        public static readonly float HIGH_FLICKER_INTENSITY = 4.0f;
        public MagicLantern(float _intensity, Color _color, Vector3 _)
        {
            targetIntensity = _intensity;
            color = _color; 
        }

        public MagicLantern(float _intensity, Color _color)
        {
            targetIntensity = _intensity;
            color = _color;
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
            float target = stability * getIntensity() + (1 - stability) * (targetIntensity + flickerIntensity * (float)(Globals.random.NextDouble() - .5) * targetIntensity);
            base.setIntensity(target);
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


        /*public DirectionalLight(GraphicsDevice device)
        {
            target = Vector3.Transform(target, rotation);
            shadowTargets = new RenderTargets(backBufferWidth, backBufferWidth, device);

        }*/


        Matrix getViewMatrix()
        {

            
            Vector3 location = new Vector3(0, 75, 0);
            return Matrix.CreateLookAt(location, target + location, Vector3.Transform(up, rotation));
        }


        private Matrix getProjectionMatrix()
        {
            return Matrix.CreateOrthographic(50, 50, .1f, 100f);
        }

        public override void drawAlternateGBufferFirstPass(Matrix transform)
        {
            renderDirectionalLightFirstPass();
            foreach (GameObject child in children)
            {
                child.drawAlternateGBufferFirstPass(transform);
            }

        }


        private GBuffer renderDirectionalLightFirstPass()
        {
            Matrix viewMatrix = getViewMatrix();
            Matrix projection = getProjectionMatrix();
            GBuffer LightPerspectiveGBuffer = Rendering.DrawGBuffer(viewMatrix, projection, shadowTargets, a => true);
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

    class PointLight : Light
    {
        List<SpotLight> lights;
        public PointLight() : base()
        {
            lights = new List<SpotLight>();
            lights.Add(new SpotLight(Vector3.UnitX, Vector3.UnitY));
            lights.Add(new SpotLight(-Vector3.UnitX, Vector3.UnitY));
            lights.Add(new SpotLight(Vector3.UnitY, Vector3.UnitZ));
            lights.Add(new SpotLight(-Vector3.UnitY, Vector3.UnitZ));
            lights.Add(new SpotLight(Vector3.UnitZ, Vector3.UnitY));
            lights.Add(new SpotLight(-Vector3.UnitZ, Vector3.UnitY));
            foreach (Light l in lights)
            {
                addChild(l);
            }
        }

        public override void setIntensity(float intensity)
        {
            base.setIntensity(intensity);
            foreach (Light l in lights)
            {
                l.setIntensity(intensity);
            }
        }

    }


    class SpotLight : DirectionalLight
    {
        RenderTargets shadowTargets;
        int backBufferWidth = 600;
        Vector3 target = new Vector3(0, -1, 0);
        Vector3 up = new Vector3(0, 0, 1);
        Matrix rotation = Matrix.CreateFromYawPitchRoll(0,0, 0);

        public SpotLight(Vector3 _target, Vector3 _up) {
            target = _target;
            up = _up;
        }


        Matrix getViewMatrix(Matrix transform)
        {
            Vector3 location = transform.Translation;
            return Matrix.CreateLookAt(location, target + location, Vector3.Transform(up, rotation));
        }


        private Matrix getProjectionMatrix()
        {
            float aspectRatio = 1;
            float fieldOfView = MathHelper.ToRadians(90f);
            float nearClipPlane = 0.004f;
            float farClipPlane = 80;
            return Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
        }

        public override void drawAlternateGBufferFirstPass( Matrix transform)
        {
            if (shadowTargets == null)
            {
                shadowTargets = Rendering.getEmptyTargets(backBufferWidth, backBufferWidth);
            }

            renderSpotLightFirstPass(transform);
            foreach (GameObject child in children)
            {
                child.drawAlternateGBufferFirstPass(transform);
            }

        }


        private GBuffer renderSpotLightFirstPass(Matrix transform)
        {
            Matrix viewMatrix = getViewMatrix(transform);
            Matrix projection = getProjectionMatrix();
            GBuffer LightPerspectiveGBuffer = Rendering.DrawGBuffer(viewMatrix, projection, shadowTargets, a => !a.hasTag(ObjectTag.DoesNotCastShadow));
            return LightPerspectiveGBuffer;
        }


        public override void drawSecondPass(Effect effect, Matrix transform, GraphicsDevice device)
        {
            effect.Parameters["lightIntensity"].SetValue(getIntensity());
            effect.Parameters["shadowDepthMap"].SetValue(shadowTargets.getTextures().depth);
            Rendering.debugTexture = shadowTargets.getTextures().depth;
            effect.Parameters["lightDirection"].SetValue(target);
            Matrix viewMatrix = getViewMatrix(transform);
            Matrix projection = getProjectionMatrix();
            effect.Parameters["lightView"].SetValue(viewMatrix);
            effect.Parameters["lightPosition"].SetValue(transform.Translation);
            effect.Parameters["lightProjection"].SetValue(projection);
            //effect.Parameters["lightIntensity"].SetValue(lightIntesity);
            effect.Parameters["lightRadius"].SetValue(20f * getIntensity());
            effect.CurrentTechnique = effect.Techniques["SpotLightTechnique"];
            new QuadRenderer().Render(effect, device);

            foreach (GameObject child in children)
            {
                child.drawSecondPass(effect, transform, device);
            }
        }
    }
}

