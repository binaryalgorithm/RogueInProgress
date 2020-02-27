using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Diagnostics;

namespace RogueInProgress
{
    public class MapData
    {
        public Dictionary<(int x, int y), MapTile> ship = new Dictionary<(int x, int y), MapTile>();
        public int playerX = 0;
        public int playerY = 0;
        public List<CrewMember> crew = new List<CrewMember>();

        public MapTile GetShipTile(int x, int y)
        {
            if (ship.TryGetValue((x, y), out MapTile tile) == true)
            {
                return tile;
            }
            else // no ship here only empty void
            {
                tile = new MapTile();
                tile.x = x;
                tile.y = y;
                tile.backColor = Color.Black;
                tile.foreColor = Color.Black;
                tile.type = "void";
                return tile;
            }
        }

        public Dictionary<(int x, int y), MapTile> fragment = new Dictionary<(int x, int y), MapTile>();
        public int fragmentX = 0;
        public int fragmentY = 0;

        public MapTile GetFragmentTile(int x, int y)
        {
            if (fragment.TryGetValue((x, y), out MapTile tile) == true)
            {
                return tile;
            }
            else // no ship here only empty void
            {
                tile = new MapTile();
                tile.x = x;
                tile.y = y;
                tile.backColor = Color.Black;
                tile.foreColor = Color.Black;
                tile.type = "void";
                return tile;
            }
        }
    }

    public class MapRender
    {
        Stopwatch sw = new Stopwatch();

        public MapRender()
        {
            sw.Start();
        }

        public void DrawBackground(MapTile tile, int gridX, int gridY, double alpha = 1.0)
        {
            // solid block glyph
            int glyphX = 11;
            int glyphY = 13;

            double glyphRatio = 1.0 / 16.0; // 16 glyphs per row
            int tileSize = 20; // pixels per glyph 20x20

            GL.Color3((double)tile.backColor.R * alpha, (double)tile.backColor.G * alpha, (double)tile.backColor.B * alpha);

            GL.TexCoord2(glyphX * glyphRatio, glyphY * glyphRatio);
            GL.Vertex3(gridX * tileSize, gridY * tileSize, 0);

            GL.TexCoord2((glyphX + 1) * glyphRatio, glyphY * glyphRatio);
            GL.Vertex3(gridX * tileSize + tileSize, gridY * tileSize, 0);

            GL.TexCoord2((glyphX + 1) * glyphRatio, (glyphY + 1) * glyphRatio);
            GL.Vertex3(gridX * tileSize + tileSize, gridY * tileSize + tileSize, 0);

            GL.TexCoord2(glyphX * glyphRatio, (glyphY + 1) * glyphRatio);
            GL.Vertex3(gridX * tileSize, gridY * tileSize + tileSize, 0);
        }

        public void DrawGlyph(MapTile tile, int gridX, int gridY, double alpha = 1.0)
        {
            int glyphX = tile.glyph % 16;
            int glyphY = tile.glyph / 16;

            double glyphRatio = 1.0 / 16.0; // 16 glyphs per row
            int tileSize = 20; // pixels per glyph 20x20

            GL.Color3((double)tile.foreColor.R * alpha, (double)tile.foreColor.G * alpha, (double)tile.foreColor.B * alpha);

            GL.TexCoord2(glyphX * glyphRatio, glyphY * glyphRatio);
            GL.Vertex3(gridX * tileSize, gridY * tileSize, 0);

            GL.TexCoord2((glyphX + 1) * glyphRatio, glyphY * glyphRatio);
            GL.Vertex3(gridX * tileSize + tileSize, gridY * tileSize, 0);

            GL.TexCoord2((glyphX + 1) * glyphRatio, (glyphY + 1) * glyphRatio);
            GL.Vertex3(gridX * tileSize + tileSize, gridY * tileSize + tileSize, 0);

            GL.TexCoord2(glyphX * glyphRatio, (glyphY + 1) * glyphRatio);
            GL.Vertex3(gridX * tileSize, gridY * tileSize + tileSize, 0);
        }

        public void DrawShip(MapData map, int gridWidth, int gridHeight)
        {
            for (int gx = 0; gx < gridWidth; gx++)
            {
                for (int gy = 0; gy < gridHeight; gy++)
                {
                    MapTile tile = map.GetShipTile(gx - gridWidth/2 + map.playerX, gy - gridHeight/2 + map.playerY);

                    if (!(tile.type == "void"))
                    {
                        DrawBackground(tile, gx, gy);
                        DrawGlyph(tile, gx, gy);
                    }
                }
            }
        }

