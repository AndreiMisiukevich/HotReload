using System;

namespace Xamarin.Forms.HotReload.Extension.Abstractions
{
    public interface IEnvironmentCommand
    {
        event EventHandler Executed;

        bool IsVisible { get; set; }

        bool IsEnabled { get; set; }
    }
}