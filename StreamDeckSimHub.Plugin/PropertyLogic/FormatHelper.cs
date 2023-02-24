// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.RegularExpressions;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Helper class for formatting of string.
/// </summary>
public class FormatHelper
{
    private readonly Regex _formatStringRegex = new Regex("(?<prefix>.*?){(?<format>.+)}(?<suffix>.*)", RegexOptions.Singleline);

    /// <summary>
    /// Properties can be formatted with a format string. To make it easier for the user, there is a "simple format" and
    /// "full format". This method translates both formats into a C# format string. Especially, <c>{0}</c> has to be inserted
    /// correctly.
    /// </summary>
    public string CompleteFormatString(string incompleteFormatString)
    {
        if (string.IsNullOrEmpty(incompleteFormatString))
        {
            // No format string
            return "{0}";
        }

        var match = _formatStringRegex.Match(incompleteFormatString);
        if (!match.Success)
        {
            // Simple format
            if (incompleteFormatString.StartsWith(':')) return "{0" + incompleteFormatString + "}";
            return "{0," + incompleteFormatString + "}";
        }

        // Full format
        var fullFormatString = "";
        if (match.Groups["prefix"].Success) fullFormatString += match.Groups["prefix"].Value;

        fullFormatString += "{0";
        if (match.Groups["format"].Value.StartsWith(':')) fullFormatString += match.Groups["format"].Value;
        else fullFormatString += "," + match.Groups["format"].Value;
        fullFormatString += "}";
        if (match.Groups["suffix"].Success) fullFormatString += match.Groups["suffix"].Value;

        return fullFormatString;
    }
}