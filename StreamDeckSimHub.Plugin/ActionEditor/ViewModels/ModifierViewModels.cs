// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;
using StreamDeckSimHub.Plugin.SimHub.ShakeIt;
using Color = SixLabors.ImageSharp.Color;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public abstract partial class ModifierViewModel : ObservableObject
{
    private readonly Modifier _model;
    private readonly IViewModel _rootViewModel;

    protected ModifierViewModel(Modifier model, IViewModel rootViewModel)
    {
        this._model = model;
        _rootViewModel = rootViewModel;
        _expressionControlConditionViewModel = new ExpressionControlViewModel(model.NCalcConditionHolder)
        {
            ExpressionLabel = "Condition:",
            ExpressionToolTip = "Please enter a valid NCalc expression, that returns true or false or a number",
            Example="[DataCorePlugin.Computed.Fuel_RemainingLaps] <= 2",
            FetchShakeItProfilesCallback = FetchShakeItProfilesCallback
        };
    }

    public abstract ImageSource? Icon { get; }

    public string DisplayName => _model.DisplayName;

    [ObservableProperty] private ExpressionControlViewModel _expressionControlConditionViewModel;

    public Modifier GetModel() => _model;

    private Func<string, Task<IList<Profile>>> FetchShakeItProfilesCallback => FetchShakeItProfiles;

    private async Task<IList<Profile>> FetchShakeItProfiles(string type)
    {
        return type == "Bass"
            ? await _rootViewModel.FetchShakeItBassProfiles()
            : await _rootViewModel.FetchShakeItMotorsProfiles();
    }
}

public partial class ModifierColorViewModel(ModifierColor model, IViewModel rootViewModel)
    : ModifierViewModel(model, rootViewModel), IColorSelectable
{
    public override ImageSource? Icon => null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorHex))]
    [NotifyPropertyChangedFor(nameof(ColorAsWpf))]
    private Color _imageSharpColor = model.Color;

    public string ColorHex => $"#{model.Color.ToHexWithoutAlpha()}";

    public System.Windows.Media.Color ColorAsWpf => ImageSharpColor.ToWpfColor();

    partial void OnImageSharpColorChanged(Color value)
    {
        model.Color = value;
    }
}

public partial class ModifierFlashViewModel(ModifierFlash model, IViewModel rootViewModel)
    : ModifierViewModel(model, rootViewModel)
{
    public override ImageSource? Icon => null;

    [ObservableProperty] private int? _durationOn = model.DurationOn;
    [ObservableProperty] private int? _durationOff = model.DurationOff;


    partial void OnDurationOnChanged(int? value)
    {
        model.DurationOn = value;
    }

    partial void OnDurationOffChanged(int? value)
    {
        model.DurationOff = value;
    }
}