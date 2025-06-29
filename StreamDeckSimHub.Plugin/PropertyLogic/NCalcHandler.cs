// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NCalc;
using NCalc.Exceptions;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

public class NCalcHandler
{
    /// <summary>
    /// Extracts property names from a given NCalc expression. In the same time, the expression is validated.
    /// </summary>
    /// <throws cref="NCalcParserException">If the expression is invalid.</throws>
    public HashSet<string> ExtractProperties(string expression)
    {
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var ncalcExpression = CreateExpression(expression);
        ncalcExpression.EvaluateParameter += (name, args) =>
        {
            properties.Add(name);
            args.Result = true;
        };

        ncalcExpression.Evaluate();

        return properties;
    }

    private Expression CreateExpression(string expression)
    {
        return new Expression(expression, ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
    }
}