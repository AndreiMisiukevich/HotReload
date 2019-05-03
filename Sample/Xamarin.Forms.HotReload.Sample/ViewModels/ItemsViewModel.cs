using System.Collections.ObjectModel;

namespace Xamarin.Forms.HotReload.Sample.ViewModels
{
    public class ItemsViewModel
    {
        public ItemsViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>
            {
                new ItemViewModel("Thanos", "https://www.sideshow.com/product-asset/903429/feature", "The Mad Titan", @"Thanos was born on Saturn's moon Titan as the son of Eternals A'lars and Sui-San; his brother is Eros of Titan. Thanos carries the Deviants gene, and as such, shares the physical appearance of the Eternals' cousin race. Shocked by his appearance and the belief that he would destroy all life in the universe, Sui-San attempted to kill him, but she was stopped by A'lars. During his school years, Thanos was a pacifist and would only play with his brother Eros and pets. By adolescence, Thanos had become fascinated with nihilism and death, worshipping and eventually falling in love with the physical embodiment of death, Mistress Death. As an adult, Thanos augmented his physical strength and powers through his superior scientific knowledge. He also attempted to create a new life for himself by siring many children as well as becoming a pirate. He finds no fulfillment in either until he is visited again by Mistress Death, for whom he murders his offspring and his pirate captain"),
                new ItemViewModel("Tony Stark", "https://nerdist.com/wp-content/uploads/2018/05/Tony-Stark-1.png", "Avengers"),
                new ItemViewModel("Steven Rogers", "https://pbs.twimg.com/profile_images/989407515946422272/92kTlg2-_400x400.jpg", "Avengers"),
                new ItemViewModel("Carol Danvers", "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRmGyc9Vgzl18DyNBkxGJEwZ4bpudymz3itkz4Dmt4HI_lWlk-Fyw", "Infinity Watch"),
                new ItemViewModel("Peter Quill", "https://media.tenor.com/images/9d586f93e1d889dbc194397cb5fb860d/raw", "Guardians of the Galaxy"),
            };
        }

        public ObservableCollection<ItemViewModel> Items { get; }
    }
}
