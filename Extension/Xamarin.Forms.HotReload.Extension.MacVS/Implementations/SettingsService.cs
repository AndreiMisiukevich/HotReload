using MonoDevelop.Core;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Implementations
{
    public class SettingsService : ISettingsService
    {
        public bool SaveConfiguration
        {
            get => PropertyService.Get<bool>(SharedGlobals.SavePreferencesPath, false);
            set => PropertyService.Set(SharedGlobals.SavePreferencesPath, value);
        }

        public string SerializedConnectionItems
        {
            get => PropertyService.Get<string>(SharedGlobals.SerializedConnectionItemsPath,
                SharedGlobals.DefaultSerializedConnectionItemsValue);
            set => PropertyService.Set(SharedGlobals.SerializedConnectionItemsPath, value);
        }

        public bool ShowEnableHotReloadTooltip
        {
            get => PropertyService.Get<bool>(SharedGlobals.ShowEnableHotReloadTooltipPath, true);
            set => PropertyService.Set(SharedGlobals.ShowEnableHotReloadTooltipPath, value);
        }

        public void Initialize()
        {
        }
    }
}