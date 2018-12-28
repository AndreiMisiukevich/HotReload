namespace Xamarin.Forms.HotReload.Reloader
{
    public static class HotReloaderExtensions
    {
        public static void InitializeElement(this Element element) 
        => HotReloader.Current.InitializeElement(element);
    }
}