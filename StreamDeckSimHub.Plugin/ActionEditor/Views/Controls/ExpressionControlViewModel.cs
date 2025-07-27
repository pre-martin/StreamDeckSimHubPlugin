// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub.ShakeIt;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;

public partial class ExpressionControlViewModel : ObservableObject
{
    private readonly NCalcHandler _ncalcHandler = new();

    public required string ExpressionLabel { get; init; }

    [ObservableProperty] private NCalcHolder _nCalcHolder;
    public required string ExpressionToolTip { get; init; }
    public required string Example { get; init; }
    public required Func<string, Task<IList<Profile>>> FetchShakeItProfilesCallback { get; init; }

    [ObservableProperty] private string? _expressionErrorMessage;

    [ObservableProperty] private int _caretIndex;

    public ExpressionControlViewModel(NCalcHolder nCalcHolder)
    {
        NCalcHolder = nCalcHolder;
        OnExpressionStringChanged(NCalcHolder.ExpressionString); // pre-populate error message
        NCalcHolder.PropertyChanged += NCalcHolderOnPropertyChanged;
    }

    private void NCalcHolderOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NCalcHolder.ExpressionString))
        {
            OnExpressionStringChanged(NCalcHolder.ExpressionString);
        }
    }

    private void OnExpressionStringChanged(string value)
    {
        ExpressionErrorMessage = _ncalcHandler.UpdateNCalcHolder(value, NCalcHolder.ShakeItDictionary, NCalcHolder);
    }

    public int InsertShakeIt(string type, int caretIndex, EffectsContainerBase selectedProfile)
    {
        var prefix = type == "Bass" ? "sib." : "sim.";
        NCalcHolder.ShakeItDictionary[prefix + selectedProfile.Id] = selectedProfile.Name;
        var textToInsert = $"[{prefix}{selectedProfile.Id}.Gain]";
        // Order is important: Update ExpressionString last, because this will trigger the PropertyChanged event
        NCalcHolder.ExpressionString = NCalcHolder.ExpressionString.Insert(caretIndex, textToInsert);
        return textToInsert.Length;
    }
}