
namespace Xamarin.Forms.HotReload.Sample.Pages
{
    public class CodeContentPage : ContentPage
    {
        public CodeContentPage()
        {
            BackgroundColor = Color.Red;
            Content = new StackLayout
            {
                BackgroundColor = Color.Sienna,
                Margin = 50
            };

            this.SetBinding(TitleProperty, "Title");
        }
    }
}
