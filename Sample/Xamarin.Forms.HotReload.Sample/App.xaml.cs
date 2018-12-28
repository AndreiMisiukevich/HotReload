namespace Xamarin.Forms.HotReload.Sample
{
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            HotReloader.Current.Start();
            this.InitializeElement();
#else
            InitializeComponent();
#endif
            MainPage = new NavigationPage(new MainPage());
        }
    }
}