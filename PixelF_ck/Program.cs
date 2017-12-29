using PixelF_ck;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.ImageSharp.Helpers;

namespace ConsoleApp1
{
    class Program
    {
        private static string _file;

        private static string _hostname;

        private static ushort _ports;

        private static int _leftMargin;

        private static int _rightMargin;

        private static int _topMargin;

        private static int _bottomMargin;

        private static int _threads = 1;

        private static void Main(string[] args)
        {
            var cmdLineParserResult = Parser.Default.ParseArguments<Options>(args);
            var settings = cmdLineParserResult.WithParsed(pOptions =>
            {
                _hostname = pOptions.Hostname;
                _ports = pOptions.Port;
                _file = pOptions.Image;
                _leftMargin = pOptions.LeftMargin;
                _rightMargin = pOptions.RightMargin;
                _topMargin = pOptions.TopMargin;
                _bottomMargin = pOptions.BottomMargin;

                Test();
            });
            Console.In.Read();
            Console.Out.Write("END"); 
        }

        private static async Task Test()
        {
            using (var pf = new Pixelflut(_hostname, _ports))
            {
                // connect and get resolution
                Console.WriteLine($"Connecting ...");
                await pf.Connect();
                Console.WriteLine($"Connected, retrieving res ...");
                var res = await pf.GetResolutionAsync();
                Console.WriteLine($"Screen res: {res.X}x{res.Y}");

                res.X = res.X - _leftMargin - _rightMargin;
                res.Y = res.Y - _topMargin - _bottomMargin;

                // decode image and resize to screen res
                Rgba32[] pixels;
                using (var image = Image.Load(_file))
                {
                    pixels = new Rgba32[image.Width * image.Height];
                    Console.WriteLine($"Img size: {image.Width}x{image.Height}");
                    // https://stackoverflow.com/questions/1940581/c-sharp-image-resizing-to-different-size-while-preserving-aspect-ratio
                    var ratioX = res.X / (double)image.Width;
                    var ratioY = res.Y / (double)image.Height;
                    var ratio = ratioX < ratioY ? ratioX : ratioY;
                    var newHeight = Convert.ToInt32(image.Height * ratio);
                    var newWidth = Convert.ToInt32(image.Width * ratio);
                    image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(newWidth, newHeight) }));
                    //image.Resize(newWidth, newHeight); // old API
                    Console.WriteLine($"Image resized to: {image.Width}x{image.Height}");
                    image.Mutate(x => x.Pad(res.X, res.Y));
                    //image.Pad(res.X, res.Y); // old API
                    //pixels = image.Pixels.ToArray(); // old API
                    var pixelBytes = image.SavePixelData(); // TODO !!!
                    for (var i = 0; i < pixelBytes.Length; i += 4)
                    {
                        pixels[i / 4] = new Rgba32(pixelBytes[i], pixelBytes[i + 1], pixelBytes[i + 2], pixelBytes[i + 3]);
                    }
                    Console.WriteLine($"Image pad to: {image.Width}x{image.Height}");
                    image.SaveAsPng(File.OpenWrite(@"C:\temp\test2.png"));
                }

                var hexPixels = pixels.Select(x => x.ToHex().Substring(0, 6)).ToArray();
                Console.Out.Write("Starting threads");
                for (var i = 0; i < _threads; i++)
                {
                    var t = new Thread(async () =>
                    {
                        using (var tpf = new Pixelflut(_hostname, _ports))
                        {
                            while (true)
                            {
                                try
                                {
                                    await tpf.Connect();
                                    tpf.LoadImage(hexPixels, res.X, 500, _leftMargin, _topMargin);
                                    while (true)
                                    {
                                        await tpf.SendImage();
                                        //Console.Out.Write('*');
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