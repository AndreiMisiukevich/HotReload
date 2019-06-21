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
            HotReloader.Current.Run(this);
#endif
            MainPage = new NavigationPage(new ItemsPage { BindingContext = new ItemsViewModel() })
            {
                BarBackgroundColor = Color.Purple,
                BarTextColor = Color.White
            };
        }
    }
}