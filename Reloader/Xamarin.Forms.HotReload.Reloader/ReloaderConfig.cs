
namespace Xamarin.Forms.HotReload.Reloader
{
    public sealed class ReloaderConfig
    {
        private readonly string[] _addresses;
        internal ReloaderConfig(string[] addresses)
        {
            _addresses = addresses;
        }

        public void ConfigureAutoDiscovery(int receiverPort = 15000)
        {
            var ipMsg = string.Join(";", _addresses ?? new string[0]);
            var sender = new UdpSender();
            sender.Send(ipMsg);
        }
    }
}
