using System.Threading.Tasks;
using JetBrains.ProjectModel;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.Rider.Implementations
{
    [SolutionComponent]
    public class GuiServiceStub: IGuiService
    {
        public void ShowMessageBox(string title, string message)
        {
        }

        public Task<InfoBarActionType> ShowInfoBarAsync(string message, params InfoBarAction[] infoBarActions)
        {
            return Task.FromResult(InfoBarActionType.NoAction);
        }

        public void HideInfoBar()
        {
        }

        public bool ShowDialog<T>(object model) where T : IDialog
        {
            return true;
        }

        public void ShowExtensionToolbar()
        {
        }

        public void HideExtensionToolbar()
        {
        }
    }
}