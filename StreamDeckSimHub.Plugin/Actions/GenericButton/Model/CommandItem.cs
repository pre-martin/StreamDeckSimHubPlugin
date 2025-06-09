// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using StreamDeckSimHub.Plugin.PropertyLogic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract partial class CommandItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    public ObservableCollection<ConditionExpression> ActiveConditions { get; set; } = new();
}

