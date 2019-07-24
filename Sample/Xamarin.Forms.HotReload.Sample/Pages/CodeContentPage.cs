
using Xamarin.Forms.HotReload.Sample.ViewModels;

namespace Xamarin.Forms.HotReload.Sample.Pages
{
    [HotReloader.CSharp]
    public class CodeContentPage : ContentPage
    {
        public CodeContentPage()
        {
            BackgroundColor = Color.Green;
            Content = new StackLayout
            {
                BackgroundColor = Color.White,
                Margin = 50,
                Children =
                {
                    new Button
                    {
                        FontSize = 40,
                        Text = "Text!"
                    }
                }
            };

            this.SetBinding(TitleProperty, "Title");
        }
    }
}
