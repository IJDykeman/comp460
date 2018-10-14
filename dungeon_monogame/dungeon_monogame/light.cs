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
        public MagicLantern(float _intensity, Color _color)
        {
            targetIntensity = _intensity;
            base.setIntensity(0);
            
            color = _color;
        }


        protected override List<Action> update()
        {
            float lambda = .93f;
            base.setIntensity(lambda * getIntensity() + (1 - lambda) * (targetIntensity + (float)(Globals.random.NextDouble() - .5)  * targetIntensity));
            return new List<Action>();
        }

        public override void setIntensity(float intensity)
        {
            targetIntensity = intensity;
        }
    }

    class FireLight : Light
    {
        float level = 0f;
        float luminanceTendency = 0;
        protected override List<Action> update()
        {
            luminanceTendency += .2f * (.5f * ((float)Globals.random.NextDouble() - .5f) - ((float)Math.Tanh(luminanceTendency) * .9f + .05f));

            level += luminanceTendency;
            level = MathHelper.Clamp(level, -2, 2);

            setIntensity((level + 2) / 4f /2f + 1.3f);
            return new List<Action>();
        }
    }
}
