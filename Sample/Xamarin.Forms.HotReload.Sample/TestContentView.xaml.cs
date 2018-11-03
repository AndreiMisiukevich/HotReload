using Xamarin.Forms.HotReload.Reloader;

namespace Xamarin.Forms.HotReload.Sample
{
    public partial class TestContentView : ContentView
    {
        public TestContentView()
        {
#if DEBUG
            this.InitializeElement();
#else
            InitializeComponent();
#endif
        }
    }
}
