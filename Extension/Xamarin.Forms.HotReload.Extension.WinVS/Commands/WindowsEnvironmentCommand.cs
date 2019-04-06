using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Enums;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Commands
{
    public class WindowsEnvironmentCommand : IEnvironmentCommand
    {
        public HotReloadCommands CommandType { get; }
        public event EventHandler Executed;

        public bool IsVisible
        {
            get => MenuItem.Visible;
            set => MenuItem.Visible = value;
        }

        public bool IsEnabled
        {
            get => MenuItem.Enabled;
            set => MenuItem.Enabled = value;
        }

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        protected readonly AsyncPackage Package;

        protected WindowsEnvironmentCommand(HotReloadCommands commandType, AsyncPackage package,
            OleMenuCommandService commandService, Guid commandSet, int commandId)
        {
            Package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(commandSet, commandId);
            MenuItem = new OleMenuCommand(Execute, menuCommandId);
            commandService.AddCommand(MenuItem);
            CommandType = commandType;
        }

        protected OleMenuCommand MenuItem { get; set; }

        private void OnExecuted()
        {
            Executed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        protected async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(Package.DisposalToken);
            OnExecuted();
        }
    }
}