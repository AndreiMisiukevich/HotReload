using Xamarin.Forms.HotReload.Sample.ViewModels;

namespace Xamarin.Forms.HotReload.Sample.Pages
{
    public partial class ItemsPage : ContentPage
    {
        public ItemsPage()
        {
            InitializeComponent();
        }

        void OnItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            Navigation.PushAsync(new ItemPage { BindingContext = e.Item });
        }

        void OnCodePageTapped(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new CodeContentPage(Color.Purple) { BindingContext = new CodeContentViewModel() });
        }
    }
}
