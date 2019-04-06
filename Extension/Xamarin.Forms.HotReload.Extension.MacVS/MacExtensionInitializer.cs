using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.MacVS.Commands;

namespace Xamarin.Forms.HotReload.Extension.MacVS
{
    public static class MacExtensionInitializer
    {
        private static readonly Dictionary<HotReloadCommands, IEnvironmentCommand> RegisteredCommandInstances
            = new Dictionary<HotReloadCommands, IEnvironmentCommand>();

        private static EnvironmentService _devEnviromentService;
        private static IGuiService _guiService;
        private static ISettingsService _settingsService;

        public static void Init(EnvironmentService devEnviromentService,
            IGuiService guiService, ISettingsService settingsService)
        {
            _devEnviromentService = devEnviromentService;
            _guiService = guiService;
            _settingsService = settingsService;

            RegisteredCommandInstances.Add(HotReloadCommands.Enable, null);
            RegisteredCommandInstances.Add(HotReloadCommands.Disable, null);
        }

        public static void RegisterCommand(HotReloadCommands commandType, MacEnvironmentCommand environmentCommand)
        {
            if (RegisteredCommandInstances.ContainsKey(commandType))
            {
                RegisteredCommandInstances[commandType] = environmentCommand;
            }

            if (RegisteredCommandInstances.All((arg) => arg.Value != null))
            {
                Main.Init(_devEnviromentService, RegisteredCommandInstances, _guiService, _settingsService);
            }
        }
    }
}