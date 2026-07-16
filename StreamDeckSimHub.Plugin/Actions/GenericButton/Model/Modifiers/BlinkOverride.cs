// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

/// <summary>
/// When activated, this object owns the phase state for all blink modifiers of the same element: all blink in sync.
/// </summary>
public partial class BlinkOverride : ObservableObject
{
    [ObservableProperty] private bool _enabled;
    [ObservableProperty] private int _durationOn = 5;
    [ObservableProperty] private int _durationOff = 5;

    private int CurrentTick { get; set; }

    /// <summary>
    /// Increments the tick counter and returns <c>true</c> if an on/off transition occurred (on->off or off->on).
    /// </summary>
    /// <returns><c>true</c> if an on/off or off/on transition occurred</returns>
    public bool Tick()
    {
        if (!Enabled) return false;
        if (DurationOn <= 0 || DurationOff <= 0) return false;

        var cycleDuration = DurationOn + DurationOff;
        CurrentTick++;

        // Wrap around at the end of the cycle
        if (CurrentTick > cycleDuration)
        {
            CurrentTick = 1;
        }

        // Determine if transitioned on->off or off->on
        return CurrentTick == DurationOn + 1 || CurrentTick == 1;
    }

    /// <summary>
    /// Is the override currently in the "Off" phase?
    /// </summary>
    public bool IsOffPhase()
    {
        if (!Enabled) return false;
        if (DurationOn <= 0 || DurationOff <= 0) return false;

        return CurrentTick > DurationOn;
    }

    /// <summary>
    /// Resets the tick counter when the override is enabled, so that it starts with a full "On" phase.
    /// </summary>
    partial void OnEnabledChanged(bool value)
    {
        if (value) CurrentTick = 0;
    }
}