using System.Collections.Generic;
using System.Xml;

namespace Xamarin.Forms.HotReload.Reloader
{
    internal sealed class ReloadItem
    {
        public ReloadItem()
        {
            Objects = new HashSet<object>();
            Xaml = new XmlDocument();
        }

        public XmlDocument Xaml { get; set; }
        public HashSet<object> Objects { get; }
        public bool HasUpdates { get; set; }
        public string Css { get; set; }
    }
}