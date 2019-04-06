using System;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.WinVS.Dialogs.Connections;

namespace Xamarin.Forms.HotReload.Extension.WinVS
{
    public class DependenciesRegistrar : IDependenciesRegistrar
    {
        public void Register(IDependencyContainer container)
        {
            //Do all Win VS registrations here.
            container.Register<IConnectionsDialog, ConnectionsDialog>();
        }
    }
}