using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

namespace Xamarin.Forms.HotReload.Extension.Helpers
{
    internal sealed class UdpReceiver
    {
        internal event Action<string> Received;

        private readonly int _port;
        private readonly object _lockObject = new object();

        private UdpClient _udpClient;
        private CancellationTokenSource _udpTokenSource;

        internal UdpReceiver(int port)
        {
            _port = port;
        }

        internal async void StartAsync()
        {
            lock (_udpClient)
            {
                _udpClient = new UdpClient(_port);
                _udpTokenSource?.Cancel();
                _udpTokenSource = new CancellationTokenSource();
            }

            var token = _udpTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                var receivedResult = _udpClient != null
                    ? await _udpClient.ReceiveAsync().ConfigureAwait(false)
                    : default(UdpReceiveResult);

                var bytes = receivedResult.Buffer;

                if (!token.IsCancellationRequested &&
                    bytes != null &&
                    bytes.Any())
                {
                    var message = Encoding.ASCII.GetString(bytes);
                    Received?.Invoke(message);
                }
            }
        }

        internal void Stop()
        {
            lock (_lockObject)
            {
                _udpTokenSource?.Cancel();
                _udpClient?.Dispose();
                _udpClient = null;
            }
        }
    }
}
