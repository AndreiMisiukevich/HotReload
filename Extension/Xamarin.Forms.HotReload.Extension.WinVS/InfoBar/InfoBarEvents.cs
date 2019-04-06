using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.WinVS.InfoBar
{
    public class InfoBarEvents : IVsInfoBarUIEvents
    {
        private readonly InfoBarAction[] _infoBarActions;
        private readonly TaskCompletionSource<InfoBarActionType> _resulTaskCompletionSource;

        public InfoBarEvents(InfoBarAction[] infoBarActions,
            TaskCompletionSource<InfoBarActionType> resulTaskCompletionSource)
        {
            _infoBarActions = infoBarActions;
            _resulTaskCompletionSource = resulTaskCompletionSource;
        }

        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            var item = _infoBarActions.FirstOrDefault(i => i.Title == actionItem.Text);
            if (item != null)
            {
                _resulTaskCompletionSource.SetResult(item.Type);
                infoBarUIElement.Close();
            }
        }

        public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
            if (!_resulTaskCompletionSource.Task.IsCompleted)
            {
                _resulTaskCompletionSource.SetResult(InfoBarActionType.NoAction);
            }
        }
    }
}