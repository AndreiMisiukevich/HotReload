namespace Xamarin.Forms.HotReload.Sample
{
    public partial class ResDict : ResourceDictionary
    {
        public ResDict()
        {
            InitializeComponent();
            //TODO: ResDict autodetection (remove)
#if DEBUG
            //this.RegisterHotReload();
#endif
        }
    }
}
