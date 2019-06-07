using System.Collections.Generic;
using JetBrains.ProjectModel;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Rider.Implementations;

namespace Xamarin.Forms.HotReload.Extension.Rider
{
    [SolutionComponent]
    public class PluginInitializer
    {
     
        private static readonly Dictionary<HotReloadCommands, IEnvironmentCommand> RegisteredCommandInstances = new Dictionary<HotReloadCommands, IEnvironmentCommand>();

        public PluginInitializer(RiderEnvironmentService environmentService, IGuiService service, ISettingsService settingsStore)
        {
            if (!RegisteredCommandInstances.ContainsKey(HotReloadCommands.Enable))
            {
                RegisteredCommandInstances.Add(HotReloadCommands.Enable, new EnvCommandStud());
            }

            if (!RegisteredCommandInstances.ContainsKey(HotReloadCommands.Disable))
            {
                RegisteredCommandInstances.Add(HotReloadCommands.Disable, new EnvCommandStud());
            }

            Main.Init(environmentService, RegisteredCommandInstances, service, settingsStore);

            var enableCommand = RegisteredCommandInstances[HotReloadCommands.Enable] as EnvCommandStud;
            enableCommand.OnExecuted();
        }
    }
}