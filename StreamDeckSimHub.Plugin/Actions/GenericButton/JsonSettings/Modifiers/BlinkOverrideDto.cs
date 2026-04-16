// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings.Modifiers;

public class BlinkOverrideDto
{
    public bool Enabled { get; set; }
    public int DurationOn { get; set; } = 5;
    public int DurationOff { get; set; } = 5;
}