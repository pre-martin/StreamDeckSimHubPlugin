// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub.ShakeIt;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;

/// <summary>
/// ViewModel that is wrapping an instance of NCalcHolder. It adds a layer of abstraction for the expression string
/// (so that we can show ShakeIt properties more user-friendly), and it adds displaying the validation results.
/// </summary>
public partial class ExpressionControlViewModel : ObservableObject
{
    private readonly NCalcHandler _ncalcHandler = new();

    public required string ExpressionLabel { get; init; }

    [ObservableProperty] private NCalcHolder _nCalcHolder;
    [ObservableProperty] private string _expressionStringDisplay = string.Empty;
    public required string ExpressionToolTip { get; init; }
    public required string Example { get; init; }
    public required Func<string, Task<IList<Profile>>> FetchShakeItProfilesCallback { get; init; }

    [ObservableProperty] private string? _expressionErrorMessage;

    public ExpressionControlViewModel(NCalcHolder nCalcHolder)
    {
        NCalcHolder = nCalcHolder;
        ExpressionStringDisplay = NCalcHolder.ExpressionString; // this also pre-populates the error message
    }

    partial void OnExpressionStringDisplayChanged(string value)
    {
        NCalcHolder.ExpressionString = value;
        ExpressionErrorMessage = _ncalcHandler.UpdateNCalcHolder(value, NCalcHolder);
    }

    public int InsertShakeIt(string type, int caretIndex, EffectsContainerBase selectedEffect)
    {
        var prefix = type == "Bass" ? "sib" : "sim";
        NCalcHolder.ShakeItDictionary[$"{prefix}.{selectedEffect.Id}"] = selectedEffect.Name;

        var textToInsert = $"[{prefix}.{selectedEffect.Id}.Gain]";

        // Order is important: Update ExpressionString (via ExpressionStringDisplay) last, because this will trigger the
        // PropertyChanged event of NCalcHolder
        ExpressionStringDisplay = NCalcHolder.ExpressionString.Insert(caretIndex, textToInsert);
        return textToInsert.Length;
    }
}