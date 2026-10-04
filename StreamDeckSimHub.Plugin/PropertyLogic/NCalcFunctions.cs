// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Globalization;
using NCalc.Exceptions;
using NCalc.Handlers;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Custom functions for NCalc.
/// </summary>
public abstract class NCalcFunctions
{

    public static object? StrFunction(FunctionData args)
    {
        if (args.Count != 1)
        {
            throw new NCalcParserException("Error parsing the expression.",
                new NCalcParserException("The 'str' function requires exactly one argument."));
        }

        return args.Evaluate(0)?.ToString() ?? string.Empty;
    }

    public static object? IntFunction(FunctionData args)
    {
        if (args.Count != 1)
        {
            throw new NCalcParserException("Error parsing the expression.",
                new NCalcParserException("The 'int' function requires exactly one argument."));
        }

        var value = args.Evaluate(0);
        return value is int intValue ? intValue : Convert.ToInt32(value);
    }

    public static object? FormatFunction(FunctionData args)
    {
        if (args.Count < 2)
        {
            throw new NCalcParserException("Error parsing the expression.",
                new NCalcParserException("The 'format' function requires at least two arguments."));
        }

        var format = args.Evaluate(0)?.ToString() ?? string.Empty;
        var parameters = new object[args.Count - 1];
        for (var i = 1; i < args.Count; i++)
        {
            parameters[i - 1] = args.Evaluate(i) ?? string.Empty;
        }
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