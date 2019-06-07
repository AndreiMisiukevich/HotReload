using System;
using Xamarin.Forms.HotReload.Extension.Abstractions;

namespace Xamarin.Forms.HotReload.Extension.Rider.Implementations
{
    public class EnvCommandStud : IEnvironmentCommand
    {
        public event EventHandler Executed;
        public bool IsVisible { get; set; }
        public bool IsEnabled { get; set; }

        public void OnExecuted()
        {
            Executed?.Invoke(this, EventArgs.Empty);
        }
    }
}