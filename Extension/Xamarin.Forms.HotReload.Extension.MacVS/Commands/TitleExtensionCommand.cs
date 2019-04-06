using MonoDevelop.Components.Commands;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Commands
{
    //Dummy command to show group title in Mac VS.
    public class TitleExtensionCommand : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Enabled = false;
        }
    }
}