// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Installer.Actions;

public abstract partial class AbstractInstallerAction : ObservableObject, IInstallerAction
{
    private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public abstract string Name { get; }

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private Brush _actionResultColor = ActionColors.InactiveBrush;

    public async Task<ActionResult> Execute()
    {
        _logger.Info($"Starting action {GetType().Name}");

        ActionResultColor = ActionColors.RunningBrush;
        try
        {
            var result = await ExecuteInternal();
            ActionResultColor = result switch
            {
                ActionResult.Success => ActionColors.SuccessBrush,
                ActionResult.Error => ActionColors.ErrorBrush,
                ActionResult.NotRequired => ActionColors.InactiveBrush,
                ActionResult.Warning => ActionColors.WarningBrush,
                _ => throw new ArgumentOutOfRangeException()
            };

            _logger.Info($"Finished action {GetType().Name} with result {result}");

            // Give the user a tiny moment to realize that there is something going on.
            await Task.Delay(1000);

            return result;
        }
        catch (Exception e)
        {
            SetAndLogError(e, $"Action {GetType().Name} failed with exception");
            return ActionResult.Error;
        }
    }

    protected abstract Task<ActionResult> ExecuteInternal();

    protected void SetAndLogInfo(string message)
    {
        Message = message;
        _logger.Info(message);
    }

    protected void SetAndLogError(string message)
    {
        Message = message;
        _logger.Info(message);
    }

    protected void SetAndLogError(Exception e, string message)
    {
        Message = message;
        _logger.Error(e, message);
    }
}