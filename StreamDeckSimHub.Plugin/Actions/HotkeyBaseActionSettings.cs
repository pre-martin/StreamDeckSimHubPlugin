// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Settings for Hotkey Action. Extends the Hotkey with SimHub Control and SimHub Property.
/// </summary>
public class HotkeyBaseActionSettings : HotkeySettings
{
    public string SimHubProperty { get; set; } = string.Empty;
    public bool HasLongKeypress { get; set; } = false;
    public HotkeySettings LongKeypressSettings { get; set; } = new();
    public uint LongKeypressShortHoldTime { get; set; } = 50;
    public uint LongKeypressTimeSpan { get; set; } = 500;

    public override string ToString()
    {
        return
            $"{base.ToString()}, SimHubProperty: {SimHubProperty}, " +
            $"HasLongKeypress: {HasLongKeypress}, " +
            $"LongKeypressSettings: {LongKeypressSettings}, LongKeypressTimeSpan: {LongKeypressTimeSpan}, " +
            $"LongKeypressShortHoldTime: {LongKeypressShortHoldTime}";
    }
}
