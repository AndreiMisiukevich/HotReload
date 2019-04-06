using System;
using Document = MonoDevelop.Ide.Gui.Document;
using MonoDevelop.Ide;
using Xamarin.Forms.HotReload.Extension.MacVS.Extensions;
using MonoDevelop.Projects;
using System.Linq;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;

namespace Xamarin.Forms.HotReload.Extension.MacVS.Implementations
{
    public class MacEnvironmentService : EnvironmentService
    {
        private Document _currentActiveDocument;

        public MacEnvironmentService()
        {
            IdeApp.Workbench.ActiveDocumentChanged += OnActiveDocumentChanged;
            IdeApp.Workspace.SolutionLoaded += OnSolutionLoaded;
            IdeApp.Workspace.SolutionUnloaded += OnSolutionUnloaded;
        }

        public override bool IsSolutionOpened => IdeApp.Workspace.GetAllSolutions().Any();

        private void OnSolutionUnloaded(object sender, SolutionEventArgs e)
        {
            OnSolutionClosed();
        }

        private void OnSolutionLoaded(object sender, SolutionEventArgs e)
        {
            OnSolutionOpened();
        }

        private void OnActiveDocumentChanged(object sender, EventArgs e)
        {
            var activeDocument = IdeApp.Workbench.ActiveDocument;
            if (_currentActiveDocument == activeDocument)
            {
                return;
            }

            if (_currentActiveDocument != null)
            {
                _currentActiveDocument.Saved -= OnActiveDocumentSaved;
                _currentActiveDocument = null;
            }

            if (activeDocument != null)
            {
                activeDocument.Saved += OnActiveDocumentSaved;
                _currentActiveDocument = activeDocument;
            }
        }

        private void OnActiveDocumentSaved(object sender, EventArgs e)
        {
            var enviromentDocument = _currentActiveDocument.ToDevEnviromentDocument();
            OnDocumentSaved(enviromentDocument);
        }

        public override bool SolutionHasXamarinProject()
        {
            //Not implemented for Mac VS.
            return true;
        }
    }
}