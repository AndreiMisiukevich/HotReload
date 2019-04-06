using Microsoft.VisualStudio.Settings;
using ISettingsService = Xamarin.Forms.HotReload.Extension.Abstractions.Services.ISettingsService;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Implementations
{
    public class SettingsService : Abstractions.Services.ISettingsService
    {
        private readonly WritableSettingsStore _settings;

        public SettingsService(WritableSettingsStore settings)
        {
            _settings = settings;
        }

        public bool SaveConfiguration
        {
            get => _settings.GetBoolean(SharedGlobals.PackageCollectionPath, SharedGlobals.SavePreferencesPath, false);
            set => _settings.SetBoolean(SharedGlobals.PackageCollectionPath, SharedGlobals.SavePreferencesPath, value);
        }

        public string SerializedConnectionItems
        {
            get => _settings.GetString(SharedGlobals.PackageCollectionPath, SharedGlobals.SerializedConnectionItemsPath,
                SharedGlobals.DefaultSerializedConnectionItemsValue);
            set => _settings.SetString(SharedGlobals.PackageCollectionPath,
                SharedGlobals.SerializedConnectionItemsPath, value);
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