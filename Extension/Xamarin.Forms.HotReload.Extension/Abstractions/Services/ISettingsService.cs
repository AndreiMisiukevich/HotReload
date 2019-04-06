namespace Xamarin.Forms.HotReload.Extension.Abstractions.Services
{
    public interface ISettingsService
    {
        bool SaveConfiguration { get; set; }

        string SerializedConnectionItems { get; set; }

        bool ShowEnableHotReloadTooltip { get; set; }

        void Initialize();
    }
}