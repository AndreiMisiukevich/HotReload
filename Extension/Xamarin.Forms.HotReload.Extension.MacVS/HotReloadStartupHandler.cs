using MonoDevelop.Components.Commands;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.Helpers;
using Xamarin.Forms.HotReload.Extension.MacVS;
using Xamarin.Forms.HotReload.Extension.MacVS.Dialogs.Connections;
using Xamarin.Forms.HotReload.Extension.MacVS.Implementations;

public class HotReloadStartupHandler : CommandHandler
{
    protected override void Run()
    {
        base.Run();

        var dependencyContainer = new DependencyContainer(new DependenciesRegistrar());

        var enviromentEvents = new MacEnvironmentService();
        var guiPresenter = new GuiService(dependencyContainer);
        var settingsPersistense = new SettingsService();

        MacExtensionInitializer.Init(enviromentEvents, guiPresenter, settingsPersistense);
    }
}