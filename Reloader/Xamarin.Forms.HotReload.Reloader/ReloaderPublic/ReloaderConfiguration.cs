using System;
namespace Xamarin.Forms
{
    public partial class HotReloader
    {
        public sealed class Configuration
        {
            /// <summary>
            /// Sets the RELOADER (Device/Simulator) port "http://127.0.0.1:{ReloaderPort}. Default value is 8000
            /// </summary>
            public int DeviceUrlPort { internal get; set; } = 8000;
            /// <summary>
            /// Sets the RELOADER (Device/Simulator) protocol scheme "{ReloaderProtocolScheme}://127.0.0.1:8000. Default value is HTTP
            /// </summary>
            public Scheme DeviceUrlScheme { internal get; set; }
            /// <summary>
            /// [SHOULD BE THE SAME WITH EXTENSION'S ALERT VALUE] Sets the EXTENSION autodiscovery port. Extension shows it in alert after enabling. Default value is 15000
            /// </summary>
            public int ExtensionAutoDiscoveryPort { internal get; set; } = 15000;
        }
    }
}
