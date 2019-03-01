using System.Collections.Generic;

namespace Xamarin.Forms.HotReload.Sample
{
    public partial class SecondPage : ContentPage
    {
        public SecondPage()
        {
            InitializeComponent();
            BindingContext = new SecondViewModel();
        }
    }

    public class SecondViewModel
    {
        public List<string> Items { get; } = new List<string>
        {
            "First",
            "Second",
            "Third"
        };
    }
}
