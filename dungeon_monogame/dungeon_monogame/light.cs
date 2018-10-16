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

        public override void drawDeferredPass(Effect effect, Matrix transform, GraphicsDevice device)
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
                child.drawDeferredPass(effect, transform, device);
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
        public static readonly float HIGH_FLICKER_INTENSITY = 2.0f;
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
}
