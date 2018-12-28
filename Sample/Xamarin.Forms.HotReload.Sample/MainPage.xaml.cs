#if DEBUG
using Xamarin.Forms.HotReload.Reloader;
#endif

using System.Windows.Input;

namespace Xamarin.Forms.HotReload.Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
#if DEBUG
            this.InitializeElement();
#else
            InitializeComponent();
#endif
            BindingContext = new MainViewModel();
            PushCommand = new Command(() => Navigation.PushAsync(new SecondPage()));
        }

        public ICommand PushCommand { get; }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            Btn.Text = (int.Parse(Btn.Text) + 1).ToString();
        }

    }

    public class MainViewModel
    {
        public MainViewModel()
        {
            PushCommand = new Command(() => Application.Current.MainPage.Navigation.PushAsync(new SecondPage()));
        }

        public ICommand PushCommand { get; }
    }
}
