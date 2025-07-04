// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

/// <summary>
/// Common interface for the flat command list with different entries.
/// </summary>
public interface IFlatCommandItemsViewModel;

/// <summary>
/// ViewModel for StreamDeckAction (header in the command list)
/// </summary>
public class StreamDeckActionViewModel(StreamDeckAction action) : ObservableObject, IFlatCommandItemsViewModel
{
    public StreamDeckAction Action { get; } = action;

    public override string ToString() => Action.ToString();
}

/// <summary>
/// Base ViewModel for all CommandItems
/// </summary>
public abstract class CommandItemViewModel(CommandItem model, StreamDeckAction parentAction)
    : ItemViewModel(model), IFlatCommandItemsViewModel
{
    public StreamDeckAction ParentAction { get; } = parentAction;
}

/// <summary>
/// ViewModel for CommandItemKeypress
/// </summary>
public class CommandItemKeypressViewModel(CommandItemKeypress model, StreamDeckAction parentAction)
    : CommandItemViewModel(model, parentAction)
{
    // Add properties specific to CommandItemKeypress here
}

/// <summary>
/// ViewModel for CommandItemSimHubControl
/// </summary>
public class CommandItemSimHubControlViewModel(CommandItemSimHubControl model, StreamDeckAction parentAction)
    : CommandItemViewModel(model,
        parentAction)
{
    // Add properties specific to CommandItemSimHubControl here
}

/// <summary>
/// ViewModel for CommandItemSimHubRole
/// </summary>
public class CommandItemSimHubRoleViewModel(CommandItemSimHubRole model, StreamDeckAction parentAction)
    : CommandItemViewModel(model, parentAction)
{
    // Add properties specific to CommandItemSimHubRole here
}