        public void DrawFragment(MapData map, int gridWidth, int gridHeight)
        {
            double alpha = Math.Cos(sw.ElapsedMilliseconds / 100);

            for (int gx = 0; gx < gridWidth; gx++)
            {
                for (int gy = 0; gy < gridHeight; gy++)
                {
                    MapTile tile = map.GetFragmentTile(gx - gridWidth / 2 + map.fragmentX, gy - gridHeight / 2 + map.fragmentY);

                    if (!(tile.type == "void"))
                    {
                        // DrawBackground(tile, gx, gy, alpha);
                        DrawGlyph(tile, gx, gy, alpha);
                    }
                }
            }
        }
    }

    public class CrewMember
    {
        public int x;
        public int y;
        public Color foreColor;

        public int destX;
        public int destY;
        public int stateOfMind; // what are we trying to do? current goal?

        public int engineeringSkill;
        public int pilotSkill;
        public int repairSkill;
    }

    public class MapTile
    {
        public int x;
        public int y;
        public Color backColor;
        public Color foreColor;
        public byte glyph;
        public string type;
    }

    public class Game : GameWindow
    {
        int textureIndex = 0;
        const double tileRatio = 1.0 / 16.0;
        const int tileSize = 20;

        KeyboardState lastInput;
        bool fragmentPlacementMode = true;

        MapData map = new MapData();
        MapRender render = new MapRender();

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }
            
            if (input.IsKeyDown(Key.Up) && !lastInput.IsKeyDown(Key.Up))
            {
                if (!fragmentPlacementMode)
                {
                    map.playerY--;
                }
                else
                {
                    map.fragmentY++;
                }
            }

            if (input.IsKeyDown(Key.Down) && !lastInput.IsKeyDown(Key.Down))
            {
                if (!fragmentPlacementMode)
                {
                    map.playerY++;
                }
                else
                {
                    map.fragmentY--;
                }
            }

            if (input.IsKeyDown(Key.Left) && !lastInput.IsKeyDown(Key.Left))
            {
                if (!fragmentPlacementMode)
                {
                    map.playerX--;
                }
                else
                {
                    map.fragmentX++;
                }
            }

            if (input.IsKeyDown(Key.Right) && !lastInput.IsKeyDown(Key.Right))
            {
                if (!fragmentPlacementMode)
                {
                    map.playerX++;
                }
                else
                {
                    map.fragmentX--;
                }
            }

            if (input.IsKeyDown(Key.Space) && !lastInput.IsKeyDown(Key.Space))
            {
                if (!fragmentPlacementMode)
                {
                    //
                }
                else
                {
                    // set fragment to ship map

                    foreach (KeyValuePair<(int x, int y), MapTile> item in map.fragment)
                    {
                        int x = item.Key.x;
                        int y = item.Key.y;
                        MapTile tile = item.Value;

                        map.ship[(x - map.fragmentX, y - map.fragmentY)] = tile;
                    }

                    fragmentPlacementMode = false;
                }
            }

            lastInput = input;

            base.OnUpdateFrame(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.WindowState = WindowState.Maximized;

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            textureIndex = LoadTexture("cp437_20x20.png"); // 16x16 texture tiles of 20x20 size

            Random rnd = new Random();

            // set ship map initially for testing

            MapTile tile = new MapTile();
            tile.foreColor = Color.Green;
            tile.type = "floor";
            tile.glyph = 14 + (16 * 12);

            map.ship[(0, 0)] = tile;
            map.ship[(1, 0)] = tile;
            map.ship[(0, 1)] = tile;
            map.ship[(-1, 0)] = tile;
            map.ship[(0, -1)] = tile;

            tile = new MapTile();
            tile.foreColor = Color.Red;
            tile.type = "floor";
            tile.glyph = 12 + (16 * 14);

            map.fragment[(0, 0)] = tile;
            map.fragment[(1, 0)] = tile;
            map.fragment[(0, 1)] = tile;
            map.fragment[(-1, 0)] = tile;
            map.fragment[(0, -1)] = tile;

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, Width, Height, 0.0, -1.0, 1.0); // 2D mode, pixel mode

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, textureIndex);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Color3(1.0, 1.0, 1.0);

            GL.Begin(PrimitiveType.Quads);

            render.DrawShip(map, 32, 32);

            if (fragmentPlacementMode)
            {
                render.DrawFragment(map, 32, 32);
            }

            GL.End();

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        public int LoadTexture(string file)
        {
            Bitmap bitmap = new Bitmap(file);

            int tex;
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            IntPtr ptr = data.Scan0;

            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Set every alpha channel byte to 0 (transparent) if the pixel is black
            // this allows glyphs to be transparent
            for (int counter = 3; counter < rgbValues.Length; counter += 4)
            {
                if (rgbValues[counter - 3] + rgbValues[counter - 2] + rgbValues[counter - 1] == 0)
                {
                    rgbValues[counter] = 0;
                }
                else
                {
                    rgbValues[counter] = 255;
                }
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return tex;
        }
    }
}
