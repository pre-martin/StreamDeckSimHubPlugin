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
        localNcalcExpression.EvaluateParameter += (name, args) =>
        {
            properties.Add(name);
            args.Result = 1;
        };

        localNcalcExpression.Evaluate();

        ncalcExpression = localNcalcExpression;
        return properties;
    }

    private Expression CreateExpression(string expression)
    {
        return new Expression(expression, ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
    }
}