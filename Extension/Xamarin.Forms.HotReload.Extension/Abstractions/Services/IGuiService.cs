using System.Threading.Tasks;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.Abstractions.Services
{
    public interface IGuiService
    {
        void ShowMessageBox(string title, string message);

        Task<InfoBarActionType> ShowInfoBarAsync(string message, params InfoBarAction[] infoBarActions);

        void HideInfoBar();

        bool ShowDialog<T>(object model) where T : IDialog;

        void ShowExtensionToolbar();

        void HideExtensionToolbar();
    }
}