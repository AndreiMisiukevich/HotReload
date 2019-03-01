using System.Collections.Generic;

namespace Xamarin.Forms.HotReload.Reloader
{
    internal sealed class ReloadItem
    {
        public ReloadItem()
        {
            Objects = new HashSet<object>();
            Xaml = string.Empty;
        }

        public string Xaml { get; set; }
        public HashSet<object> Objects { get; }
    }
}