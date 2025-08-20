// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub.ShakeIt;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;

/// <summary>
/// ViewModel that is wrapping an instance of NCalcHolder.
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
    public ObservableCollection<string> ShakeItLegend { get; } = [];

    public ExpressionControlViewModel(NCalcHolder nCalcHolder)
    {
        NCalcHolder = nCalcHolder;
        ExpressionStringDisplay = NCalcHolder.ExpressionString; // this also pre-populates the error message
    }

    partial void OnExpressionStringDisplayChanged(string value)
    {
        ExpressionErrorMessage = _ncalcHandler.UpdateNCalcHolder(value, NCalcHolder);
        BuildShakeItLegend();
    }

    public int InsertShakeIt(string type, int caretIndex, EffectsContainerBase selectedEffect)
    {
        var prefix = type == "Bass" ? "sib" : "sim";

        // Build the effect tree from the selected effect up to the root
        var effectsPath = new List<ShakeItEntry>();
        IEffectElement? currentEffect = selectedEffect;
        while (currentEffect != null)
        {
            effectsPath.Add(new ShakeItEntry { Id = currentEffect.Id, Name = currentEffect.Name });
            currentEffect = currentEffect.Parent;
        }

        // then reverse the list to have the root effect first
        effectsPath.Reverse();

        NCalcHolder.ShakeItDictionary[$"{prefix}.{selectedEffect.Id}"] = effectsPath;

        var textToInsert = $"[{prefix}.{selectedEffect.Id}.Gain]";

        // Order is important: Update ExpressionString (via ExpressionStringDisplay) last, because this will trigger the
        // PropertyChanged event of NCalcHolder
        ExpressionStringDisplay = NCalcHolder.ExpressionString.Insert(caretIndex, textToInsert);
        return textToInsert.Length;
    }

    private void BuildShakeItLegend()
    {
        ShakeItLegend.Clear();
        foreach (var usedProperty in NCalcHolder.UsedProperties)
        {
            if (!usedProperty.StartsWith("sib.") && !usedProperty.StartsWith("sim."))
            {
                continue; // only interested in ShakeIt properties
            }

            var parts = usedProperty.Split('.');
            if (parts.Length < 2) continue; // Invalid entry. Should not happen.

            var prefix = parts[0]; // sib or sim
            var guid = parts[1];   // the guid part

            if (NCalcHolder.ShakeItDictionary.TryGetValue($"{prefix}.{guid}", out var shakeItEntries))
            {
                var name = shakeItEntries.Aggregate(string.Empty, (current, entry) => current + " / " + entry.Name);
                ShakeItLegend.Add($"{prefix}.{guid} = {name[3..]}");
            }
        }
    }
}