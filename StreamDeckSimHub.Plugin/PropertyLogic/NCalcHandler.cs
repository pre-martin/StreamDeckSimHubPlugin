// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NCalc;
using NCalc.Exceptions;
using NLog;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Delegate to retrieve a property value by its name.
/// </summary>
public delegate IComparable? GetPropertyDelegate(string propertyName);

public class NCalcHandler
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Parses the given expression into a NCalc expression and extracts the property names used in the expression.
    /// </summary>
    /// <throws cref="NCalcParserException">If the expression is invalid. In this case, the returned Set is empty and the
    /// NCalcExpression is <c>null</c>.</throws>
    public HashSet<string> Parse(string expression, out Expression? ncalcExpression)
    {
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(expression))
        {
            ncalcExpression = null;
            return properties;
        }

        var localNcalcExpression = CreateExpression(expression);
        var parameters = localNcalcExpression.GetParameterNames().Where(p => p != "null");

        ncalcExpression = localNcalcExpression;
        return new HashSet<string>(parameters, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Updates a NCalcHolder with the expression. If the expression is valid, the NCalcExpression and UsedProperties are updated too.
    /// </summary>
    /// <returns><c>null</c> if the expression could be parsed successfully, otherwise an error message.</returns>
    public string? UpdateNCalcHolder(string expression, Dictionary<string, string> shakeItDictionary, NCalcHolder ncalcHolder)
    {
        string? errorMessage = null;

        // Important: Update the NCalcExpression and UsedProperties first, and the ExpressionString last. The reason is that
        // our model has PropertyChanged event handlers that listen only to the ExpressionString.

        try
        {
            var usedProperties = Parse(expression, out var ncalcExpression);
            // Update NCalcExpression and UsedProperties only if parsing was successful ...
            ncalcHolder.NCalcExpression = ncalcExpression;
            ncalcHolder.UsedProperties = usedProperties;
            ncalcHolder.ShakeItDictionary = shakeItDictionary;
        }
        catch (Exception e)
        {
            errorMessage = BuildNCalcErrorMessage(e);
        }

        // ... then update the ExpressionString in any case
        ncalcHolder.ExpressionString = expression;

        return errorMessage;
    }

    /// <summary>
    /// Constructs an error message from the NCalc exception. If the <c>InnerException</c> is set (which is usually the case),
    /// a second line with the inner exception message is added.
    /// </summary>
    public string BuildNCalcErrorMessage(Exception e)
    {
        var msg = e.Message;
        if (e.InnerException != null)
        {
            msg += "\n" + e.InnerException.Message;
        }

        return msg;
    }

    /// <summary>
    /// Evaluates the given NCalc expression by using the given delegate to retrieve the property values.
    /// </summary>
    public object? EvaluateExpression(NCalcHolder nCalcHolder, GetPropertyDelegate getProperty, string loggingContext)
    {
        if (nCalcHolder.NCalcExpression == null) return null;

        var expression = nCalcHolder.NCalcExpression;

        lock (expression)
        {
            // Are the parameters of the NCalc expression up to date?
            if (expression.DynamicParameters.Count != nCalcHolder.UsedProperties.Count)
            {
                expression.DynamicParameters.Clear();
                foreach (var usedProperty in nCalcHolder.UsedProperties)
                {
                    expression.DynamicParameters[usedProperty] = _ => getProperty.Invoke(usedProperty);
                }
            }
        }

        try
        {
            var result = expression.Evaluate();
            if (_logger.IsDebugEnabled)
            {
                var msg = $"{loggingContext}: ";
                msg += $"\"{expression.ExpressionString}\" => \"{result}\", ";
                msg += "parameters: ";
                msg = nCalcHolder.UsedProperties.Aggregate(msg, (current, propName) =>
                    current + $"\"{propName}\"=\"{getProperty.Invoke(propName)}\", ");
                _logger.Debug(msg);
            }

            return result;
        }
        catch (Exception e)
        {
            _logger.Warn($"{loggingContext} Error evaluating expression: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Specialized version of <see cref="EvaluateExpression"/> that evaluates if the given expression
    /// (which should be a condition) is "active".
    /// </summary>
    public bool IsConditionActive(NCalcHolder nCalcConditionHolder, GetPropertyDelegate getProperty, string loggingContext)
    {
        if (nCalcConditionHolder.NCalcExpression == null)
        {
            _logger.Debug($"{loggingContext}: No condition set, always active.");
            return true; // No condition means always active.
        }

        var value = EvaluateExpression(nCalcConditionHolder, getProperty, loggingContext);
        return value is true or > 0 or > 0.0f or > 0.0d;
    }

    private Expression CreateExpression(string expression)
    {
        return new Expression(expression, ExpressionOptions.IgnoreCaseAtBuiltInFunctions | ExpressionOptions.AllowNullParameter);
    }
}