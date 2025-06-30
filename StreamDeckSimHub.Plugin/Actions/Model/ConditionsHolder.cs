// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.Model;

public partial class ConditionsHolder : ObservableObject
{
    [ObservableProperty] private string _conditionString = string.Empty;

    public ObservableCollection<string> UsedProperties { get; set; } = [];

    public NCalc.Expression? NCalcExpression { get; set; }
}