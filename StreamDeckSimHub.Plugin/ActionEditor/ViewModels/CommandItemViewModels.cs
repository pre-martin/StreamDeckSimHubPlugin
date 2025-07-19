// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Media;
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
public abstract class CommandItemViewModel(
    CommandItem model,
    Window parentWindow,
    StreamDeckAction parentAction) : ItemViewModel(model, parentWindow), IFlatCommandItemsViewModel
{
    public StreamDeckAction ParentAction { get; } = parentAction;

    protected static bool IsLongPressAllowed(StreamDeckAction action)
    {
        // Long press is only allowed for KeyDown and DialDown.
        return action is StreamDeckAction.KeyDown or StreamDeckAction.DialDown;
    }
}

/// <summary>
/// ViewModel for CommandItemKeypress
/// </summary>
public partial class CommandItemKeypressViewModel(
    CommandItemKeypress model,
    Window parentWindow,
    StreamDeckAction parentAction) : CommandItemViewModel(model, parentWindow, parentAction)
{
    public override ImageSource? Icon => ParentWindow.FindResource("DiKeyboardOutlinedGray") as ImageSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _key = model.Key;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private bool _modifierCtrl = model.ModifierCtrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private bool _modifierAlt = model.ModifierAlt;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private bool _modifierShift = model.ModifierShift;

    partial void OnKeyChanged(string value)
    {
        model.Key = value;
    }

    partial void OnModifierCtrlChanged(bool value)
    {
        model.ModifierCtrl = value;
    }

    partial void OnModifierAltChanged(bool value)
    {
        model.ModifierAlt = value;
    }

    partial void OnModifierShiftChanged(bool value)
    {
        model.ModifierShift = value;
    }

    [ObservableProperty] private bool _longEnabled = IsLongPressAllowed(parentAction) && model.LongEnabled;

    public bool LongPressAllowed => IsLongPressAllowed(ParentAction);

    partial void OnLongEnabledChanged(bool value)
    {
        model.LongEnabled = value;
    }
}

/// <summary>
/// ViewModel for CommandItemSimHubControl
/// </summary>
public partial class CommandItemSimHubControlViewModel(
    CommandItemSimHubControl model,
    Window parentWindow,
    StreamDeckAction parentAction) : CommandItemViewModel(model, parentWindow, parentAction)
{
    public override ImageSource? Icon => ParentWindow.FindResource("DiSimHubControlGray") as ImageSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))] // see CommandItemSimHubControl.RawDisplayName which uses Control
    private string _control = model.Control;

    partial void OnControlChanged(string value)
    {
        model.Control = value;
    }

    [ObservableProperty] private bool _longEnabled = IsLongPressAllowed(parentAction) && model.LongEnabled;

    public bool LongPressAllowed => IsLongPressAllowed(ParentAction);

    partial void OnLongEnabledChanged(bool value)
    {
        model.LongEnabled = value;
    }
}

/// <summary>
/// ViewModel for CommandItemSimHubRole
/// </summary>
public partial class CommandItemSimHubRoleViewModel(
    CommandItemSimHubRole model,
    Window parentWindow,
    StreamDeckAction parentAction)
    : CommandItemViewModel(model, parentWindow, parentAction)
{
    public override ImageSource? Icon => ParentWindow.FindResource("DiSimHubRoleGray") as ImageSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))] // see CommandItemSimHubRole.RawDisplayName which uses Control
    private string _role = model.Role;

    partial void OnRoleChanged(string value)
    {
        model.Role = value;
    }

    [ObservableProperty] private bool _longEnabled = IsLongPressAllowed(parentAction) && model.LongEnabled;

    public bool LongPressAllowed => IsLongPressAllowed(ParentAction);

    partial void OnLongEnabledChanged(bool value)
    {
        model.LongEnabled = value;
    }
}