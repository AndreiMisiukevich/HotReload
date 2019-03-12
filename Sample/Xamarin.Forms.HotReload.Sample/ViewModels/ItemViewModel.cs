using System;
namespace Xamarin.Forms.HotReload.Sample.ViewModels
{
    public sealed class ItemViewModel
    {
        public ItemViewModel(string title, string imageUrl, string team)
        {
            Title = title;
            ImageUrl = imageUrl;
        }

        public string Title { get; }
        public string ImageUrl { get; }
    }
}
