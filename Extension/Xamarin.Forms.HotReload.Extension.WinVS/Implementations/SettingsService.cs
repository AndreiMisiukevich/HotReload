using Microsoft.VisualStudio.Settings;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Implementations
{
    public class SettingsService : Abstractions.Services.ISettingsService
    {
        private readonly WritableSettingsStore _settings;

        public SettingsService(WritableSettingsStore settings)
        {
            _settings = settings;
        }
        
        public bool ShowEnableHotReloadTooltip
        {
            get => _settings.GetBoolean(SharedGlobals.PackageCollectionPath,
                SharedGlobals.ShowEnableHotReloadTooltipPath, true);
            set => _settings.SetBoolean(SharedGlobals.PackageCollectionPath,
                SharedGlobals.ShowEnableHotReloadTooltipPath, value);
        }

        public void Initialize()
        {
            if (!_settings.CollectionExists(SharedGlobals.PackageCollectionPath))
            {
                _settings.CreateCollection(SharedGlobals.PackageCollectionPath);
            }
        }
    }
}