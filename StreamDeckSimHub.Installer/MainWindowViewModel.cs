// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StreamDeckSimHub.Installer.Actions;

namespace StreamDeckSimHub.Installer;

public partial class MainWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private static readonly Brush SuccessBrush = Brushes.Green;
    private static readonly Brush WarningBrush = Brushes.Orange;
    private static readonly Brush ErrorBrush = Brushes.Red;

    public string Version => ThisAssembly.AssemblyFileVersion;

    [ObservableProperty]
    private ObservableCollection<IInstallerAction> _installerSteps = [];

    [ObservableProperty]
    private string _result = string.Empty;

    [ObservableProperty]
    private Brush _resultBrush = SuccessBrush;

    [RelayCommand]
    private async Task Install()
    {
        ClearResultText();
        InstallerSteps.Clear();
        Logger.Info("========== Starting installation ==========");

        var stopStreamDeck = new StopStreamDeckSoftware();
        InstallerSteps.Add(stopStreamDeck);
        if (await stopStreamDeck.Execute() == ActionResult.Error)
        {
            SetErrorResultText();
            return;
        }

        var result = ActionResult.Success;
        var installStreamDeckPlugin = new InstallStreamDeckPlugin();
        InstallerSteps.Add(installStreamDeckPlugin);
        result = SetHigherResultLevel(await installStreamDeckPlugin.Execute(), result);

        var startStreamDeck = new StartStreamDeckSoftware();
        InstallerSteps.Add(startStreamDeck);
        result = SetHigherResultLevel(await startStreamDeck.Execute(), result);

        var verifySimHubPlugin = new VerifySimHubPlugin();
        InstallerSteps.Add(verifySimHubPlugin);
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
        Result = """
                 The installation was NOT successful. Please stop the Stream Deck software manually and try again.
                 """;
        ResultBrush = ErrorBrush;
    }
}