// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using NCalc;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Holds an (observable) expression as a string as well as optionally the NCalc expression object (if the string is valid),
/// a list of the parameter names used in the expression, and a dictionary for resolving ShakeIt names by their IDs.
/// </summary>
/// <remarks>
/// The ShakeIt dictionary should not be in this class, because it has nothing to do with the NCalc expression.
/// But storing this data here simplifies the code because the resolving of ShakeIt names is only done in the context of editing NCalc expressions.
/// </remarks>
public partial class NCalcHolder : ObservableObject
{
    [ObservableProperty] private string _expressionString = string.Empty;

    public HashSet<string> UsedProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// From <c>sib.guid | sim.guid</c> to the list of effect elements from root to the leaf element.
    /// </summary>
    public Dictionary<string, List<ShakeItEntry>> ShakeItDictionary { get; set; } = new();

    public Expression? NCalcExpression { get; set; }
}

public class ShakeItEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}