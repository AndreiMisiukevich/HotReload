using System;

using Xamarin.Forms;

namespace Xamarin.Forms.HotReload.Sample.Pages.Views
{
    public class CustomContentView : ContentView
    {
        public CustomContentView()
        {
            Margin = new Thickness(50);
            BackgroundColor = Color.Yellow; 
            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.FillAndExpand;

            Content = new Label
            {
                Text = "HI Xamarin!",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
        }
    }
}

