using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.Models;
using Xamarin.Forms.HotReload.Extension.Presenters;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Dialogs.Connections
{
    public partial class ConnectionsDialog : IConnectionsDialog
    {
        private readonly ConnectionsPresenter _presenter;

        public ConnectionsDialog(ConnectionsConfigurationModel model)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            _presenter = new ConnectionsPresenter(this, model);
        }

        public bool AddConnectionButtonEnabled
        {
            get => AddConnectionButton.IsEnabled;
            set => AddConnectionButton.IsEnabled = value;
        }

        public IEnumerable ConnectionItems
        {
            get => ConnectionItemsListView.ItemsSource;
            set => ConnectionItemsListView.ItemsSource = value;
        }

        public bool IsSaveConfigurationEnabled
        {
            get => SaveConfigurationCheckBox.IsChecked ?? false;
            set => SaveConfigurationCheckBox.IsChecked = value;
        }

        public bool ConnectButtonEnabled
        {
            get => ConnectButton.IsEnabled;
            set => ConnectButton.IsEnabled = value;
        }

        private void OnAddConnectionClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            _presenter.OnAddConnectionClicked();
        }

        private void OnRemoveConnectionClicked(object sender, RoutedEventArgs e)
        {
            var connectionItem = ((FrameworkElement) e.OriginalSource).DataContext;
            _presenter.OnRemoveConnectionClicked(connectionItem);
        }

        private void OnConnectButtonClicked(object sender, RoutedEventArgs e)
        {
            _presenter.OnConnectClicked();
            DialogResult = true;
            Close();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConnectionItemsListView.SelectedItem != null)
            {
                ConnectionItemsListView.SelectedItem = null;
            }
        }

        private void OnPortTextChanged(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OnHyperlinkClicked(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}