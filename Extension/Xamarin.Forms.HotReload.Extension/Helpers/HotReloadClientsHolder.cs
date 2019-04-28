using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.Helpers
{
    internal class HotReloadClientsHolder
    {
        private readonly Dictionary<ConnectionItem, HotReloadClient> _hotReloadClients =
            new Dictionary<ConnectionItem, HotReloadClient>();

        internal void Init(List<ConnectionItem> connectionItems)
        {
            _hotReloadClients.Clear();
            foreach (var connectionItem in connectionItems)
            {
                var hotReloadClient = new HotReloadClient();
                _hotReloadClients.Add(connectionItem, hotReloadClient);
                hotReloadClient.SetBaseAddress(connectionItem.FullAddress);
            }
        }

        internal Task UpdateResourceAsync(string pathString, string contentString)
        {
            var establishConnectionTasks = new Task[_hotReloadClients.Count];

            for (int i = 0; i < _hotReloadClients.Count; i++)
            {
                var connectionItem = _hotReloadClients.ElementAt(i);
                establishConnectionTasks[i] = connectionItem.Value.PostResourceUpdateAsync(pathString, contentString);
            }

            return Task.Run(() => Task.WaitAll(establishConnectionTasks));
        }
    }
}