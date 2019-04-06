using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Gtk;
using Xamarin.Forms.HotReload.Extension.MacVS.Dialogs.Connections;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Controls
{
    public class ConnectionsListView : ScrolledWindow
    {
        public event EventHandler<object> ItemRemoved;

        private readonly Container _connectionsContainer;
        private ObservableCollection<ConnectionItem> _items;

        public ConnectionsListView()
        {
            _connectionsContainer = new VBox();
            AddWithViewport(_connectionsContainer);
            SetPolicy(PolicyType.Never, PolicyType.Automatic);
        }

        public int RowHeight { get; set; }

        public int MaxVisibleItemsCount { get; set; }

        public IEnumerable Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    if (_items != null)
                    {
                        _items.CollectionChanged -= OnItemsObservableCollectionChanged;
                    }

                    _items = (ObservableCollection<ConnectionItem>) value;
                    _items.CollectionChanged += OnItemsObservableCollectionChanged;
                    foreach (var item in value)
                    {
                        AddViewItem(item);
                    }

                    AdjustHeight();
                }
            }
        }

        private void OnItemsObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddViewItem(e.NewItems[0]);
                    AdjustHeight();
                    break;
            }
        }

        private void AddViewItem(object item)
        {
            var itemView = new ConnectionItemView((ConnectionItem) item)
            {
                HeightRequest = RowHeight
            };
            itemView.RemoveItemClicked += ItemView_RemoveItemClicked;
            _connectionsContainer.Add(itemView);
            itemView.ShowAll();
        }

        private void ItemView_RemoveItemClicked(object sender, EventArgs e)
        {
            var viewItem = ((ConnectionItemView) sender);
            _connectionsContainer.Remove(viewItem);
            viewItem.RemoveItemClicked -= ItemView_RemoveItemClicked;
            AdjustHeight();
            OnItemRemoved(viewItem.Model);
        }

        private void AdjustHeight()
        {
            var connectionItemsWidgentsCount = _connectionsContainer.Children.Count();
            if (connectionItemsWidgentsCount == 0)
            {
                HeightRequest = RowHeight;
            }
            else if (connectionItemsWidgentsCount <= MaxVisibleItemsCount)
            {
                HeightRequest = connectionItemsWidgentsCount * RowHeight;
            }
            else
            {
                HeightRequest = MaxVisibleItemsCount * RowHeight;
            }
        }

        private void OnItemRemoved(object removedItem)
        {
            ItemRemoved?.Invoke(this, removedItem);
        }
    }
}