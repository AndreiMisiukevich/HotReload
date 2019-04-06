using System.ComponentModel.Design;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Models;
using Xamarin.Forms.HotReload.Extension.WinVS.InfoBar;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Implementations
{
    public class GuiService : IGuiService
    {
        private readonly IServiceContainer _serviceContainer;
        private readonly IDependencyContainer _dialogRersolver;
        private IVsInfoBarUIElement _currentInfoBar;

        public GuiService(IServiceContainer serviceContainer, IDependencyContainer dialogRersolver)
        {
            _serviceContainer = serviceContainer;
            _dialogRersolver = dialogRersolver;
        }

        public bool ShowDialog(IDialog dialog)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var uiShell = (IVsUIShell) _serviceContainer.GetService(typeof(SVsUIShell));
            uiShell.GetDialogOwnerHwnd(out var hwnd);
            uiShell.EnableModeless(0);
            try
            {
                var dialogResult = WindowHelper.ShowModal((Window) dialog, hwnd);
                return dialogResult == 1;
            }
            finally
            {
                uiShell.EnableModeless(1);
            }
        }

        public bool ShowDialog<T>(object model) where T : IDialog
        {
            var dialog = _dialogRersolver.Resolve<T>(model);
            return ShowDialog(dialog);
        }

        public void ShowMessageBox(string title, string message)
        {
            MessageBox.Show(title, message, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task<InfoBarActionType> ShowInfoBarAsync(string message, params InfoBarAction[] infoBarActions)
        {
            var resulTaskCompletionSource = new TaskCompletionSource<InfoBarActionType>();
            var vsInfoBarUiFactory = (IVsInfoBarUIFactory) _serviceContainer.GetService(typeof(SVsInfoBarUIFactory));

            if (TryGetInfoBarData(out var vsInfoBarHost))
            {
                var infoBarModel = new HotReloadInfoBar(message);
                infoBarModel.SetInfoBarActions(infoBarActions);
                var infoBarUiElement = vsInfoBarUiFactory.CreateInfoBar(infoBarModel);
                infoBarUiElement.Advise(new InfoBarEvents(infoBarActions, resulTaskCompletionSource), out _);
                vsInfoBarHost.AddInfoBar(infoBarUiElement);
                _currentInfoBar = infoBarUiElement;
            }

            var result = await resulTaskCompletionSource.Task;
            _currentInfoBar = null;
            return result;
        }

        public void HideInfoBar()
        {
            if (_currentInfoBar != null && TryGetInfoBarData(out var _))
            {
                _currentInfoBar.Close();
            }
        }

        private bool TryGetInfoBarData(out IVsInfoBarHost infoBarHost)
        {
            infoBarHost = null;

            var vsShell = (IVsShell) _serviceContainer.GetService(typeof(SVsShell));
            if (vsShell == null ||
                ErrorHandler.Failed(vsShell.GetProperty((int) __VSSPROPID7.VSSPROPID_MainWindowInfoBarHost,
                    out var globalInfoBar)))
            {
                return false;
            }

            infoBarHost = (IVsInfoBarHost) globalInfoBar;
            return infoBarHost != null;
        }
    }
}