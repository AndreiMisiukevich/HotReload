using System;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Host.Features;
using JetBrains.Rider.Model;

namespace Xamarin.Forms.HotReload.Extension.Rider
{
    [SolutionComponent]
    public class RunManager
    {
        private readonly HotReloadPluginModel myModel;

        public RunManager(ISolution solution)
        {
            myModel = solution.GetProtocolSolution().GetHotReloadPluginModel();
        }
        
        public void PerformModelAction(Action<HotReloadPluginModel> action)
        {
                action(myModel);
        }
    }
}