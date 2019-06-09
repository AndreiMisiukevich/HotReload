using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Host.Features;
using Xamarin.Forms.HotReload.Extension.Rider.Implementations;

namespace Xamarin.Forms.HotReload.Extension.Rider
{
    [SolutionComponent]
    public class ReloadTracker
    {
        public ReloadTracker(Lifetime lifetime, ISolution solution, RunManager host, RiderEnvironmentService envService)
        {
            if (solution.GetData(ProjectModelExtensions.ProtocolSolutionKey) == null)
                return;

            host.PerformModelAction(rd =>
                rd.Reload.Advise(lifetime, savedDoc => { envService.OnDocumentSaved(savedDoc); }));
        }
    }
}