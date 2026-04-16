// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings.Modifiers;

public class ModifierBlinkDto : ModifierDto
{
    public required int DurationOn { get; set;  }
    public required int DurationOff { get; set;  }
}