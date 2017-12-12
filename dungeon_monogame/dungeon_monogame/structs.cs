using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{

    public struct VertexPostitionColorPaintNormal
    {
        public Vector3 Position;
        public Color Color;
        public Color PaintColor;
        public Vector3 Normal;

        public VertexPostitionColorPaintNormal(Vector3 nPosition, Color nColor, Color nPaintColor, Vector3 nNormal)
        {
            Position = nPosition;
            Color = nColor;
            PaintColor = nPaintColor;
            Normal = nNormal;
        }


        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(sizeof(float) * 3 + 8, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );


    }

}
