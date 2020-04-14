using System;

using Xamarin.Forms;
using Xamarin.Forms.HotReload.Sample.Pages.Views;

namespace Xamarin.Forms.HotReload.Sample.Pages
{
    public class CodePage : ContentPage
    {
        public CodePage()
        {
            BackgroundColor = Color.Red;
            Content = new StackLayout
            {
                Children =
                {
                    new CustomContentView(),
                    new Button { Text = "Click", BackgroundColor = Color.White }
                }
            };
        }
    }
}

