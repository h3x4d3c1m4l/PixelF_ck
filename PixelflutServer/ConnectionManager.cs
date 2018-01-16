using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PixelflutServer
{
    static class ConnectionManager
    {
        private static int connectionCount;

        public static int ConnectionCount => connectionCount;

        public static void Init()
        {
            new Thread(() =>
            {
                var server = new TcpListener(IPAddress.Loopback, 1234);
                server.ExclusiveAddressUse = false;
                try
                {
                    server.Start();
                    while (true)
                    {
                        var socket = server.AcceptSocket();
                        Interlocked.Increment(ref connectionCount);
                        new Thread(() =>
                        {
                            using (socket)
                            {
                                socket.ReceiveTimeout = 30000;
                                //socket.SendTimeout = 5000;
                                connectionLoop(socket);
                            }
                            Interlocked.Decrement(ref connectionCount);
                        }).Start();
                    }
                }
                finally
                {
                    server.Stop();
                }
            }).Start();
        }

        private static void connectionLoop(Socket socket)
        {
            try
            {
                const int bufSize = 1;
                byte[] recvBuf = new byte[bufSize];
                var sb = new StringBuilder(100);
                while (socket.Connected)
                {
                    if (socket.Receive(recvBuf, bufSize, SocketFlags.None) != bufSize || sb.Length >= 100)
                        return;

                    var c = (char)recvBuf[0];
                    if (c == '\b')
                    {
                        // telnet backspace
                        if (sb.Length > 0)
                            sb.Length--;
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        // newline
                        if (sb.Length == 0) continue;

                        var answer = execCommand(sb.ToString());
                        if (answer.Length != 0)
                        {
                            var answerBytes = Encoding.UTF8.GetBytes(answer);
                            socket.Send(answerBytes);
                        }
                        sb.Clear();
                    }
                    else
                    {
                        // normal character
                        sb.Append(c);
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private static string execCommand(string v)
        {
            var cmdSplit = v.Split(' ');
            if (string.Equals(cmdSplit[0], "SIZE", StringComparison.OrdinalIgnoreCase))
            {
                // return size
                return $"SIZE {PixelManager.H} {PixelManager.V}\r\n";
            }
            else if (string.Equals(cmdSplit[0], "PX", StringComparison.OrdinalIgnoreCase))
            {
                // set pixel
                var x = fastIntParse(cmdSplit[1]);
                var y = fastIntParse(cmdSplit[2]);
                var rgb = Fast.FromHexString(cmdSplit[3]);
                var r = rgb[0];
                var g = rgb[1];
                var b = rgb[2];
                PixelManager.SetPixel(x, y, r, g, b);
                return string.Empty;
            }
            else
            {
                // send help
                return "This server supports: SIZE, PX.\r\n";
            }
        }

        private static int fastIntParse(string s)
        {
            var y = 0;
            var total = 0;
            for (int i = 0; i < s.Length; i++)
                y = y * 10 + (s[i] - '0');
            total += y;
            return total;
        }
    }
}
