// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Settings for Hotkey Action, which are set in the Stream Deck UI.
/// </summary>
public class HotkeySettings
{
    public string Hotkey { get; init; } = string.Empty;
    public bool Ctrl { get; init; }
    public bool Alt { get; init; }
    public bool Shift { get; init; }

    protected string HotkeyString(string hotkey, bool ctrl, bool alt, bool shift)
    {
        return $"Hotkey: {hotkey}, Modifier: {(ctrl ? 'C' : '-')}{(alt ? 'A' : '-')}{(shift ? 'S' : '-')}";
    }

    public override string ToString()
    {
        return $"{HotkeyString(Hotkey, Ctrl, Alt, Shift)}";
    }
}