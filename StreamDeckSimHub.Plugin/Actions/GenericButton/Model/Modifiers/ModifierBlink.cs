// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

public partial class ModifierBlink : Modifier
{
    public const string UiName = "Blink";

    public static ModifierBlink Create()
    {
        return new ModifierBlink();
    }

    [ObservableProperty] private int? _durationOn;
    [ObservableProperty] private int? _durationOff;

    public override string DisplayName => UiName;

    [JsonIgnore]
    public int CurrentTick { get; set; }

    [JsonIgnore]
    public bool WasActiveLastTick { get; set; }
}