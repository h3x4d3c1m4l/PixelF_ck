using System.Timers;

namespace PixelflutServer
{
    /// <summary>
    /// Manages the pixels.
    /// </summary>
    static class PixelManager
    {
        private static byte[,,] pixels;

        public static byte[,,]Pixels => pixels;

        public static int H { get; private set; }

        public static int V { get; private set; }

        private static int _pps;

        public static int Pps => _pps;

                /// <summary>
        /// Inits the pixel grid.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public static void Init(int h, int v)
        {
            H = h;
            V = v;
            pixels = new byte[v, h, 3];

            var timer = new Timer();
            timer.Elapsed += (_, __) =>
            {
                _pps = 0;
            };
            timer.Interval = 1000;
            timer.Start();
        }

        public static void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            lock (pixels)
            {
                _pps++;
                pixels[V - y - 1, x, 0] = r;
                pixels[V - y - 1, x, 1] = g;
                pixels[V - y - 1, x, 2] = b;
            }
        }
    }
}
