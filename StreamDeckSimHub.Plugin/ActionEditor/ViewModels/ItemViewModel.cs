// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public abstract partial class ItemViewModel : ObservableObject
{
    private readonly NCalcHandler _ncalcHandler = new();
    private readonly Item _model;

    protected ItemViewModel(Item model)
    {
        _model = model;
        _name = model.Name;
        _conditionString = model.ConditionsHolder.ConditionString;

        try
        {
            _ncalcHandler.Parse(_conditionString, out _);
        }
        catch (Exception e)
        {
            ErrorMessage = BuildNCalcErrorMessage(e);
        }
    }

    /// <summary>
    /// How shall the element be displayed/called in the UI?
    /// </summary>
    public abstract string DisplayName { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _name;

    [ObservableProperty] private string _conditionString;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorVisibility))]
    private string? _errorMessage;

    public Visibility ErrorVisibility => ErrorMessage is not null ? Visibility.Visible : Visibility.Collapsed;

    partial void OnNameChanged(string value)
    {
        _model.Name = value;
    }

    partial void OnConditionStringChanged(string value)
    {
        // Update ConditionString in any case...
        _model.ConditionsHolder.ConditionString = value;

        ErrorMessage = null;
        try
        {
            var usedProperties = _ncalcHandler.Parse(value, out var ncalcExpression);
            // ...update NCalcExpression and UsedProperties only if parsing was successful
            _model.ConditionsHolder.NCalcExpression = ncalcExpression;
            _model.ConditionsHolder.UsedProperties = new ObservableCollection<string>(usedProperties);
        }
        catch (Exception e)
        {
            ErrorMessage = BuildNCalcErrorMessage(e);
        }
    }

    private string BuildNCalcErrorMessage(Exception e)
    {
        var msg = e.Message;
        if (e.InnerException != null)
        {
            msg += "\n" + e.InnerException.Message;
        }

        return msg;
    }

    public Item GetModel()
    {
        return _model;
    }
}