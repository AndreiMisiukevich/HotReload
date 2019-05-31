using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xamarin.Forms.HotReload.Reloader
{
    internal sealed class UdpSender
    {
        public void Send(string message, int port = 15000)
        {
            using (var client = new UdpClient())
            {
                var ip = new IPEndPoint(IPAddress.Broadcast, port);
                var bytes = Encoding.ASCII.GetBytes(message);
                client.Send(bytes, bytes.Length, ip);
            }
        }
    }
}
