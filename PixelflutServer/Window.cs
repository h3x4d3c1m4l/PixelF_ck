using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Text;

namespace PixelflutServer
{
    class PixelflutWindow : GameWindow
    {
        public PixelflutWindow() : base(640, 480, GraphicsMode.Default, "Pixelflut")
        {
            //VSync = VSyncMode.On;
        }



        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();
        }

        protected override unsafe void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            DrawImage(loadTexture());

            SwapBuffers();
        }

        private int tex;

        // https://stackoverflow.com/questions/11645368/opengl-c-sharp-opentk-load-and-draw-image-functions-not-working
        private unsafe int loadTexture()
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            if (tex == 0)
                GL.GenTextures(1, out int tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            fixed (byte* p = PixelManager.Pixels)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, PixelManager.H, PixelManager.V, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)p);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return tex;
        }

        public static void DrawImage(int image)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix(); 
            GL.LoadIdentity();

            GL.Ortho(0, PixelManager.H, 0, PixelManager.V, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.Lighting);

            GL.Enable(EnableCap.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, image);

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex3(0, 0, 0);

            GL.TexCoord2(1, 0);
            GL.Vertex3(640, 0, 0);

            GL.TexCoord2(1, 1);
            GL.Vertex3(640, 480, 0);

            GL.TexCoord2(0, 1);
            GL.Vertex3(0, 480, 0);

            GL.End();

            GL.Disable(EnableCap.Texture2D);
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();

            GL.MatrixMode(MatrixMode.Modelview);
        }
    }
}
