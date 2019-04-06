using System.Threading.Tasks;
using Gtk;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Helpers;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Implementations
{
    public class GuiService : IGuiService
    {
        private readonly DependencyContainer _dependencyContainer;

        public GuiService(DependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public bool ShowDialog<T>(object model) where T : IDialog
        {
            var dialog = (Dialog) (object) _dependencyContainer.Resolve<T>(model);
            var intResult = MonoDevelop.Ide.MessageService.ShowCustomDialog(dialog);
            return intResult == (int) ResponseType.Ok;
        }

        public void ShowMessageBox(string title, string message)
        {
            MonoDevelop.Ide.MessageService.ShowWarning(message, title);
        }

        public Task<InfoBarActionType> ShowInfoBarAsync(string message, params InfoBarAction[] infoBarActions)
        {
            // No implementation on mac
            return Task.FromResult(InfoBarActionType.NoAction);
        }

        public void HideInfoBar()
        {
            // No implementation on mac
        }
    }
}