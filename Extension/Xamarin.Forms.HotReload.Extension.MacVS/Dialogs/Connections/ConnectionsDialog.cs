using System;
using System.Collections;
using Gdk;
using Gtk;
using MonoDevelop.Ide;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.MacVS.Controls;
using Xamarin.Forms.HotReload.Extension.Models;
using Xamarin.Forms.HotReload.Extension.Presenters;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Dialogs.Connections
{
    public class ConnectionsDialog : Dialog, IConnectionsDialog
    {
        private readonly ConnectionsPresenter _connectionsPresenter;
        private CheckButton _saveConfigurationCheckbox;
        private Button _connectButton;
        private Button _addConnectionItemButton;
        private ConnectionsListView _connectionsListView;

        public ConnectionsDialog(ConnectionsConfigurationModel model)
        {
            Title = TextResources.MainDialog_Title;
            Modal = true;
            AllowGrow = false;
            AllowShrink = false;
            Parent = IdeApp.Workbench.RootWindow;
            TransientFor = IdeApp.Workbench.RootWindow;
            ParentWindow = IdeApp.Workbench.RootWindow.GdkWindow;

            SetPosition(WindowPosition.CenterOnParent);
            SetupWidgets();
            ShowAll();

            _connectionsPresenter = new ConnectionsPresenter(this, model);
        }

        public bool AddConnectionButtonEnabled
        {
            get => _addConnectionItemButton.Sensitive;
            set => _addConnectionItemButton.Sensitive = value;
        }

        public IEnumerable ConnectionItems
        {
            get => _connectionsListView.Items;
            set => _connectionsListView.Items = value;
        }

        public bool IsSaveConfigurationEnabled
        {
            get => _saveConfigurationCheckbox.Active;
            set => _saveConfigurationCheckbox.Active = value;
        }

        public bool ConnectButtonEnabled
        {
            get => _connectButton.Sensitive;
            set => _connectButton.Sensitive = value;
        }

        private void OnAddConnectionItemClicked(object sender, EventArgs e)
        {
            _connectionsPresenter.OnAddConnectionClicked();
        }

        private void OnConnectButtonClicked(object sender, EventArgs e)
        {
            if (_connectionsPresenter.InputValid)
            {
                _connectionsPresenter.OnConnectClicked();
                Respond(ResponseType.Ok);
                Destroy();
            }
            else
            {
                MessageService.ShowWarning(TextResources.MainDialog_IpIncorrect, TextResources.Global_Warning);
            }
        }

        private void OnConnectionsListViewItemRemoved(object sender, object e)
        {
            _connectionsPresenter.OnRemoveConnectionClicked((ConnectionItem) e);
        }

        private void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            System.Diagnostics.Process.Start(TextResources.MainDialog_MoreInfo_Url);
        }

        private void SetupWidgets()
        {
            var titleHBox = new HBox();
            var controlPanelHBox = new HBox();
            var savePrefsHBox = new HBox();
            var helpInfoHBox = new HBox();
            var connectHBox = new HBox();

            var titleLabel = new Label()
            {
                Text = TextResources.MainDialog_EnterIp
            };
            titleHBox.Add(titleLabel);

            _addConnectionItemButton = new Button()
            {
                Label = "+",
                WidthRequest = 30
            };
            _addConnectionItemButton.Clicked += OnAddConnectionItemClicked;
            controlPanelHBox.PackStart(_addConnectionItemButton, false, false, 0);
            controlPanelHBox.PackEnd(new Alignment(0, 0, 1, 1), true, true, 0);

            savePrefsHBox.PackStart(new Alignment(0.5f, 0, 1, 1));
            _saveConfigurationCheckbox = new CheckButton()
            {
                Label = TextResources.MainDialog_SaveConfiguration
            };
            savePrefsHBox.Add(_saveConfigurationCheckbox);

            var helpInfoLabel0 = new Label()
            {
                Text = TextResources.MainDialog_MoreInfo_0
            };
            var helpInfoEventBox = new EventBox();
            var helpInfoLabel1 = new Label()
            {
                Text = TextResources.MainDialog_MoreInfo_1,
                UseUnderline = true
            };
            Color color = Color.Zero;
            Color.Parse("#0645AD", ref color);
            helpInfoLabel1.ModifyFg(StateType.Normal, color);
            helpInfoEventBox.Add(helpInfoLabel1);
            helpInfoEventBox.ButtonReleaseEvent += OnButtonReleaseEvent;

            var helpInfoLabel2 = new Label()
            {
                Text = TextResources.MainDialog_MoreInfo_2
            };

            helpInfoHBox.PackStart(new Alignment(0, 0, 1, 1) {WidthRequest = 40}, true, true, 0);
            helpInfoHBox.Add(helpInfoLabel0);
            helpInfoHBox.Add(helpInfoEventBox);
            helpInfoHBox.Add(helpInfoLabel2);
            helpInfoHBox.PackEnd(new Alignment(0, 0, 1, 1) {WidthRequest = 40}, true, true, 0);

            _connectButton = new Button()
            {
                Label = TextResources.MainDialog_Connect,
                WidthRequest = 140
            };

            _connectButton.Clicked += OnConnectButtonClicked;
            connectHBox.PackStart(new Alignment(0, 0, 1, 1));
            connectHBox.Add(_connectButton);
            connectHBox.PackEnd(new Alignment(0, 0, 1, 1));

            var mainVBox = new VBox()
            {
                Spacing = 10
            };
            mainVBox.Add(titleHBox);
            mainVBox.Add(controlPanelHBox);

            _connectionsListView = new ConnectionsListView()
            {
                WidthRequest = 400,
                MaxVisibleItemsCount = 6,
                RowHeight = 30
            };
            _connectionsListView.ItemRemoved += OnConnectionsListViewItemRemoved;
            mainVBox.Add(_connectionsListView);
            mainVBox.Add(savePrefsHBox);
            mainVBox.Add(helpInfoHBox);
            mainVBox.Add(connectHBox);

            AddActionWidget(mainVBox, ResponseType.Ok);
        }
    }
}