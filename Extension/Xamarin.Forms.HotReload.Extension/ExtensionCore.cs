using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Helpers;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension
{
    internal class ExtensionCore
    {
        private const string XamlResourceExtension = ".xaml";
        private const string CssResourceExtension = ".css";

        private readonly IGuiService _guiService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;
        private readonly HotReloadClientsHolder _clientsHolder;
        private readonly HashSet<string> _supportedResourceExtensions;

        private IEnvironmentCommand _enableExtensionCommand;
        private IEnvironmentCommand _disableExtensionCommand;
        private EnvironmentService _environmentService;

        internal ExtensionCore(IGuiService service, ISettingsService settingsStore, ILogger logger)
        {
            _guiService = service;
            _settingsService = settingsStore;
            _logger = logger;
            _clientsHolder = new HotReloadClientsHolder();
            _supportedResourceExtensions = new HashSet<string>
            {
                XamlResourceExtension,
                CssResourceExtension
            };
        }

        internal void Init(EnvironmentService environmentService,
            Dictionary<HotReloadCommands, IEnvironmentCommand> environmentCommands)
        {
            _settingsService.Initialize();

            _enableExtensionCommand = environmentCommands[HotReloadCommands.Enable];
            _disableExtensionCommand = environmentCommands[HotReloadCommands.Disable];
            _enableExtensionCommand.Executed += OnEnableExtensionExecuted;
            _disableExtensionCommand.Executed += OnDisableExtensionExecuted;

            environmentService.SolutionOpened += OnEnviromentSolutionOpened;
            environmentService.SolutionClosed += OnEnviromentSolutionClosed;
            _environmentService = environmentService;
            
            UpdateUiElementsVisibility();
     
            _disableExtensionCommand.IsEnabled = true;
            _enableExtensionCommand.IsEnabled = true;
        }

        private void OnEnableExtensionExecuted(object sender, EventArgs e)
        {
            _guiService.HideInfoBar();
            EnableExtension();
        }

        private void OnDisableExtensionExecuted(object sender, EventArgs e)
        {
            DisableExtension();
        }

        private void EnableExtension()
        {
            _clientsHolder.Run();
            _environmentService.DocumentSaved += OnEnviromentDocumentSaved;
            _enableExtensionCommand.IsVisible = false;
            _disableExtensionCommand.IsVisible = true;
        }

        private void DisableExtension()
        {
            _clientsHolder.Stop();
            _environmentService.DocumentSaved -= OnEnviromentDocumentSaved;
            _enableExtensionCommand.IsVisible = true;
            _disableExtensionCommand.IsVisible = false;
        }

        private void OnInfoBarResult(Task<InfoBarActionType> resultTask)
        {
            switch (resultTask.Result)
            {
                case InfoBarActionType.DontShowAgain:
                    _settingsService.ShowEnableHotReloadTooltip = false;
                    break;
                case InfoBarActionType.Enable:
                    EnableExtension();
                    break;
                case InfoBarActionType.NoAction:
                    break;
                default:
                    throw new NotImplementedException($"Result code {resultTask.Result} is not supported.");
            }
        }

        private void OnEnviromentSolutionClosed(object sender, EventArgs e)
        {
            _environmentService.DocumentSaved -= OnEnviromentDocumentSaved;
            UpdateUiElementsVisibility();
        }

        private void OnEnviromentSolutionOpened(object sender, EventArgs e)
        {
            UpdateUiElementsVisibility();
        }

        private void UpdateUiElementsVisibility()
        {
            if (_environmentService.IsSolutionOpened && _environmentService.SolutionHasXamarinProject())
            {
                _guiService.ShowExtensionToolbar();
                _enableExtensionCommand.IsVisible = true;
                _disableExtensionCommand.IsVisible = false;
                if (_settingsService.ShowEnableHotReloadTooltip)
                {
                    var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    _guiService.ShowInfoBarAsync(TextResources.InfoBar_ExtensionNotEnabled,
                            new InfoBarAction(InfoBarActionType.DontShowAgain, TextResources.InfoBar_DontShowAgain),
                            new InfoBarAction(InfoBarActionType.Enable, TextResources.InfoBar_EnableExtension))
                        .ContinueWith(OnInfoBarResult, taskScheduler);
                }
            }
            else
            {
                _guiService.HideInfoBar();
                _enableExtensionCommand.IsVisible = false;
                _disableExtensionCommand.IsVisible = false;
                _guiService.HideExtensionToolbar();
            }
        }

        private async void OnEnviromentDocumentSaved(object sender, DevEnviromentDocument e)
        {
            var documentExtension = Path.GetExtension(e.Path);

            try
            {
                if (_supportedResourceExtensions.Contains(documentExtension.ToLowerInvariant()))
                {
                    await _clientsHolder.UpdateResourceAsync(e.Path, e.Content);
                }
            }
            catch (AggregateException)
            {
                _logger.Log($"{TextResources.Update_Failed_Message} {TextResources.Device_Offline_Message}");
            }
        }
    }
}