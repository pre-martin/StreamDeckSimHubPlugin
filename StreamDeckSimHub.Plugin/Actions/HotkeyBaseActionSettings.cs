// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Settings for Hotkey Action. Extends the Hotkey with SimHub Control and SimHub Property.
/// </summary>
public class HotkeyBaseActionSettings : HotkeySettings
{
    public string SimHubControl { get; init; } = string.Empty;
    public string SimHubProperty { get; set; } = string.Empty;
    public bool HasLongKeypress { get; init; } = false;
    public HotkeySettings LongKeypressSettings { get; init; } = new();
    public uint LongKeypressShortHoldTime { get; } = 50;
    public uint LongKeypressTimeSpan { get; } = 500;

    public override string ToString()
    {
        return $"{base.ToString()}, SimHubControl: {SimHubControl}, SimHubProperty: {SimHubProperty}, HasLongKeypress: {HasLongKeypress}, " +
            $"LongKeypressSettings: {LongKeypressSettings}, LongKeypressTimeSpan: {LongKeypressTimeSpan}, " +
            $"LongKeypressShortHoldTime: {LongKeypressShortHoldTime}";
    }
}
