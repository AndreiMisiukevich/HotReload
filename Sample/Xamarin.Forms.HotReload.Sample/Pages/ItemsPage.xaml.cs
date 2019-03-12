namespace Xamarin.Forms.HotReload.Sample.Pages
{
    public partial class ItemsPage : ContentPage
    {
        public ItemsPage()
        {
            InitializeComponent();
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
            => Navigation.PushAsync(new ItemPage { BindingContext = e.Item });
    }
}
