using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Helpers;
using Xamarin.Forms.HotReload.Extension.WinVS.Commands;
using Xamarin.Forms.HotReload.Extension.WinVS.Dialogs.Connections;
using Xamarin.Forms.HotReload.Extension.WinVS.Implementations;
using Task = System.Threading.Tasks.Task;

namespace Xamarin.Forms.HotReload.Extension.WinVS
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", SharedGlobals.AppVersion, IconResourceID =
        400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class HotReloadPackage : AsyncPackage
    {
        /// <summary>
        /// HotReloadPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "6dc5d6e4-b788-4f66-92e7-29253691e9e3";

        /// <summary>
        /// Initializes a new instance of the <see cref="HotReloadPackage"/> class.
        /// </summary>
        public HotReloadPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var settingsManager = new ShellSettingsManager(this);
            var writableSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            var settingsStore = new SettingsService(writableSettingsStore);

            var dte = (DTE) await GetServiceAsync(typeof(SDTE));
            var applicationEvents = dte.Application.Events;
            var documentEvents = applicationEvents.DocumentEvents;
            var enviromentEvents = new WindowsEnvironmentService(this, documentEvents, applicationEvents.SolutionEvents,
                dte.Application.Solution);

            var dependencyContainer = new DependencyContainer(new DependenciesRegistrar());
            var guiPresenter = new GuiService(this, dependencyContainer);

            await EnableExtensionCommand.InitializeAsync(this);
            await DisableExtensionCommand.InitializeAsync(this);

            var commandsDictionary = new Dictionary<HotReloadCommands, IEnvironmentCommand>
            {
                {HotReloadCommands.Enable, EnableExtensionCommand.Instance},
                {HotReloadCommands.Disable, DisableExtensionCommand.Instance}
            };

            Main.Init(enviromentEvents, commandsDictionary, guiPresenter, settingsStore);
        }

        #endregion
    }
}