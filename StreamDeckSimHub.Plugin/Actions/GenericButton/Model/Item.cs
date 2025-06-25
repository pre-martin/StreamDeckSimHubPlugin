// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract partial class Item : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    public ObservableCollection<ConditionExpression> Conditions { get; set; } = [];
}