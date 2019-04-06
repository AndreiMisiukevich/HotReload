using System.Linq;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.Presenters
{
    public class ConnectionsPresenter
    {
        private readonly IConnectionsDialog _connectionsDialog;
        private readonly ConnectionsConfigurationModel _model;

        public ConnectionsPresenter(IConnectionsDialog connectionsDialog, ConnectionsConfigurationModel model)
        {
            _connectionsDialog = connectionsDialog;
            _model = model;

            _connectionsDialog.ConnectionItems = model.ConnectionItems;
            _connectionsDialog.AddConnectionButtonEnabled = true;
            _connectionsDialog.IsSaveConfigurationEnabled = model.SaveConfiguration;
        }

        public bool InputValid => _model.ConnectionItems.All(item => item.IsValid);

        public void OnRemoveConnectionClicked(object connectionItem)
        {
            _model.ConnectionItems.Remove((ConnectionItem) connectionItem);
            RefreshButtonsState();
        }

        public void OnAddConnectionClicked()
        {
            _model.ConnectionItems.Add(new ConnectionItem(SharedGlobals.DefaultProtocolValue,
                SharedGlobals.DefaultIpAddressValue,
                SharedGlobals.DefaultPortValue));
            RefreshButtonsState();
        }

        public void OnConnectClicked()
        {
            _model.SaveConfiguration = _connectionsDialog.IsSaveConfigurationEnabled;
        }

        private void RefreshButtonsState()
        {
            _connectionsDialog.ConnectButtonEnabled = _model.ConnectionItems.Any();
            _connectionsDialog.AddConnectionButtonEnabled =
                _model.ConnectionItems.Count < SharedGlobals.MaxConnectionsCount;
        }
    }
}