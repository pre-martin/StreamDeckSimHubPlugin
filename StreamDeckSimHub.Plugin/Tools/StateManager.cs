// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NLog;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Tools;

/// <summary>
/// Manages the "State" of a Stream Deck key based on a SimHub property (optionally with an expression).
/// </summary>
public class StateManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly PropertyComparer _propertyComparer;
    private readonly ISimHubConnection _simHubConnection;
    private readonly StateChangedFunc _stateChangedFunc;
    private readonly bool _useCondition;
    private readonly IPropertyChangedReceiver _statePropertyChangedReceiver;
    private PropertyChangedArgs? _lastStatePropertyChangedEvent;
    private ConditionExpression? _conditionExpression;
    public int State { get; private set; } = 1;

    public delegate Task StateChangedFunc(int state);

    /// <param name="propertyComparer">PropertyComparer for parsing and evaluating expressions</param>
    /// <param name="simHubConnection">SimHub Connection to use</param>
    /// <param name="stateChangedFunc">Called whenever the state has to be updated</param>
    /// <param name="useCondition">If "true", the PropertyComparer will be used to determine the state from a condition. If "false", the property value will be used directly as state.</param>
    public StateManager(PropertyComparer propertyComparer, ISimHubConnection simHubConnection, StateChangedFunc stateChangedFunc, bool useCondition = true)
    {
        _propertyComparer = propertyComparer;
        _simHubConnection = simHubConnection;
        _stateChangedFunc = stateChangedFunc;
        _useCondition = useCondition;
        _statePropertyChangedReceiver = new PropertyChangedDelegate(StatePropertyChanged);
    }

    public async Task HandleExpression(string expression, bool forceSubscribe)
    {
        var newCondExpr = _propertyComparer.Parse(expression);

        // Unsubscribe previous SimHub state property, if it was set and is different from the new one.
        if (!string.IsNullOrEmpty(_conditionExpression?.Property) && _conditionExpression.Property != newCondExpr.Property )
        {
            await _simHubConnection.Unsubscribe(_conditionExpression.Property, _statePropertyChangedReceiver);
        }

        // Subscribe SimHub state property, if it is set and different from the previous one.
        if (!string.IsNullOrEmpty(newCondExpr.Property) &&
            (_conditionExpression == null || _conditionExpression.Property != newCondExpr.Property || forceSubscribe))
        {
            await _simHubConnection.Subscribe(newCondExpr.Property, _statePropertyChangedReceiver);
        }

        var recalculateState = !Equals(newCondExpr, _conditionExpression);
        _conditionExpression = newCondExpr;
        if (recalculateState)
        {
            await RefireStatePropertyChanged();
        }
    }

    public async void Deactivate()
    {
        if (!string.IsNullOrEmpty(_conditionExpression?.Property))
        {
            await _simHubConnection.Unsubscribe(_conditionExpression.Property, _statePropertyChangedReceiver);
        }
    }

    private async Task StatePropertyChanged(PropertyChangedArgs args)
    {
        Logger.Debug("Property {PropertyName} changed to '{PropertyValue}'", args.PropertyName, args.PropertyValue);
        _lastStatePropertyChangedEvent = args;
        if (_useCondition)
        {
            var condEval = _conditionExpression == null ||
                           _propertyComparer.Evaluate(args.PropertyType, args.PropertyValue, _conditionExpression);
            State = condEval ? 1 : 0;
        }
        else
        {
            var propertyType = args.PropertyType;
            var propertyValue = args.PropertyValue;
            switch (propertyType)
            {
                case PropertyType.Boolean:
                    State = propertyValue == null ? 0 : (bool)propertyValue ? 1 : 0;
                    break;
                case PropertyType.Integer:
                case PropertyType.Long:
                    State = propertyValue == null ? 0 : (int)propertyValue;
                    break;
                case PropertyType.Double:
                    State = propertyValue == null ? 0 : (int)Math.Round((double)propertyValue, MidpointRounding.AwayFromZero);
                    break;
                case PropertyType.Object:
                default:
                    // "object" as 4-state? for the moment, simply return 0.
                    State = 0;
                    break;
            }
        }

        await _stateChangedFunc(State);
    }

    private async Task RefireStatePropertyChanged()
    {
        if (_lastStatePropertyChangedEvent != null)
        {
            await StatePropertyChanged(_lastStatePropertyChangedEvent);
        }
    }

}