using System.Collections.ObjectModel;

namespace Xamarin.Forms.HotReload.Sample.ViewModels
{
    public class ItemsViewModel
    {
        public ItemsViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>
            {
                new ItemViewModel("Michael Jordan", "https://statics.sportskeeda.com/editor/2018/03/a4a7b-1520474015-800.jpg", "BULLS"),
                new ItemViewModel("Scottie Pippen", "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e4/Lipofsky_Pippen.jpg/275px-Lipofsky_Pippen.jpg", "BULLS"),
                new ItemViewModel("Dennis Rodman", "https://vignette.wikia.nocookie.net/deadoralive/images/e/ec/Rodman.jpg/revision/latest?cb=20111208143535", "BULLS"),
                new ItemViewModel("Reggie Miller", "https://pbs.twimg.com/profile_images/3336677293/4282e7607066e3f19649eee5e7e7f506.jpeg", "OTHER"),
                new ItemViewModel("Karl Mallone", "http://thesource.com/wp-content/uploads/2015/02/Karl-Malone-620x480.jpg", "OTHER"),
            };
        }

        public ObservableCollection<ItemViewModel> Items { get; }
    }
}
