
#if !DEBUG
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
#endif

namespace Xamarin.Forms.HotReload.Sample
{
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            HotReloader.Current.Start();
#endif
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
