namespace Xamarin.Forms.HotReload.Sample
{
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            HotReloader.Current.Start();
#endif
            this.InitComponent(InitializeComponent);
            MainPage = new NavigationPage(new MainPage());
        }
    }
}