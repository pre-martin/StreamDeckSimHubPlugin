// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows.Media;

namespace StreamDeckSimHub.Installer.Actions;

public enum ActionResult
{
    NotRequired,
    Success,
    Warning,
    Error,
}

public abstract class ActionColors
{
    public static readonly Brush InactiveBrush = Brushes.Gray;
    public static readonly Brush RunningBrush = Brushes.Orange;
    public static readonly Brush SuccessBrush = Brushes.Green;
    public static readonly Brush ErrorBrush = Brushes.Red;
    public static readonly Brush WarningBrush = Brushes.Yellow;
}

/// <summary>
/// An action that can be executed by the installer.
/// </summary>
/// <remarks>Holds the action logic, but is also misused as a ViewModel.</remarks>
public interface IInstallerAction
{
    /// <summary>
    /// Displayed as header in the UI.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Detailed message below the header.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Result of the action in the form of a colored brush.
    /// </summary>
    Brush ActionResultColor { get; }

    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <returns>matic result of the action. Is used in the installer logic.</returns>
    Task<ActionResult> Execute();
}
