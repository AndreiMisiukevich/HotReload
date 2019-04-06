using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Extensions
{
    public static class SettingsStoreExtensions
    {
        public static async Task<WritableSettingsStore> GetWritableSettingsStoreAsync(
            this Microsoft.VisualStudio.Shell.IAsyncServiceProvider asyncServiceProvider)
        {
            var vsSettingsManager =
                (IVsSettingsManager) await asyncServiceProvider.GetServiceAsync(typeof(SVsSettingsManager));
            var shellSettingsManager = new ShellSettingsManager(vsSettingsManager);
            return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }
    }
}