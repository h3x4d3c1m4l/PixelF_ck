using PixelF_ck;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        private static int _threads;

        private static int _bulkPixels;

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
                _threads = pOptions.Threads;
                _bulkPixels = pOptions.Bulkpixels;

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

                var imageX = res.X - _leftMargin - _rightMargin;
                var imageY = res.Y - _topMargin - _bottomMargin;

                // decode image and resize to screen res
                Rgba32[] pixels;
                using (var image = Image.Load(_file))
                {
                    pixels = new Rgba32[image.Width * image.Height];
                    Console.WriteLine($"Img size: {image.Width}x{image.Height}");
                    // https://stackoverflow.com/questions/1940581/c-sharp-image-resizing-to-different-size-while-preserving-aspect-ratio
                    var ratioX = imageX / (double)image.Width;
                    var ratioY = imageY / (double)image.Height;
                    var ratio = ratioX < ratioY ? ratioX : ratioY;
                    var newHeight = Convert.ToInt32(image.Height * ratio);
                    var newWidth = Convert.ToInt32(image.Width * ratio);
                    Console.WriteLine($"Resizing image to: {newWidth}x{newHeight}");
                    image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(newWidth, newHeight) }));
                    Console.WriteLine($"Image resized!");
                    image.Mutate(x => x.Pad(imageX, imageY));
                    var pixelBytes = image.SavePixelData();
                    for (var i = 0; i < pixelBytes.Length; i += 4)
                    {
                        pixels[i / 4] = new Rgba32(pixelBytes[i], pixelBytes[i + 1], pixelBytes[i + 2], pixelBytes[i + 3]);
                    }
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
                                    tpf.LoadImage(hexPixels, imageX, _bulkPixels, _leftMargin - 1, _topMargin - 1);
                                    while (true)
                                    {
                                        await tpf.SendImage();
                                        //Console.Out.Write('*');
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // thread crash
                                    lock (Console.Out)
                                    {
                                        Console.Out.WriteLine(ex);
                                    }
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