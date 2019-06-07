using JetBrains.ProjectModel;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;

namespace Xamarin.Forms.HotReload.Extension.Rider.Implementations
{
    [SolutionComponent]
    public class SettingsServiceStub : ISettingsService
    {
        public bool ShowEnableHotReloadTooltip { get; set; }
        public void Initialize()
        {
        }
    }
}