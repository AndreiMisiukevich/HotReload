namespace Xamarin.Forms.HotReload.Extension.Abstractions.Services
{
    public interface ISettingsService
    {
        bool ShowEnableHotReloadTooltip { get; set; }

        void Initialize();
    }
}