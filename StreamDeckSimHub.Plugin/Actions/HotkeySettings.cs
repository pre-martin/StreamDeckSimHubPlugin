// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Settings for Hotkey Action, which are set in the Stream Deck UI.
/// </summary>
public class HotkeySettings
{
    public string Hotkey { get; set; } = string.Empty;

    public string SimHubProperty { get; set; } = string.Empty;

    public bool Ctrl { get; set; }

    public bool Alt { get; set; }

    public bool Shift { get; set; }
}
