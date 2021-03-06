﻿using dungeon_monogame.WorldGeneration;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace dungeon_monogame
{
    /// <summary>
    /// Support for reading .VOX files generated by Ephtracy's MagicaVoxel Tool.
    /// https://ephtracy.github.io/
    /// </summary>
    /// 
    // adapted from : https://github.com/Arlorean/Voxels.Core/blob/master/MagicaVoxel.cs
    // MIT license

    class MagicaVoxel
    {
        public static string root = System.Reflection.Assembly.GetEntryAssembly().Location;
        public static string modelsRoot = root+@"..\..\..\..\..\..\..\..\voxel_models\";
        public static Color GetDefaultColor(int i)
        {
            return new Color(DefaultColors[i]);
        }

        static readonly uint[] DefaultColors = new uint[] { // 0xAABBGGRR (little endian)
            0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
            0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
            0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
            0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
            0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
            0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
            0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
            0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
            0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
            0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
            0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
            0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
            0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
            0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
            0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
            0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111,
        };

        const int VOX_ = ('V') + ('O' << 8) + ('X' << 16) + (' ' << 24);
        const int MAIN = ('M') + ('A' << 8) + ('I' << 16) + ('N' << 24);
        const int PACK = ('P') + ('A' << 8) + ('C' << 16) + ('K' << 24);
        const int SIZE = ('S') + ('I' << 8) + ('Z' << 16) + ('E' << 24);
        const int XYZI = ('X') + ('Y' << 8) + ('Z' << 16) + ('I' << 24);
        const int RGBA = ('R') + ('G' << 8) + ('B' << 16) + ('A' << 24);

        public static Tuple<List<IntLoc>, Color[], List<int>, Tuple<int, int, int, int, int, int>> Read1(Stream stream)
        {
            int xmin = 90000;
            int ymin = 90000;
            int zmin = 90000;
            int xmax = -90000;
            int ymax = -90000;
            int zmax = -90000;

            int sx, sy, sz;
            sx = 0;
            sy = 0;
            sz = 0;
            
            BinaryReader reader = new BinaryReader(stream);
            var magic = reader.ReadUInt32();
            var version = reader.ReadInt32();
            List<IntLoc> blockLocs = new List<IntLoc>();
            List<int> colorIndices= new List<int>();

            if (magic != VOX_)
            {
                return null;
            }

            //var voxelData = null as VoxelData;
            Color[] colors = new Color[256];
            for (var i = 0; i < DefaultColors.Length; ++i)
            {
                colors[i] = new Color(DefaultColors[i]);
            }
            while (reader.PeekChar() != -1)
            {
                var chunkId = reader.ReadUInt32();
                var chunkSize = reader.ReadInt32();
                var childChunks = reader.ReadInt32();

                switch (chunkId)
                {
                    case SIZE:
                        
                        sx = reader.ReadInt32();
                        sy = reader.ReadInt32();
                        sz = reader.ReadInt32();
                        reader.ReadBytes(chunkSize - sizeof(int) * 3);
                        //voxelData = new VoxelData(new XYZ(sx, sy, sz), new Color[256]);
                        break;
                    case XYZI:
                        var n = reader.ReadInt32();
                        for (var i = 0; i < n; ++i)
                        {
                            var x = reader.ReadByte();
                            var y = reader.ReadByte();
                            var z = reader.ReadByte();
                            xmin = MathHelper.Min(x, xmin);
                            xmax = MathHelper.Max(x, xmax);
                            ymin = MathHelper.Min(y, ymin);
                            ymax = MathHelper.Max(y, ymax);
                            zmin = MathHelper.Min(z, zmin);
                            zmax = MathHelper.Max(z, zmax);
                            var c = reader.ReadByte();
                            blockLocs.Add(new IntLoc(x, z, y));
                            colorIndices.Add(c);
                        }
                        break;
                    case RGBA:
                        colors = new Color[256];
                        // last color is not used, so we only need to read 255 colors
                        for (var i = 1; i < 256; ++i)
                        {
                            byte r = reader.ReadByte();
                            byte g = reader.ReadByte();
                            byte b = reader.ReadByte();
                            byte a = reader.ReadByte();
                            //Console.WriteLine(r);
                            //Console.WriteLine(a);
                            colors[i] = new Color(r, g, b, a);
                        }
                        // NOTICE : skip the last reserved color
                        reader.ReadUInt32();
                        break;
                    default:
                        reader.ReadBytes(chunkSize);
                        break;
                }
            }

            //if (voxelData != null && colors == null)
            if (colors == null)
            {
                for (var i = 0; i < DefaultColors.Length; ++i)
                {
                    colors[i] = new Color(DefaultColors[i]);
                }
            }
            stream.Close();

            xmin = 0;
            ymin = 0;
            zmin = 0;
            xmax = sx;
            ymax = sy;
            zmax = sz;
            return new Tuple<List<IntLoc>, Color[], List<int>, Tuple<int,int,int,int,int,int>>(blockLocs, colors, colorIndices, new Tuple<int,int,int,int,int,int>(xmin, xmax, ymin, ymax, zmin, zmax));

        }


        public static ChunkManager ChunkManagerFromStream(Stream stream)
        {
            Tuple<List<IntLoc>, Color[], List<int>, Tuple<int, int, int, int, int, int>> data = Read1(stream);
            ChunkManager manager = new ChunkManager();
            List<IntLoc> blockLocs = data.Item1;
            List<int> colorIndices = data.Item3;
            Color[] colors = data.Item2;
            Tuple<int, int, int, int, int, int> extents = data.Item4;
            for (int i = 0; i < blockLocs.Count; i++)
            {
                manager.set(blockLocs[i], new Block(1, colors[colorIndices[i]]));
            }
            //return voxelData;
            manager.remeshAllSerial();
            manager.setExtents(extents.Item1, extents.Item2, extents.Item3, extents.Item4, extents.Item5, extents.Item6);
            return manager;
        }

        public static ChunkManager ChunkManagerFromVoxAbsolutePath(string path)
        {
            Stream stream = File.Open(path, FileMode.Open);
            return ChunkManagerFromStream(stream);
        }

        public static ChunkManager ChunkManagerFromResource(string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            return ChunkManagerFromStream(stream);
        }


        public static ChunkManager ChunkManagerFromVox(string path)
        {
            return ChunkManagerFromVoxAbsolutePath(modelsRoot + path);
        }

        public static List<int> getVoxelModelSize(string path)
        {
            Tuple<List<IntLoc>, Color[], List<int>, Tuple<int, int, int, int, int, int>> data = Read1(File.Open(path, FileMode.Open));
            Tuple<int, int, int, int, int, int> extents = data.Item4;
            int sx = extents.Item2 - extents.Item1;
            int sy = extents.Item4 - extents.Item3;
            int sz = extents.Item6 - extents.Item5;
            List<int> result = new List<int> { sx, sy, sz };
            return result;
        }

        public static List<Tile> TilesFromPath(string path, int tileWidth)
        {
            Tuple<List<IntLoc>, Color[], List<int>, Tuple<int, int, int, int, int, int>> data = Read1(File.Open(path, FileMode.Open));
            List<IntLoc> blockLocs = data.Item1;
            List<int> colorIndices = data.Item3;
            Color[] colors = data.Item2;
            Tuple<int, int, int, int, int, int> extents = data.Item4;

            int xSize = (int)Math.Ceiling(((extents.Item2 - extents.Item1 + 1) / (double)tileWidth)) * tileWidth;
            int ySize = (int)Math.Ceiling(((extents.Item4 - extents.Item3 + 1) / (double)tileWidth)) * tileWidth;
            int zSize = (int)Math.Ceiling(((extents.Item6 - extents.Item5 + 1) / (double)tileWidth)) * tileWidth;
            Block[,,] megaTile = new Block[xSize, ySize, zSize];
            for (int i = 0; i < blockLocs.Count; i++)
            {
                // a megatile is a single model containing multiple tiles.  Can be convenient for creating complex tilesets.
                megaTile[blockLocs[i].i, blockLocs[i].j, blockLocs[i].k] = new Block(1, colors[colorIndices[i]]);
            }
            List<Tile> result = new List<Tile>();
            for (int a = 0; a < xSize / tileWidth; a++)
            {
                for (int b = 0; b < ySize / tileWidth; b++)
                {
                    for (int c = 0; c < zSize / tileWidth; c++)
                    {
                        Block[,,] smalltile = new Block[tileWidth, tileWidth, tileWidth];
                        for (int i = 0; i < tileWidth; i++)
                        {
                            for (int j = 0; j < tileWidth; j++)
                            {
                                for (int k = 0; k < tileWidth; k++)
                                {
                                    smalltile[i, j, k] = megaTile[a * tileWidth + i, b * tileWidth + j, c * tileWidth + k];

                                }
                            }
                        }
                        result.Add(new Tile(smalltile, tileWidth, path.Split('\\').Last()));
                    }
                }

            }
            return result;
        }

        public static List<Tile> TilesFromExampleModel(string path, int tileWidth)
        {
            Tuple<List<IntLoc>, Color[], List<int>, Tuple<int, int, int, int, int, int>> data = Read1(File.Open(path, FileMode.Open));
            List<IntLoc> blockLocs = data.Item1;
            List<int> colorIndices = data.Item3;
            Color[] colors = data.Item2;
            Tuple<int, int, int, int, int, int> extents = data.Item4;

            int xSize = (extents.Item2 + 1);
            int ySize = (extents.Item4 + 1);
            int zSize = (extents.Item6 + 1);
            int size = Math.Max(zSize, Math.Max(xSize, ySize));
            Block[,,] megaTile = new Block[size, size, size];
            for (int i = 0; i < blockLocs.Count; i++)
            {
                megaTile[blockLocs[i].i, blockLocs[i].j, blockLocs[i].k] = new Block(1, colors[colorIndices[i]]);
            }
            List<Tile> result = new List<Tile>();
            for (int a = 1; a < xSize - 1; a++)
            {
                for (int b = 1; b < ySize - 1; b++)
                {
                    for (int c = 1; c < zSize - 1; c++)
                    {
                        Block[,,] smalltile = new Block[tileWidth, tileWidth, tileWidth];
                        for (int i = 0; i < tileWidth; i++)
                        {
                            for (int j = 0; j < tileWidth; j++)
                            {
                                for (int k = 0; k < tileWidth; k++)
                                {
                                    smalltile[i, j, k] = megaTile[a + i - 1, b + j - 1, c + k - 1];

                                }
                            }
                        }

                        Tile nTile = new Tile(smalltile, tileWidth, path.Split('\\').Last());
                        if (!result.Contains(nTile))
                        {
                            result.Add(nTile);

                        }
                    }
                }

            }
            return result;
        }



    }
}
