namespace Xamarin.Forms.HotReload.Sample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
#if DEBUG
            HotReloader.Current.Start(this);
#endif
            MainPage = new NavigationPage(new MainPage());
        }
    }
}