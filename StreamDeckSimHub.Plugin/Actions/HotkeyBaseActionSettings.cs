// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Settings for Hotkey Action. Extends the Hotkey with SimHub Control and SimHub Property.
/// </summary>
public class HotkeyBaseActionSettings : HotkeySettings
{
    public string SimHubControl { get; init; } = string.Empty;
    public string SimHubProperty { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"{base.ToString()}, SimHubControl: {SimHubControl}, SimHubProperty: {SimHubProperty}";
    }
}