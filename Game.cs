using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK.Input;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RogueInProgress
{
    public class MapTile
    {
        public int x;
        public int y;
        public Color backColor;
        public Color foreColor;
        public byte glyph;
    }

    public class Game : GameWindow
    {
        int textureIndex = 0;
        const double tileRatio = 1.0 / 16.0;
        const int tileSize = 20;

        int playerMapX = 0;
        int playerMapY = 0;

        KeyboardState lastInput;

        Dictionary<(int x, int y), MapTile> map = new Dictionary<(int x, int y), MapTile>();
        MapTile defaultTile = new MapTile();

        public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        public void DrawMap(int centerX, int centerY, int radius)
        {
            int offsetX = radius * tileSize; // graphical offset from center
            int offsetY = radius * tileSize;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    MapTile currentTile = new MapTile();
                    map.TryGetValue((centerX + dx, centerY + dy), out currentTile);

                    if (currentTile != null)
                    {
                        int gx = (dx + radius) * tileSize;
                        int gy = (dy + radius) * tileSize;

                        // solid block
                        int tileX = 11;
                        int tileY = 13;

                        GL.Color3(currentTile.backColor.R, currentTile.backColor.G, currentTile.backColor.B);

                        GL.TexCoord2(tileX * tileRatio, tileY * tileRatio);
                        GL.Vertex3(gx, gy, 0);

                        GL.TexCoord2((tileX + 1) * tileRatio, tileY * tileRatio);
                        GL.Vertex3(gx + tileSize, gy, 0);

                        GL.TexCoord2((tileX + 1) * tileRatio, (tileY + 1) * tileRatio);
                        GL.Vertex3(gx + tileSize, gy + tileSize, 0);

                        GL.TexCoord2(tileX * tileRatio, (tileY + 1) * tileRatio);
                        GL.Vertex3(gx, gy + tileSize, 0);

                        //GL.Vertex3(gx, gy, 0);
                        //GL.Vertex3(gx + tileSize, gy, 0);
                        //GL.Vertex3(gx + tileSize, gy + tileSize, 0);
                        //GL.Vertex3(gx, gy + tileSize, 0);

                        if (dx == 0 && dy == 0)
                        {
                            tileX = 0;
                            tileY = 4;
                            currentTile.foreColor = Color.White;
                        }
                        else
                        {
                            tileX = currentTile.glyph % 16;
                            tileY = currentTile.glyph / 16;
                        }

                        // if(false)
                        {
                            GL.Color3(currentTile.foreColor.R, currentTile.foreColor.G, currentTile.foreColor.B);

                            GL.TexCoord2(tileX * tileRatio, tileY * tileRatio);
                            GL.Vertex3(gx, gy, 0);

                            GL.TexCoord2((tileX + 1) * tileRatio, tileY * tileRatio);
                            GL.Vertex3(gx + tileSize, gy, 0);

                            GL.TexCoord2((tileX + 1) * tileRatio, (tileY + 1) * tileRatio);
                            GL.Vertex3(gx + tileSize, gy + tileSize, 0);

                            GL.TexCoord2(tileX * tileRatio, (tileY + 1) * tileRatio);
                            GL.Vertex3(gx, gy + tileSize, 0);
                        }
                    }
                }
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }
            
            if (input.IsKeyDown(Key.Up) && !lastInput.IsKeyDown(Key.Up))
            {
                playerMapY--;
            }

            if (input.IsKeyDown(Key.Down) && !lastInput.IsKeyDown(Key.Down))
            {
                playerMapY++;
            }

            if (input.IsKeyDown(Key.Left) && !lastInput.IsKeyDown(Key.Left))
            {
                playerMapX--;
            }

            if (input.IsKeyDown(Key.Right) && !lastInput.IsKeyDown(Key.Right))
            {
                playerMapX++;
            }

            lastInput = input;

            base.OnUpdateFrame(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.WindowState = WindowState.Maximized;

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            //textureIndex = LoadTexture("test.png");
            textureIndex = LoadTexture("cp437_20x20.png"); // 16x16 texture tiles of 20x20 size

            Random rnd = new Random();

            for (int dx = -100; dx <= 100; dx++)
            {
                for (int dy = -100; dy <= 100; dy++)
                {
                    MapTile newTile = new MapTile();
                    newTile.x = dx;
                    newTile.y = dy;
                    newTile.glyph = (byte)rnd.Next(256);
                    newTile.backColor = Color.FromArgb((byte)rnd.Next(Math.Abs(dx)), (byte)rnd.Next(Math.Abs(dx)), (byte)rnd.Next(Math.Abs(dx)));
                    newTile.foreColor = Color.FromArgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
                    map[(dx, dy)] = newTile;
                }
            }

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

            DrawMap(playerMapX, playerMapY, 21);

            if (false)
            {
                double tileX = 2;
                double tileY = 0;

                double px = 0;
                double py = 0;

                int tileI = 0;

                for (py = 0; py < 16; py++)
                {
                    for (px = 0; px < 16; px++)
                    {
                        tileI++;

                        tileX = (int)(tileI % 16);
                        tileY = (int)(tileI / 16);

                        // solid block
                        //tileX = 11;
                        //tileY = 13;

                        GL.Color3(px * tileRatio, py * tileRatio, 0.5);

                        GL.TexCoord2(tileX * tileRatio, tileY * tileRatio);
                        GL.Vertex3(px * tileSize, py * tileSize, 0);

                        GL.TexCoord2((tileX + 1) * tileRatio, tileY * tileRatio);
                        GL.Vertex3((px + 1) * tileSize, py * tileSize, 0);

                        GL.TexCoord2((tileX + 1) * tileRatio, (tileY + 1) * tileRatio);
                        GL.Vertex3((px + 1) * tileSize, (py + 1) * tileSize, 0);

                        GL.TexCoord2(tileX * tileRatio, (tileY + 1) * tileRatio);
                        GL.Vertex3(px * tileSize, (py + 1) * tileSize, 0);
                    }
                }
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

            // Get the address of the first line.
            IntPtr ptr = data.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Set every third value to 255. A 24bpp bitmap will look red.  
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

            // Copy the RGB values back to the bitmap
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
