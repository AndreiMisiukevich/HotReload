using System;

using Xamarin.Forms;

namespace Xamarin.Forms.HotReload.Sample.Pages
{
    public class CodePage : ContentPage
    {
        public CodePage()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label {
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        Text = "Hello ContentPage!!!!!"
                    }
                }
            };
        }
    }
}

