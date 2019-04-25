using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace dungeon_monogame
{
    class GlobalSettings
    {
        public static readonly float maxAmbientLight = 3f;
        public static readonly float defaultAmbientLight = .7f;
        public static readonly float minAmbientLight = 0.0f;
        public static readonly Color worldBackgroundColor = Color.Black;
        public static float vignetteStrength = 0f;
        public static Keys OpenMainMenuKey = Keys.Escape;

        public static float AmbientLightContinuousAdjustmentIncrement = 1f / 60f;

        //UI
        public static readonly Color defaultButtonColor = Color.White;
        public static readonly Color mousedOVerButtonColor= Color.Gray;

        public static RasterizerState DrawGBufferRasterizerState = RasterizerState.CullCounterClockwise;

        public static int BlockColorJitter = 5;
    }
}
