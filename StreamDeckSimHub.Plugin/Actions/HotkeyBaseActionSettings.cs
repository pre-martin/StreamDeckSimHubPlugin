// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Settings for Hotkey Action, which are set in the Stream Deck UI.
/// </summary>
public class HotkeyBaseActionSettings
{
    public string Hotkey { get; init; } = string.Empty;

    public string SimHubControl { get; init; } = string.Empty;

    public string SimHubProperty { get; init; } = string.Empty;

    public bool Ctrl { get; init; }

    public bool Alt { get; init; }

    public bool Shift { get; init; }

    public override string ToString()
    {
        return $"Ctrl: {Ctrl}, Alt: {Alt}, Shift: {Shift}, Hotkey: {Hotkey}, SimHubControl: {SimHubControl}, SimHubProperty: {SimHubProperty}";
    }
}