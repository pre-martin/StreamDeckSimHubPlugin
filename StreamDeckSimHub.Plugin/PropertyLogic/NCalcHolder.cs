// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using NCalc;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Holds an (observable) expression as a string as well as optionally the NCalc expression object (if the string is valid),
/// a list of the parameter names used in the expression, and a dictionary for resolving ShakeIt names by their IDs.
/// </summary>
public partial class NCalcHolder : ObservableObject
{
    [ObservableProperty] private string _expressionString = string.Empty;

    public HashSet<string> UsedProperties { get; set; } = [];

    public Dictionary<string, string> ShakeItDictionary { get; set; } = new();

    public Expression? NCalcExpression { get; set; }
}