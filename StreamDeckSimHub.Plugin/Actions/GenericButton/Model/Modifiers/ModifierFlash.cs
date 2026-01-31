// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

public partial class ModifierFlash : Modifier
{
    public const string UiName = "Flash";

    public static ModifierFlash Create()
    {
        return new ModifierFlash();
    }

    [ObservableProperty] private int? _durationOn;
    [ObservableProperty] private int? _durationOff;

    public override string DisplayName => UiName;

    [JsonIgnore]
    public int CurrentTick { get; set; }
}