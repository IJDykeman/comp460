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
        Color color = Color.LightYellow;

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


        public void setIntensity(float intensity)
        {
            intensity = Math.Max(intensity, 0);
            lightIntesity = intensity;
        }

    }
}
