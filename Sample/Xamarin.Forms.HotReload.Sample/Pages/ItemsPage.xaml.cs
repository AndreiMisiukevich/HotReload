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
    }
}
