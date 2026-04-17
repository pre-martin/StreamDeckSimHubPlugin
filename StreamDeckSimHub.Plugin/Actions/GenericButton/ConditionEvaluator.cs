// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton;

/// <summary>
/// Helper class for evaluating conditions on Items and Modifiers.
/// Centralizes the logic for checking active states and evaluating expressions.
/// </summary>
public class ConditionEvaluator
{
    private readonly NCalcHandler _ncalcHandler;
    private readonly GetPropertyDelegate _getPropertyDelegate;
    private readonly Func<string> _coordinatesProvider;

    /// <summary>
    /// Creates a new instance of the ConditionEvaluator.
    /// </summary>
    /// <param name="ncalcHandler">The NCalc handler for expression evaluation.</param>
    /// <param name="getPropertyDelegate">Delegate to retrieve property values.</param>
    /// <param name="coordinatesProvider">Function that provides current coordinates for logging.</param>
    public ConditionEvaluator(
        NCalcHandler ncalcHandler,
        GetPropertyDelegate getPropertyDelegate,
        Func<string> coordinatesProvider)
    {
        _ncalcHandler = ncalcHandler;
        _getPropertyDelegate = getPropertyDelegate;
        _coordinatesProvider = coordinatesProvider;
    }

    /// <summary>
    /// Evaluates the condition of an Item. If the result is true or a positive number, the item is considered active.
    /// </summary>
    public bool IsItemActive(Item item)
    {
        if (item.NCalcConditionHolder.NCalcExpression == null) return true; // No condition means always active
        var value = Evaluate(item.NCalcConditionHolder, $"Visibility of \"{item.DisplayName}\"");
        return value is true or > 0 or > 0.0f or > 0.0d;
    }

    /// <summary>
    /// Evaluates the condition of a Modifier. If the result is true or a positive number, the modifier is considered active.
    /// </summary>
    public bool IsModifierActive(Modifier modifier)
    {
        if (modifier.NCalcConditionHolder.NCalcExpression == null) return true; // No condition means always active
        var value = Evaluate(modifier.NCalcConditionHolder, $"Modifier of \"{modifier.DisplayName}\"");
        return value is true or > 0 or > 0.0f or > 0.0d;
    }

    public object? Evaluate(NCalcHolder nCalcHolder, string loggingContext)
    {
        return _ncalcHandler.EvaluateExpression(
            nCalcHolder,
            _getPropertyDelegate,
            $"({_coordinatesProvider})   {loggingContext}");
    }
}