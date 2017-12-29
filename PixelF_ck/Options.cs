using System.Collections.Generic;
using CommandLine;

namespace PixelF_ck
{
    public class Options {
        [Option('l', "leftmargin", HelpText = "Left margin (in px)")]
        public int LeftMargin { get; set; }

        [Option('t', "topmargin", HelpText = "Top margin (in px)")]
        public int TopMargin { get; set; }

        [Option('r', "rightmargin", HelpText = "Right margin (in px)")]
        public int RightMargin { get; set; }

        [Option('b', "bottommargin", HelpText = "Bottom margin (in px)")]
        public int BottomMargin { get; set; }

        [Option('h', "hostname", HelpText = "Pixelflut server hostname (default = '127.0.0.1')'")]
        public string Hostname { get; set; } = "127.0.0.1";

        [Option('p', "port", HelpText = "Pixelflut server port (default = 8080)")]
        public ushort Port { get; set; } = 1234;
        
        [Option('i', "image", HelpText = "Image filename", Required = true)]
        public string Image { get; set; }
    }
}