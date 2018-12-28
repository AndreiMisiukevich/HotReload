using Xamarin.Forms.HotReload;

namespace Xamarin.Forms
{
    public static class HotReloaderExtensions
    {
        public static void InitializeElement(this Element element) 
        => HotReloader.Current.InitializeElement(element);
    }
}