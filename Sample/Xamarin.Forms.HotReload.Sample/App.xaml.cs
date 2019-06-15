using Xamarin.Forms.HotReload.Sample.Pages;
using Xamarin.Forms.HotReload.Sample.ViewModels;

namespace Xamarin.Forms.HotReload.Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
#if DEBUG
            HotReloader.Current.Run(this, new HotReloader.Configuration
            {
                //these are default values below
                DeviceUrlPort = 8000,
                ExtensionAutoDiscoveryPort = 15000
            });
#endif
            MainPage = new NavigationPage(new ItemsPage { BindingContext = new ItemsViewModel() })
            {
                BarBackgroundColor = Color.Purple,
                BarTextColor = Color.White
            };
        }
    }
}