using System.Collections;

namespace Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs
{
    public interface IConnectionsDialog : IDialog
    {
        bool AddConnectionButtonEnabled { get; set; }

        IEnumerable ConnectionItems { get; set; }

        bool IsSaveConfigurationEnabled { get; set; }

        bool ConnectButtonEnabled { get; set; }
    }
}