#if DEBUG
using Xamarin.Forms.HotReload.Reloader;
#endif

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
