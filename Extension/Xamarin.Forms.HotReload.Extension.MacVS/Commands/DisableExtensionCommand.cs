using Xamarin.Forms.HotReload.Extension.Enums;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Commands
{
    public class DisableExtensionCommand : MacEnvironmentCommand
    {
        public DisableExtensionCommand() : base(HotReloadCommands.Disable)
        {
        }
    }
}