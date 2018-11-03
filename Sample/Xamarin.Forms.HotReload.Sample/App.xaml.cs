using Xamarin.Forms.Xaml;

#if !DEBUG
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
