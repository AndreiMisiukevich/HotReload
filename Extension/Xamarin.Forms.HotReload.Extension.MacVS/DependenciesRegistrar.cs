using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.MacVS.Dialogs.Connections;

namespace Xamarin.Forms.HotReload.Extension.MacVS
{
    public class DependenciesRegistrar : IDependenciesRegistrar
    {
        public void Register(IDependencyContainer container)
        {
            //Place all the registrations here.
            container.Register<IConnectionsDialog, ConnectionsDialog>();
        }
    }
}
