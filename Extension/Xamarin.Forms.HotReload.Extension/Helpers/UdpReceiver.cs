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
            var activeUdpListeners = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            while (activeUdpListeners.Any(p => p.Port == port) && port < 15200)
            {
                port++;
            }

            if (port == 15200)
            {
                _port = null;
            }
            else
            {
                _port = port;
                _udpClient = new UdpClient(_port.Value);
                ListenTo();
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
