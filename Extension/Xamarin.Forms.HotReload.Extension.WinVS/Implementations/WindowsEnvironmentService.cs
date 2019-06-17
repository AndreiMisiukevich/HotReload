using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.WinVS.Extensions;

namespace Xamarin.Forms.HotReload.Extension.WinVS.Implementations
{
    public class WindowsEnvironmentService : EnvironmentService
    {
        // Fields needed to prevent these objects from garbage collection.
        private readonly IServiceProvider _serviceContainer;
        private readonly DocumentEvents _documentEvents;
        private readonly SolutionEvents _solutionEvents;
        private readonly Solution _solution;
        private readonly DTEEvents _devEnvEvents;

        public WindowsEnvironmentService(IServiceProvider serviceContainer, DocumentEvents documentEvents,
            SolutionEvents solutionEvents, Solution solution, DTEEvents devEnvEvents)
        {
            _serviceContainer = serviceContainer;
            _documentEvents = documentEvents;
            _solutionEvents = solutionEvents;
            _devEnvEvents = devEnvEvents;
            _solution = solution;
            _documentEvents.DocumentSaved += OnEnviromentDocumentSaved;
            _solutionEvents.Opened += OnEnviromentSolutionOpened;
            _solutionEvents.AfterClosing += AfterEnviromentSolutionClosing;
            _devEnvEvents.OnBeginShutdown += OnBeginShutdown;
        }

        public override bool IsSolutionOpened => _solution?.IsOpen ?? false;

        public override bool SolutionHasXamarinProject()
        {
            var solutionProjectItems = _solution.Projects;
            bool hasXamarinProject = false;
            if (solutionProjectItems != null)
            {
                var vsSolution = (IVsSolution) _serviceContainer.GetService(typeof(SVsSolution));
                var projectsEnumerator = solutionProjectItems.GetEnumerator();
                while (projectsEnumerator.MoveNext())
                {
                    var solutionItem = projectsEnumerator.Current as Project;
                    if (solutionItem == null)
                    {
                        continue;
                    }

                    hasXamarinProject = solutionItem.IsFolder()
                        ? vsSolution.FolderContainsXamarinProjects(solutionItem)
                        : vsSolution.IsXamarinProject(solutionItem);

                    if (hasXamarinProject)
                    {
                        break;
                    }
                }
            }

            return hasXamarinProject;
        }

        private void OnEnviromentDocumentSaved(Document document)
        {
            var enviromentDocument = document.ToDevEnviromentDocument();
            OnDocumentSaved(enviromentDocument);
        }

        private void OnEnviromentSolutionOpened()
        {
            OnSolutionOpened();
        }

        private void AfterEnviromentSolutionClosing()
        {
            OnSolutionClosed();
        }

        private void OnBeginShutdown()
        {
            OnIdeClosing();
        }
    }
}