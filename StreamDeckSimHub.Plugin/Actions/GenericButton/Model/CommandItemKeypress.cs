// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.ComponentModel;
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

    protected override string RawDisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrEmpty(Key) ? KeyDisplayName() : "Keypress";

    private string KeyDisplayName()
    {
        var name = "";
        if (ModifierCtrl) name += "Ctrl";
        if (ModifierAlt) name += name.Length > 0 ? "-Alt" : "Alt";
        if (ModifierShift) name += name.Length > 0 ? "-Shift" : "Shift";

        name += name.Length > 0 ? $" {Key}" : Key;
        return name;
    }

    public static CommandItemKeypress Create()
    {
        return new CommandItemKeypress
        {
            Key = string.Empty,
            ModifierCtrl = false,
            ModifierAlt = false,
            ModifierShift = false,
            Hotkey = null
        };
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        Hotkey = KeyboardUtils.CreateHotkey(ModifierCtrl, ModifierAlt, ModifierShift, Key);
        base.OnPropertyChanged(e);
    }
}