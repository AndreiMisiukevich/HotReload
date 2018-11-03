
namespace Xamarin.Forms.HotReload.Reloader
{
    public static class HotReloaderExtensions
    {
        public static void InitializeElement<TXaml>(this TXaml element) where TXaml : Element
        => HotReloader.Current.InitializeElement(element);
    }
}
