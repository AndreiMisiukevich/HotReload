using System;
using System.Net;

namespace Xamarin.Forms
{
    public partial class HotReloader
    {
        public sealed class Configuration
        {
            /// <summary>
            /// Sets RELOADER's (Device/Simulator) port "http://127.0.0.1:{ReloaderPort}. Default value is 8000
            /// </summary>
            public int DeviceUrlPort { internal get; set; } = 8000;

            [Obsolete("USELESS SETTING. WILL BE REMOVED SOON")]
            public Scheme DeviceUrlScheme { internal get; set; }

            /// <summary>
            /// [SHOULD BE THE SAME WITH EXTENSION'S ALERT VALUE] Setup EXTENSION's autodiscovery port. Extension shows it in alert after enabling. Default value is 15000
            /// </summary>
            public int ExtensionAutoDiscoveryPort { internal get; set; } = 15000;

            /// <summary>
            ///  Setup EXTENSION's (PC/LAPTOP) IP address (where do you want to send your DEVICE's IP). Default value is IPAddress.Broadcast"
            /// </summary>
            public IPAddress ExtensionIpAddress { internal get; set; } = IPAddress.Broadcast;
        }
    }
}
