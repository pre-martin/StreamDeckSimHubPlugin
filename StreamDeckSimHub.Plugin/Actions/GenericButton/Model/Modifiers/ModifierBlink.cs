// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

public partial class ModifierBlink : Modifier
{
    public const string UiName = "Blink";

    public static ModifierBlink Create()
    {
        return new ModifierBlink();
    }

    [ObservableProperty] private int _durationOn = 5;
    [ObservableProperty] private int _durationOff = 5;

    public override string DisplayName => UiName;

    private int CurrentTick { get; set; }
    private bool WasActiveLastTick { get; set; }

    /// <summary>
    /// Determines if an inactive/active or active/inactive transition occurred.
    /// </summary>
    /// <returns><c>true</c> if a transition occured</returns>
    public bool DetermineTransition(bool isActiveNow)
    {
        var transitioned = false;
        if (isActiveNow && !WasActiveLastTick)
        {
            // Transition inactive->active: reset tick counter
            CurrentTick = 0;
            transitioned = true;
        }
        else if (!isActiveNow && WasActiveLastTick)
        {
            // Transition active->inactive: Just redraw to show the item again
            transitioned = true;
        }

        // Update the state for next tick
        WasActiveLastTick = isActiveNow;

        return  transitioned;
    }

    /// <summary>
    /// Increments the tick counter and returns <c>true</c> if an on/off transition occurred (on->off or off->on).
    /// </summary>
    /// <returns><c>true</c> if an on/off or off/on transition occurred</returns>
    public bool Tick()
    {
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
    /// Is the modifier currently in the "Off" phase?
    /// </summary>
    public bool IsOffPhase()
    {
        if (DurationOn <= 0 || DurationOff <= 0) return false;

        return CurrentTick > DurationOn;
    }
}