// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Parses expressions like <c>some.property==5</c> and evaluates their resuls.
/// </summary>
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

    /// <summary>
    /// Parses an expression. The syntax has to be in one of two forms:
    /// <list type="number">
    /// <item>
    /// <c>some.property</c> This is the old format. The property will be compared with <c>>=0</c> against the property value.
    /// </item>
    /// <item>
    /// <c>some.property [condition] [value]</c> The property will be compared with the given condition against the given
    /// value. E.g. <c>some.property >= 2</c>.
    /// </item>
    /// </list>
    /// </summary>
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

        if (matchedCondition.op == ConditionOperator.Between)
        {
            var values = parts[1].Split(";", StringSplitOptions.TrimEntries);
            if (values.Length != 2)
            {
                _logger.LogWarning("Operator 'Between' requires two values separated by semicolon");
            }
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

        // When we arrive here with a SimHub property of type "object", it could be interpreted
        // as "double" or "string" (see PropertyType.ParseFromSimHub and .ParseLiberally).
        // Both values (property value and compare value) have to be of the same type, otherwise they are unequal.

        var compareFunction = expression.Operator.CompareFunction();
        if (expression.Operator != ConditionOperator.Between)
        {
            var compareValue = propertyType.ParseLiberally(expression.CompareValue);
            if (propertyValue.GetType() != compareValue.GetType())
            {
                _logger.LogDebug("Property value and compare value are of different types, returning 'false'");
                return false;
            }
            return compareFunction.Invoke(propertyValue, compareValue, null);
        }

        var values = expression.CompareValue.Split(";", StringSplitOptions.TrimEntries);
        if (values.Length != 2)
        {
            // TODO show error
            return false;
        }

        var compareValue1 = propertyType.ParseLiberally(values[0]);
        var compareValue2 = propertyType.ParseLiberally(values[1]);
        if (propertyValue.GetType() != compareValue1.GetType() || propertyValue.GetType() != compareValue2.GetType())
        {
            _logger.LogDebug("Property value and compare value are of different types, returning 'false'");
            return false;

        }
        return compareFunction.Invoke(propertyValue, compareValue1, compareValue2);
    }
}
