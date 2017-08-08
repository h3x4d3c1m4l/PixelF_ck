using ImageSharp;
using PixelF_ck;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private static string _file;

        private static string _hostname;

        private static ushort _ports;

        private static int _threads;

        private static void Main(string[] args)
        {
            _hostname = args[0];
            _ports = ushort.Parse(args[1]);
            _file = args[2];
            _threads = int.Parse(args[3]);

            Test().Wait();
            Console.In.Read();
        }

        private static async Task Test()
        {
            using (var pf = new Pixelflut(_hostname, _ports))
            {
                // connect and get resolution
                await pf.Connect();
                var res = await pf.GetResolutionAsync();
                Console.WriteLine($"Connected, screen res: {res.X}x{res.Y}");

                // decode image and resize to screen res
                Rgba32[] pixels;
                using (Image<Rgba32> image = Image.Load(_file))
                {
                    Console.WriteLine($"Img size: {image.Width}x{image.Height}");
                    // https://stackoverflow.com/questions/1940581/c-sharp-image-resizing-to-different-size-while-preserving-aspect-ratio
                    double ratioX = res.X / (double)image.Width;
                    double ratioY = res.Y / (double)image.Height;
                    double ratio = ratioX < ratioY ? ratioX : ratioY;
                    int newHeight = Convert.ToInt32(image.Height * ratio);
                    int newWidth = Convert.ToInt32(image.Width * ratio);
                    image.Resize(newWidth, newHeight);
                    Console.WriteLine($"Image resized to: {image.Width}x{image.Height}");
                    image.Pad(res.X, res.Y);
                    pixels = image.Pixels.ToArray();
                    Console.WriteLine($"Image pad to: {image.Width}x{image.Height}");
                    image.SaveAsPng(File.OpenWrite(@"C:\temp\test2.png"));
                }

                var hexPixels = pixels.Select(x => x.ToHex().Substring(0, 6)).ToArray();
                for (var i = 0; i < _threads; i++)
                {
                    var t = new Thread(() =>
                    {
                        using (var tpf = new Pixelflut(_hostname, _ports))
                        {
                            while (true)
                            {
                                try
                                {
                                    tpf.Connect().Wait();
                                    while (true)
                                    {
                                        tpf.SendImage(hexPixels, res.X).Wait();
                                        Console.Out.Write('*');
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // thread crash
                                    Console.Out.Write('X');
                                }
                            }
                        }
                    });
                    t.Start();
                }
            }
        }
    }
}