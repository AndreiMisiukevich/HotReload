using System;
using JetBrains.ProjectModel;
using JetBrains.RdBackend.Common.Features;
using JetBrains.Rider.Model;

namespace Xamarin.Forms.HotReload.Extension.Rider
{
    [SolutionComponent]
    public class RunManager
    {
        private readonly HotReloadPluginModel _myModel;

        public RunManager(ISolution solution)
        {
            _myModel = solution.GetProtocolSolution().GetHotReloadPluginModel();
        }

        public void PerformModelAction(Action<HotReloadPluginModel> action)
        {
            action(_myModel);
        }
    }
}