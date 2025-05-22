// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class CommandItemKeypress : CommandItem
{
    public const string UiName = "Keypress";

    public required string Key { get; set; } = string.Empty;
    public required bool ModifierCtrl { get; set; }
    public required bool ModifierAlt { get; set; }
    public required bool ModifierShift { get; set; }
    public required KeyboardUtils.Hotkey? Hotkey { get; set; }

    public static CommandItemKeypress Create()
    {
        return new CommandItemKeypress
        {
            ActiveConditions = [],
            Key = string.Empty,
            ModifierCtrl = false,
            ModifierAlt = false,
            ModifierShift = false,
            Hotkey = null
        };
    }
}