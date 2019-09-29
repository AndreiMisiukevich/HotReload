using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Xamarin.Forms.HotReload.Sample.Mac
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        private readonly NSWindow _window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            _window = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "HotReload",
                TitleVisibility = NSWindowTitleVisibility.Hidden
            };
        }

        public override NSWindow MainWindow => _window;

        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);
        }
    }
}
