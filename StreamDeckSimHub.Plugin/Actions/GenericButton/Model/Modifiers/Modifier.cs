// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

public abstract partial class Modifier : ObservableObject
{
    [ObservableProperty] private NCalcHolder _nCalcConditionHolder;

    protected Modifier()
    {
        // Set in constructor via the generated property to ensure that OnNCalcPropertyHolderChanged is called.
        NCalcConditionHolder = new NCalcHolder();
    }

    public virtual string DisplayName => GetType().Name;

    partial void OnNCalcConditionHolderChanged(NCalcHolder value)
    {
        value.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
        // No event handler on UsedProperties.CollectionChanged.
        // We rely only on the event of NCalcHolder.ExpressionString. This means that UsedProperties already has to contain
        // the new state when ExpressionString is being updated.
        //value.UsedProperties.CollectionChanged += (_, _) => OnPropertyChanged(nameof(NCalcHolder.UsedProperties));
    }
}