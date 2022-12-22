// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

public class PropertyComparer
{
    private readonly ILogger<PropertyComparer> _logger;

    static PropertyComparer()
    {
        // Gather all ConditionOperators in a list of strings.
        var conditions = Enum.GetValues<ConditionOperator>();
        foreach (var condition in conditions)
        {
            AllConditions.Add((condition.ConditionOperatorString(), condition));
        }
    }

    private static readonly List<(string asString, ConditionOperator op)> AllConditions = new();

    public PropertyComparer(ILogger<PropertyComparer> logger)
    {
        _logger = logger;
    }

    public ConditionExpression Parse(string expression)
    {
        var matchedCondition = AllConditions.FirstOrDefault(tuple => expression.Contains(tuple.asString));
        if (matchedCondition == default(ValueTuple<string, ConditionOperator>))
        {
            // "expression" is not in the form "<property> <ConditionOperator> <value>", so use the old behaviour.
            return new ConditionExpression(expression, ConditionOperator.Gt, "0");
        }

        var parts = expression.Split(matchedCondition.asString, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            _logger.LogWarning("Expected 3 parts in property expression, but found {count}: {expr}. Using default expression.",
                parts.Length + 1, expression);
            return new ConditionExpression(expression, ConditionOperator.Gt, "0");
        }

        return new ConditionExpression(parts[0], matchedCondition.op, parts[1]);
    }

    /// <summary>
    /// Compares a property with a given type and value against an expression.
    /// </summary>
    /// <param name="propertyType">The type of the property</param>
    /// <param name="propertyValue">The value of the property</param>
    /// <param name="expression">The expression (which contains an operation and a compare value) to compare with.</param>
    /// <returns><c>true</c> if the value fulfills the expression.</returns>
    /// <remarks>The value in the expression should be convertible into the property type.</remarks>
    public bool Evaluate(PropertyType propertyType, IComparable? propertyValue, ConditionExpression expression)
    {
        if (propertyValue == null)
        {
            return false;
        }

        var compareFunction = expression.Operator.CompareFunction();
        var compareValue = propertyType.ParseLiberally(expression.CompareValue);
        return compareValue != null && compareFunction.Invoke(propertyValue, compareValue);
    }
}