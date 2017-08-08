using ImageSharp;
using ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PixelF_ck
{
    class Pixelflut : IDisposable
    {
        private TcpClient _client;

        private StreamReader _reader;

        private StreamWriter _writer;

        private string _hostname;

        private ushort _port;

        public Pixelflut(string pHostname, ushort pPort)
        {
            _hostname = pHostname;
            _port = pPort;
        }

        public async Task Connect()
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_hostname, _port);
            _writer = new StreamWriter(_client.GetStream(), Encoding.ASCII);
            _reader = new StreamReader(_client.GetStream(), Encoding.ASCII);
        }

        public async Task<(int X, int Y)> GetResolutionAsync()
        {
            await _writer.WriteAsync("SIZE\n");
            await _writer.FlushAsync();
            while (!_client.GetStream().DataAvailable)
                await Task.Delay(10);
            var line = _reader.ReadLine(); // returns something like "SIZE 1024 768"
            _reader.DiscardBufferedData(); // clears any additional CR/LF
            var split = line.Split(' ');
            return (int.Parse(split[1]), int.Parse(split[2]));
        }

        public async Task SendImage(string[] pPixels, int pHorizontalPixels)
        {
            var px = 0;
 
            for (var y = 0; y < (pPixels.Length / pHorizontalPixels); y++)
            {
                for (var x = 0; x < pHorizontalPixels; x++)
                {
                    await _writer.WriteAsync($"PX {x} {y} {pPixels[px]}\n");
                    await _writer.FlushAsync();
                    px++;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _reader?.Dispose();
                    _writer?.Dispose();
                    _client?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Pixelflut() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
