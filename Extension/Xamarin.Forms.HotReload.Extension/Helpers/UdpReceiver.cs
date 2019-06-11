using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xamarin.Forms.HotReload.Extension.Helpers
{
    internal sealed class UdpReceiver
    {
        internal event Action<string> Received;

        private readonly int? _port;
        private readonly object _lockObject = new object();
        private readonly UdpClient _udpClient;
        
        internal UdpReceiver(int port = 15000)
        {
            var maxPort = port + 200;
            while(!_port.HasValue && port < maxPort)
            {
                try
                {
                    _udpClient = new UdpClient(port);
                    ListenTo();
                    _port = port;
                }
                catch
                {
                    ++port;
                    continue;
                }
            }
        }

        public int? Port => _port;
        
        private void ListenTo()
        {
            _udpClient.BeginReceive(Receive, new object());
        }

        private void Receive(IAsyncResult ar)
        {
            lock (_lockObject)
            {
                var ip = new IPEndPoint(IPAddress.Any, _port.Value);
                var bytes = _udpClient.EndReceive(ar, ref ip);
                var message = Encoding.ASCII.GetString(bytes);
                Received?.Invoke(message);
                ListenTo();
            }
        }
    }
}
