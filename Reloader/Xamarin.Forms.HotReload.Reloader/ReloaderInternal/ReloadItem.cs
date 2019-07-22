using System.Collections.Generic;
using System.Xml;

namespace Xamarin.Forms
{
    public sealed partial class HotReloader
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

            public string CodeKey { get; set; }
            public string Code { get; set; }
        }
    }
}