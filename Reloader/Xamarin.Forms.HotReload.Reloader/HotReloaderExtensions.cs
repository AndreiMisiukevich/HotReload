using System;

namespace Xamarin.Forms
{
    public static class HotReloaderExtensions
    {
        [Obsolete("THIS METHOD IS OBSOLETE. PLEASE, USE InitComponent INSTEAD")]
        public static void InitializeElement(this Element element)
        => InitComponent(element);

        public static void InitComponent(this Element element, Action defaultInitializer = null)
        => HotReloader.Current.InitComponent(element, defaultInitializer);
    }
}