using System.Collections.Generic;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Helpers;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension
{
    public class Main
    {
        private static ExtensionCore _extensionCore;
        private static bool _isInitialized;

        public static void Init(EnvironmentService environmentService,
            Dictionary<HotReloadCommands, IEnvironmentCommand> enviromentCommands,
            IGuiService service, ISettingsService settingsStore)
        {
            if (!_isInitialized)
            {
                var serializer = new JsonSerializer();
                SharedGlobals.DefaultSerializedConnectionItemsValue = serializer.Serialize(
                    new List<ConnectionItem>
                    {
                        new ConnectionItem(SharedGlobals.DefaultProtocolValue, SharedGlobals.DefaultIpAddressValue,
                            SharedGlobals.DefaultPortValue)
                    });
                _extensionCore = new ExtensionCore(service, settingsStore, new OutputLogger());
                _extensionCore.Init(environmentService, enviromentCommands);
                _isInitialized = true;
            }
        }
    }
}