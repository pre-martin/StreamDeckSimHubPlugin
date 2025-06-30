// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract partial class Item : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private ConditionsHolder _conditionsHolder = new();

    partial void OnConditionsHolderChanged(ConditionsHolder value)
    {
        value.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
    }
}