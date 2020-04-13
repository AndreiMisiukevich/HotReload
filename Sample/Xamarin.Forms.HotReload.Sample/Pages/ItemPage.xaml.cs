namespace Xamarin.Forms.HotReload.Sample.Pages
{
    public partial class ItemPage : ContentPage
    {
        public ItemPage()
        {
            InitializeComponent();
            BackgroundColor = Color.White;
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new CodePage());
        }
    }
}
