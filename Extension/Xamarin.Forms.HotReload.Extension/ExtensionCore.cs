using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.HotReload.Extension.Abstractions;
using Xamarin.Forms.HotReload.Extension.Abstractions.Dialogs;
using Xamarin.Forms.HotReload.Extension.Abstractions.Services;
using Xamarin.Forms.HotReload.Extension.Enums;
using Xamarin.Forms.HotReload.Extension.Helpers;
using Xamarin.Forms.HotReload.Extension.Models;

namespace Xamarin.Forms.HotReload.Extension
{
    internal class ExtensionCore
    {
        private readonly IGuiService _guiService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;
        private readonly JsonSerializer _serializer;
        private readonly HotReloadClientsHolder _clientsHolder;

        private IEnvironmentCommand _enableExtensionCommand;
        private IEnvironmentCommand _disableExtensionCommand;
        private EnvironmentService _environmentService;

        internal ExtensionCore(IGuiService service, ISettingsService settingsStore, ILogger logger)
        {
            _serializer = new JsonSerializer();
            _guiService = service;
            _settingsService = settingsStore;
            _logger = logger;
            _clientsHolder = new HotReloadClientsHolder();
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

            _disableExtensionCommand.IsVisible = false;
            if (_environmentService.IsSolutionOpened)
            {
                var isXamarinSolution = _environmentService.SolutionHasXamarinProject();
                ShowInfoBarIfNeeded(isXamarinSolution, _settingsService.ShowEnableHotReloadTooltip);
                _enableExtensionCommand.IsVisible = isXamarinSolution;
            }
            else
            {
                _enableExtensionCommand.IsVisible = false;
            }

            _disableExtensionCommand.IsEnabled = true;
            _enableExtensionCommand.IsEnabled = true;
        }

        private void OnEnableExtensionExecuted(object sender, EventArgs e)
        {
            var configuration = CreateConfigurationModel();

            if (_guiService.ShowDialog<IConnectionsDialog>(configuration))
            {
                var updatedConfiguration = configuration;
                UpdateConfiguration(updatedConfiguration);
                _clientsHolder.Init(updatedConfiguration.ConnectionItems.ToList());

                _environmentService.DocumentSaved += OnEnviromentDocumentSaved;
                _enableExtensionCommand.IsVisible = false;
                _disableExtensionCommand.IsVisible = true;
            }
        }

        private ConnectionsConfigurationModel CreateConfigurationModel()
        {
            var serializedConnectionItems = _settingsService.SerializedConnectionItems;
            var connectionItems = _serializer.Deserialize<List<ConnectionItem>>(serializedConnectionItems);
            var configuration = new ConnectionsConfigurationModel(connectionItems, _settingsService.SaveConfiguration);
            return configuration;
        }

        private void OnInfoBarResult(Task<InfoBarActionType> resultTask)
        {
            switch (resultTask.Result)
            {
                case InfoBarActionType.DontShowAgain:
                    _settingsService.ShowEnableHotReloadTooltip = false;
                    break;
                case InfoBarActionType.Enable:
                    OnEnableExtensionExecuted(this, EventArgs.Empty);
                    break;
                case InfoBarActionType.NoAction:
                    break;
                default:
                    throw new NotImplementedException($"Result code {resultTask.Result} is not supported.");
            }
        }

        private void OnDisableExtensionExecuted(object sender, EventArgs e)
        {
            _environmentService.DocumentSaved -= OnEnviromentDocumentSaved;
            _enableExtensionCommand.IsVisible = true;
            _disableExtensionCommand.IsVisible = false;
        }

        private void UpdateConfiguration(ConnectionsConfigurationModel configuration)
        {
            if (configuration.SaveConfiguration)
            {
                SaveConfiguration(configuration);
            }
            else
            {
                ResetConfigurationDefaults();
            }
        }

        private void SaveConfiguration(ConnectionsConfigurationModel configurationModel)
        {
            _settingsService.SerializedConnectionItems = _serializer.Serialize(configurationModel.ConnectionItems);
            _settingsService.SaveConfiguration = true;
        }

        private void ResetConfigurationDefaults()
        {
            _settingsService.SerializedConnectionItems = SharedGlobals.DefaultSerializedConnectionItemsValue;
            _settingsService.SaveConfiguration = false;
        }

        private void OnEnviromentSolutionClosed(object sender, EventArgs e)
        {
            _environmentService.DocumentSaved -= OnEnviromentDocumentSaved;
            _guiService.HideInfoBar();
            _disableExtensionCommand.IsVisible = false;
            _enableExtensionCommand.IsVisible = false;
        }

        private void OnEnviromentSolutionOpened(object sender, EventArgs e)
        {
            var solutionHasXamarinProject = _environmentService.SolutionHasXamarinProject();

            _disableExtensionCommand.IsVisible = false;
            _enableExtensionCommand.IsVisible = solutionHasXamarinProject;

            ShowInfoBarIfNeeded(solutionHasXamarinProject, _settingsService.ShowEnableHotReloadTooltip);
        }

        private void ShowInfoBarIfNeeded(bool solutionHasXamarinProject, bool showEnableHotReloadTooltip)
        {
            if (solutionHasXamarinProject && showEnableHotReloadTooltip)
            {
                var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                _guiService.ShowInfoBarAsync(TextResources.InfoBar_ExtensionNotEnabled,
                        new InfoBarAction(InfoBarActionType.DontShowAgain, TextResources.InfoBar_DontShowAgain),
                        new InfoBarAction(InfoBarActionType.Enable, TextResources.InfoBar_EnableExtension))
                    .ContinueWith(OnInfoBarResult, taskScheduler);
            }
        }

        private async void OnEnviromentDocumentSaved(object sender, DevEnviromentDocument e)
        {
            var documentExtension = Path.GetExtension(e.Path);

            try
            {
                if (documentExtension != null)
                {
                    switch (documentExtension.ToLowerInvariant())
                    {
                        case ".xaml":
                        {
                            await _clientsHolder.UpdateXamlAsync(e.Content);
                            break;
                        }
                        case ".css":
                        {
                            await _clientsHolder.UpdateCssAsync(e.Path);
                            break;
                        }
                    }
                }
            }
            catch (AggregateException)
            {
                _logger.Log($"{TextResources.Update_Failed_Message} {TextResources.Device_Offline_Message}");
            }
        }
    }
}