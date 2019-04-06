using System;
using MonoDevelop.Components.Commands;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Enums;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Commands
{
    public class MacEnvironmentCommand : CommandHandler, IEnvironmentCommand
    {
        public event EventHandler Executed;

        protected bool IsVisibleFallback;
        protected bool IsEnabledFallback;
        private CommandInfo _commandInfo;

        public MacEnvironmentCommand(HotReloadCommands commandType)
        {
            MacExtensionInitializer.RegisterCommand(commandType, this);
        }

        public bool IsVisible
        {
            get => _commandInfo == null ? IsVisibleFallback : _commandInfo.Visible;
            set
            {
                if (_commandInfo == null)
                {
                    IsVisibleFallback = value;
                }
                else
                {
                    _commandInfo.Visible = value;
                }
            }
        }

        public bool IsEnabled
        {
            get => _commandInfo == null ? IsEnabledFallback : _commandInfo.Enabled;
            set
            {
                if (_commandInfo == null)
                {
                    IsEnabledFallback = value;
                }
                else
                {
                    _commandInfo.Enabled = value;
                }
            }
        }

        protected override void Run()
        {
            OnExecuted();
        }

        protected override void Update(CommandInfo info)
        {
            if (_commandInfo == null)
            {
                info.Visible = IsVisibleFallback;
                info.Enabled = IsEnabledFallback;
            }
            else
            {
                info.Visible = _commandInfo.Visible;
                info.Enabled = _commandInfo.Enabled;
            }

            _commandInfo = info;
        }

        protected virtual void OnExecuted()
        {
            Executed?.Invoke(this, EventArgs.Empty);
        }
    }
}