using JetBrains.ProjectModel;
using JetBrains.Rider.Model;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.Rider.Implementations
{
    [SolutionComponent]
    public class RiderEnvironmentService: EnvironmentService
    {
        public override bool IsSolutionOpened => true;

        public override bool SolutionHasXamarinProject()
        {
            return true;
        }

        public void OnDocumentSaved(SavedDocument document)
        {
            var doc = new DevEnviromentDocument(document.FilePath, new string(document.Content));
            OnDocumentSaved(doc);
        }
    }
}