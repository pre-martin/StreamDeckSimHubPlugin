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
    private Brush _actionState = ActionStates.InactiveBrush;

    public async Task<ActionResult> Execute()
    {
        _logger.Info($"Starting action {GetType().Name}");

        ActionState = ActionStates.RunningBrush;
        try
        {
            var result = await ExecuteInternal();
            ActionState = result switch
            {
                ActionResult.Success => ActionStates.SuccessBrush,
                ActionResult.Error => ActionStates.ErrorBrush,
                ActionResult.NotRequired => ActionStates.InactiveBrush,
                _ => ActionState
            };

            _logger.Info($"Finished action {GetType().Name} with result {result}");
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