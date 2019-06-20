using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Forms.HotReload.Extension.Helpers
{
    internal sealed class UdpReceiver
    {
        internal event Action<string> Received;
        private readonly object _lockObject = new object();

        private UdpClient _udpClient;
        private CancellationTokenSource _udpTokenSource;

        internal bool TryRun(out int port)
        {
            lock (_lockObject)
            {
                port = -1;
                Stop();
                foreach (var possiblePort in Enumerable.Range(15000, 2).Union(Enumerable.Range(17502, 200)))
                {
                    try
                    {
                        _udpClient = new UdpClient(possiblePort) { EnableBroadcast = true };
                        _udpTokenSource = new CancellationTokenSource();
                        var token = _udpTokenSource.Token;
                        Task.Run(() =>
                        {
                            while (!token.IsCancellationRequested)
                            {
                                try
                                {
                                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                                    var bytes = _udpClient.Receive(ref remoteEndPoint);

                                    if (!token.IsCancellationRequested && (bytes?.Any() ?? false))
                                    {
                                        var message = Encoding.ASCII.GetString(bytes);
                                        Received?.Invoke(message);
                                    }
                                }
                                catch
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        break;
                                    }
                                }
                            }
                        });
                        port = possiblePort;
                        return true;
                    }
                    catch
                    {
                        continue;
                    }
                }
                return false;
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
