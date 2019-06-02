using MonoDevelop.Core;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Implementations
{
    public class SettingsService : ISettingsService
    {
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