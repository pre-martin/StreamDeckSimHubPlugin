// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NCalc;
using NCalc.Exceptions;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

public class NCalcHandler
{
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
    public string? UpdateNCalcHolder(string expression, NCalcHolder ncalcHolder)
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

    private Expression CreateExpression(string expression)
    {
        return new Expression(expression, ExpressionOptions.IgnoreCaseAtBuiltInFunctions | ExpressionOptions.AllowNullParameter);
    }
}