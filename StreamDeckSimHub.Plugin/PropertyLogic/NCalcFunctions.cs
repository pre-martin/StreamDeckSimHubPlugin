// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Globalization;
using NCalc;
using NCalc.Exceptions;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Custom functions for NCalc.
/// </summary>
public abstract class NCalcFunctions
{

    public static object? StrFunction(ExpressionFunctionData args)
    {
        if (args.Count() != 1)
        {
            throw new NCalcParserException("Error parsing the expression.",
                new NCalcParserException("The 'str' function requires exactly one argument."));
        }

        return args[0].Evaluate()?.ToString() ?? string.Empty;
    }

    public static object? IntFunction(ExpressionFunctionData args)
    {
        if (args.Count() != 1)
        {
            throw new NCalcParserException("Error parsing the expression.",
                new NCalcParserException("The 'int' function requires exactly one argument."));
        }

        var value = args[0].Evaluate();
        return value is int intValue ? intValue : Convert.ToInt32(value);
    }

    public static object? FormatFunction(ExpressionFunctionData args)
    {
        if (args.Count() < 2)
        {
            throw new NCalcParserException("Error parsing the expression.",
                new NCalcParserException("The 'format' function requires at least two arguments."));
        }

        var format = args[0].Evaluate()?.ToString() ?? string.Empty;
        var parameters = args.Skip(1).Select(arg => arg.Evaluate()).ToArray();
        try
        {
            return string.Format(CultureInfo.CurrentCulture, format, parameters);
        }
        catch (FormatException ex)
        {
            throw new NCalcParserException("Error formatting the string.", ex);
        }
    }
}