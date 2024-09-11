// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows.Media;

namespace StreamDeckSimHub.Installer.Actions;

public enum ActionResult
{
    Success,
    NotRequired,
    Error,
}

public abstract class ActionStates
{
    public static readonly Brush InactiveBrush = Brushes.Gray;
    public static readonly Brush RunningBrush = Brushes.Orange;
    public static readonly Brush SuccessBrush = Brushes.Green;
    public static readonly Brush ErrorBrush = Brushes.Red;
}

public interface IInstallerAction
{
    string Name { get; }
    string Message { get; }
    Brush ActionState { get; }
    Task<ActionResult> Execute();
}
