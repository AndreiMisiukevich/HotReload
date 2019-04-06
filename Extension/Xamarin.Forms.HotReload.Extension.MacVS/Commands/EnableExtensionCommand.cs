using System.Linq;
using MonoDevelop.Ide;
using Xamarin.Forms.HotReload.Extension.Enums;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Commands
{
    public class EnableExtensionCommand : MacEnvironmentCommand
    {
        public EnableExtensionCommand() : base(HotReloadCommands.Enable)
        {
            // Workaround of "Enable" command visibility issue.
            IsVisibleFallback = IdeApp.Workspace.GetAllSolutions().Any();
            IsEnabledFallback = true;
        }
    }
}