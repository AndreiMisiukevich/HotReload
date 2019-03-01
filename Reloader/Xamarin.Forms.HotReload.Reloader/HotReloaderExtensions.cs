using System;
using Xamarin.Forms.HotReload.Reloader;

namespace Xamarin.Forms
{
    [Obsolete]
    public static class HotReloaderExtensions
    {
        [Obsolete("This method is obsolete and will be removed soon.")]
        public static void InitComponent(this Element element, Action defaultInitializer = null)
        => HotReloader.Current.InitComponent(element, defaultInitializer);


        [Obsolete("This method is obsolete and will be removed soon.")]
        public static void InitComponent(this HotReloader reloader, Element element, Action defaultInitializer = null)
        {
        }

        [Obsolete("This method is obsolete and will be removed soon.")]
        public static void Start(this HotReloader reloader, string ip, int port)
            => HotReloader.Current.Start(null, port);

        [Obsolete("This method is obsolete and will be removed soon.")]
        public static void Start(this HotReloader reloader, string url)
        {
            var uri = new Uri(url);
            var scheme = uri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase)
                ? ReloaderScheme.Https
                : ReloaderScheme.Http;
            HotReloader.Current.Start(null, uri.Port, scheme);
        }
    }
}