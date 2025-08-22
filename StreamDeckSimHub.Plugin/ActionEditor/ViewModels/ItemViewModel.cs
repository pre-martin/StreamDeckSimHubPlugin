// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.SimHub.ShakeIt;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public abstract partial class ItemViewModel : ObservableObject
{
    private readonly Item _model;
    protected readonly IViewModel ParentViewModel;

    protected ItemViewModel(Item model, IViewModel parentViewModel)
    {
        _model = model;
        ParentViewModel = parentViewModel;
        _name = model.Name;
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _name;

    [ObservableProperty] private ExpressionControlViewModel _expressionControlConditionViewModel;

    partial void OnNameChanged(string value)
    {
        _model.Name = value;
    }

    public Item GetModel()
    {
        return _model;
    }

    protected Func<string, Task<IList<Profile>>> FetchShakeItProfilesCallback => FetchShakeItProfiles;

    private async Task<IList<Profile>> FetchShakeItProfiles(string type)
    {
        return type == "Bass" ? await ParentViewModel.FetchShakeItBassProfiles()
                              : await ParentViewModel.FetchShakeItMotorsProfiles();
    }
}