using System.Collections.Generic;

namespace Xamarin.Forms.HotReload.Reloader
{
    internal sealed class ReloadItem
    {
        public ReloadItem()
        {
            Elements = new HashSet<ElementEntry>();
            Xaml = string.Empty;
        }

        public string Xaml { get; set; }
        public HashSet<ElementEntry> Elements { get; }
    }
}
