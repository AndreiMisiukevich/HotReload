using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Xamarin.Forms.HotReload.Extension.Enums;
using Task = System.Threading.Tasks.Task;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DisableExtensionCommand : WindowsEnvironmentCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("1865df49-3912-4ac5-b532-3018ad5e3109");

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableExtensionCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private DisableExtensionCommand(AsyncPackage package, OleMenuCommandService commandService) : base(
            HotReloadCommands.Disable, package, commandService, CommandSet, CommandId)
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DisableExtensionCommand Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in DisableExtensionCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new DisableExtensionCommand(package, commandService);
        }
    }
}