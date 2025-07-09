// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public abstract partial class ItemViewModel : ObservableObject
{
    private readonly NCalcHandler _ncalcHandler = new();
    private readonly Item _model;
    protected readonly Window ParentWindow;

    protected ItemViewModel(Item model, Window parentWindow)
    {
        _model = model;
        ParentWindow = parentWindow;
        _name = model.Name;
        _conditionString = model.NCalcConditionHolder.ExpressionString;

        // Populate the error message if the condition string is invalid, so that we have it right when the view is displayed.
        try
        {
            _ncalcHandler.Parse(_conditionString, out _);
        }
        catch (Exception e)
        {
            ConditionErrorMessage = _ncalcHandler.BuildNCalcErrorMessage(e);
        }
    }

    public abstract ImageSource? Icon { get; }

    public string DisplayName => _model.DisplayName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _name;

    [ObservableProperty] private string _conditionString;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConditionErrorVisibility))]
    private string? _conditionErrorMessage;

    public Visibility ConditionErrorVisibility => ConditionErrorMessage is not null ? Visibility.Visible : Visibility.Collapsed;

    partial void OnNameChanged(string value)
    {
        _model.Name = value;
    }

    partial void OnConditionStringChanged(string value)
    {
        ConditionErrorMessage = _ncalcHandler.UpdateNCalcHolder(value, _model.NCalcConditionHolder);
    }

    public Item GetModel()
    {
        return _model;
    }
}