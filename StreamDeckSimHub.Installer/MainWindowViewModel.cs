// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using StreamDeckSimHub.Installer.Actions;

namespace StreamDeckSimHub.Installer
{
    public class MainWindowViewModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Brush SuccessBrush = Brushes.Green;
        private static readonly Brush WarningBrush = Brushes.Orange;
        private static readonly Brush ErrorBrush = Brushes.Red;

        public string Version => ThisAssembly.AssemblyFileVersion;

        private ObservableCollection<IInstallerAction> _installerSteps = new ObservableCollection<IInstallerAction>();

        public ObservableCollection<IInstallerAction> InstallerSteps
        {
            get => _installerSteps;
            set => SetProperty(ref _installerSteps, value);
        }

        private string _result = string.Empty;

        public string Result
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        private Brush _resultBrush = SuccessBrush;

        public Brush ResultBrush
        {
            get => _resultBrush;
            set => SetProperty(ref _resultBrush, value);
        }

        private AsyncRelayCommand _installCommand;
        public IAsyncRelayCommand InstallCommand => _installCommand ?? (_installCommand = new AsyncRelayCommand(Install));

        private async Task Install()
        {
            // Execute installation in a new task to make the UI more responsive.
            await Task.Run(InstallTask);
        }

        private async Task InstallTask()
        {
            ClearResultText();
            Application.Current.Dispatcher.Invoke(() => InstallerSteps.Clear());
            Logger.Info("========== Starting installation ==========");

            var checkDotnet = new CheckDotnetRuntime();
            Application.Current.Dispatcher.Invoke(() => InstallerSteps.Add(checkDotnet));
            if (await checkDotnet.Execute() == ActionResult.Error)
            {
                SetErrorResultText(
                    $"Error while checking/installing .NET Desktop Runtime. Please install v{checkDotnet.DotnetRequired} or newer manually and try again.");
                return;
            }

            var stopStreamDeck = new StopStreamDeckSoftware();
            Application.Current.Dispatcher.Invoke(() => InstallerSteps.Add(stopStreamDeck));
            if (await stopStreamDeck.Execute() == ActionResult.Error)
            {
                SetErrorResultText();
                return;
            }

            var result = ActionResult.Success;
            var installStreamDeckPlugin = new InstallStreamDeckPlugin();
            Application.Current.Dispatcher.Invoke(() => InstallerSteps.Add(installStreamDeckPlugin));
            result = SetHigherResultLevel(await installStreamDeckPlugin.Execute(), result);

            var startStreamDeck = new StartStreamDeckSoftware();
            Application.Current.Dispatcher.Invoke(() => InstallerSteps.Add(startStreamDeck));
            result = SetHigherResultLevel(await startStreamDeck.Execute(), result);

            var verifySimHubPlugin = new VerifySimHubPlugin();
            Application.Current.Dispatcher.Invoke(() => InstallerSteps.Add(verifySimHubPlugin));
            result = SetHigherResultLevel(await verifySimHubPlugin.Execute(), result);

            switch (result)
            {
                case ActionResult.NotRequired:
                case ActionResult.Success:
                    SetSuccessResultText();
                    break;
                case ActionResult.Warning:
                    SetWarningResultText();
                    break;
                case ActionResult.Error:
                    SetErrorResultText();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ActionResult SetHigherResultLevel(ActionResult result, ActionResult existingResult)
        {
            var v1 = (int)result;
            var v2 = (int)existingResult;
            return v1 > v2 ? result : existingResult;
        }

        private void ClearResultText()
        {
            Result = string.Empty;
            ResultBrush = SuccessBrush;
        }

        private void SetSuccessResultText()
        {
            Result = "The plugin was installed successfully. You can exit the program now.";
            ResultBrush = SuccessBrush;
        }

        private void SetWarningResultText()
        {
            Result = "There have been warnings during the installation. Please check the steps above.";
            ResultBrush = WarningBrush;
        }

        private void SetErrorResultText()
        {
            Result = "The installation was NOT successful. Please stop the Stream Deck software manually and try again.";
            ResultBrush = ErrorBrush;
        }

        private void SetErrorResultText(string message)
        {
            Result = message;
            ResultBrush = ErrorBrush;
        }
    }
}