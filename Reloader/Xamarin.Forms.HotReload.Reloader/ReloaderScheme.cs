using System;
using System.ComponentModel;

namespace Xamarin.Forms.HotReload.Reloader
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use HotReloader.Scheme instead")]
    public enum ReloaderScheme
    {
        Http,
        Https
    }
}
