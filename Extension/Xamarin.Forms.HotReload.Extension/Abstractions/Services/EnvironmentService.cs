using System;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension.Abstractions.Services
{
    public abstract class EnvironmentService
    {
        public event EventHandler<DevEnviromentDocument> DocumentSaved;

        public event EventHandler SolutionOpened;

        public event EventHandler SolutionClosed;

        public abstract bool IsSolutionOpened { get; }

        public abstract bool SolutionHasXamarinProject();


        protected virtual void OnDocumentSaved(DevEnviromentDocument e)
        {
            DocumentSaved?.Invoke(this, e);
        }

        protected virtual void OnSolutionOpened()
        {
            SolutionOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSolutionClosed()
        {
            SolutionClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}