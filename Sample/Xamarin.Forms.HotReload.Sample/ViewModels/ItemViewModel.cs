
namespace Xamarin.Forms.HotReload.Sample.ViewModels
{
    public sealed class ItemViewModel
    {
        public ItemViewModel(string title, string imageUrl, string team, string description = null)
        {
            Title = title;
            ImageUrl = imageUrl;
            Team = team;
            Description = description ?? "No info";
        }

        public string Title { get; }
        public string ImageUrl { get; }
        public string Team { get; }
        public string Description { get; }
    }
}
