using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PixelflutServer
{
    class Program
    {
        static void Main()
        {
            PixelManager.Init(640, 480);
            ConnectionManager.Init();

            new PixelflutWindow().Run();
        }
    }
}
