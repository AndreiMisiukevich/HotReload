using System;
using Gtk;
using Xamarin.Forms.HotReload.Extension.MacVS.Controls;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Dialogs.Connections
{
    public class ConnectionItemView : HBox
    {
        public event EventHandler RemoveItemClicked;

        private ComboBox _protocolComboBox;
        private IpEntry _ipEntry;
        private Entry _portEntry;

        public ConnectionItemView(ConnectionItem connectionItemModel)
        {
            Model = connectionItemModel;
            SetupWidgets(SharedGlobals.DefaultAvailableProtocolsValue, connectionItemModel.Protocol,
                connectionItemModel.IpAddress, connectionItemModel.Port);
        }

        public ConnectionItem Model { get; }

        private void OnRemoveButtonClicked(object sender, EventArgs e)
        {
            RemoveItemClicked?.Invoke(this, e);
        }

        private void SetupWidgets(string[] availableProtocols, string protocol, string address, string port)
        {
            _protocolComboBox = new ComboBox(availableProtocols)
            {
                WidthRequest = 70
            };
            _ipEntry = new IpEntry
            {
                WidthChars = 30,
                WidthRequest = 210
            };
            _portEntry = new Entry
            {
                WidthRequest = 60,
                WidthChars = 5
            };
            var removeButton = new Button {Label = "-"};
            removeButton.WidthRequest = 30;
            removeButton.Clicked += OnRemoveButtonClicked;

            Add(_protocolComboBox);
            Add(new Label {Text = TextResources.MainDialog_ProtocolPostfix});
            Add(_ipEntry);
            Add(new Label {Text = TextResources.MainDialog_IpAddressPostfix});
            Add(_portEntry);
            Add(removeButton);

            _ipEntry.Text = address;
            _portEntry.Text = port;
            _protocolComboBox.Active = availableProtocols.IndexOf(protocol);

            _protocolComboBox.Changed += OnProtocolComboboxChanged;
            _ipEntry.Changed += OnIpEntryChanged;
            _portEntry.Changed += OnPortEntryChanged;
        }

        private void OnPortEntryChanged(object sender, EventArgs e)
        {
            Model.Port = _portEntry.Text;
        }

        private void OnProtocolComboboxChanged(object sender, EventArgs e)
        {
            Model.Protocol = _protocolComboBox.ActiveText;
        }

        private void OnIpEntryChanged(object sender, EventArgs e)
        {
            Model.IpAddress = _ipEntry.Text;
        }
    }
}