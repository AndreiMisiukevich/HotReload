using System;

namespace Xamarin.Forms
{
    public static class HotReloaderExtensions
    {
        public static void InitComponent(this Element element, Action defaultInitializer = null)
        => HotReloader.Current.InitComponent(element, defaultInitializer);
    }
}