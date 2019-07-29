using System;
namespace Xamarin.Forms.HotReload.Sample.ViewModels
{
    public class CodeContentViewModel
    {
        public string Title { get; set; }

        public CodeContentViewModel()
        {
            Title = "I am very limited, but reloadable";
        }

        public string ButtonText { get; set; } = "Click Me";
    }
}
