namespace Xamarin.Forms.HotReload.Sample
{
    public partial class ResDict : ResourceDictionary
    {
        public ResDict()
        {
            InitializeComponent();
            HotReloader.Current.RegisterReloadableNonElementComponent(this);
        }
    }
}
