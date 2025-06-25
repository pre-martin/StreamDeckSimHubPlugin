// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class CommandItemKeypress : CommandItem
{
    public const string UiName = "Keypress";

    [ObservableProperty] private string _key = string.Empty;
    [ObservableProperty] private bool _modifierCtrl;
    [ObservableProperty] private bool _modifierAlt;
    [ObservableProperty] private bool _modifierShift;
    public KeyboardUtils.Hotkey? Hotkey { get; set; }

    public static CommandItemKeypress Create()
    {
        return new CommandItemKeypress
        {
            Conditions = [],
            Key = string.Empty,
            ModifierCtrl = false,
            ModifierAlt = false,
            ModifierShift = false,
            Hotkey = null
        };
    }
}