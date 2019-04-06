using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.HotReload.Extension.Models
{
    public class ConnectionsConfigurationModel
    {
        public ConnectionsConfigurationModel(List<ConnectionItem> connectionItems, bool saveConfiguration)
        {
            SaveConfiguration = saveConfiguration;
            ConnectionItems = new ObservableCollection<ConnectionItem>(connectionItems);
        }

        public bool SaveConfiguration { get; set; }

        public ObservableCollection<ConnectionItem> ConnectionItems { get; set; }
    }
}