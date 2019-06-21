using System;
using System.Net;

namespace Xamarin.Forms
{
    public partial class HotReloader
    {
        public sealed class Configuration
        {
            /// <summary>
            /// Sets RELOADER's (Device/Simulator) desired port "http://127.0.0.1:{ReloaderPort}". Default value is 8000. Can be changed by reloader automatically.
            /// </summary>
            public int DeviceUrlPort { internal get; set; } = 8000;

            /// <summary>
            ///  Setup EXTENSION's (PC/LAPTOP) IP address (where do you want to send your DEVICE's IP). Default value is IPAddress.Broadcast. Set PC/Laptop IP in case of device and PC are in different subnets
            /// </summary>
            public IPAddress ExtensionIpAddress { internal get; set; } = IPAddress.Broadcast;

            /// <summary>
            /// [SHOULD BE THE SAME WITH EXTENSION'S ALERT VALUE] Setup EXTENSION's autodiscovery port. Extension shows it in alert after enabling. Default value is 15000
            /// </summary>
            [Obsolete("NO NEED TO SET THIS PROPERTY ANYMORE. RELOADER DETECTS THIS PORT AUTOMATICALLY")]
            public int ExtensionAutoDiscoveryPort { internal get; set; }
        }
    }
}
