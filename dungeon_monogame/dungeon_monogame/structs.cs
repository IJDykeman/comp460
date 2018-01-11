using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{

    public struct VertexPostitionColorPaintNormal : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Color IndirectLightColor;
        public Vector3 Normal;
        public Color emissive;

        public VertexPostitionColorPaintNormal(Vector3 nPosition, Color nColor, Color nPaintColor, Vector3 nNormal)
        {
            Position = nPosition;
            Color = nColor;
            IndirectLightColor = nPaintColor;
            Normal = nNormal;
            emissive = Color.Black;
        }

        public VertexPostitionColorPaintNormal(Vector3 nPosition, Color nColor, Color nPaintColor, Vector3 nNormal, Color nEmissive)
        {
            Position = nPosition;
            Color = nColor;
            IndirectLightColor = nPaintColor;
            Normal = nNormal;
            emissive = nEmissive;
        }


        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(sizeof(float) * 3 + 8, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 3 * 2 + 8, VertexElementFormat.Color, VertexElementUsage.Color, 2)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }
    }

    struct Block
    {
        public int type;
        public Color color;

        public Block(int t, byte r, byte g, byte b)
        {
            type = t;
            color = new Color(r, g, b);
        }

        public Block(int t, Color c)
        {
            type = t;
            color = c;
        }

        public override bool Equals(object obj)
        {
            return (((Block)obj).type == type) && (((Block)obj).color == color);
        }



    }

    struct IntLoc
    {
        public int i, j, k;

        public IntLoc(int _i, int _j, int _k)
        {
            i = _i;
            j = _j;
            k = _k;
        }
        public IntLoc(Vector3 v)
        {
            i = (int)Math.Floor(v.X);
            j = (int)Math.Floor(v.Y);
            k = (int)Math.Floor(v.Z);
        }

        public Vector3 toVector3()
        {
            return new Vector3(i, j, k);
        }

        public override int GetHashCode()
        {
            return (i.ToString() + "_" + j.ToString() + "_" + k.ToString()).GetHashCode();
        }

        public static IntLoc operator %(IntLoc l, int n)
        {
            return new IntLoc(Globals.mod(l.i, n), Globals.mod(l.j, n), Globals.mod(l.k, n));
        }

        public static IntLoc operator -(IntLoc a, IntLoc b)
        {
            return new IntLoc(a.i - b.i, a.j - b.j, a.k - b.k);
        }

        public static IntLoc operator +(IntLoc a, IntLoc b)
        {
            return new IntLoc(a.i + b.i, a.j + b.j, a.k + b.k);
        }

        public static IntLoc operator *(IntLoc a, int b)
        {
            return new IntLoc(a.i * b, a.j * b, a.k * b);
        }
    }


}
