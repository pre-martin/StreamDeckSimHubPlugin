// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Text.RegularExpressions;

namespace StreamDeckSimHub.Plugin.PropertyLogic;

/// <summary>
/// Helper class for formatting of string.
/// </summary>
public partial class FormatHelper
{
    private readonly Regex _formatStringRegex = FormatStringRegex();

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
            if (incompleteFormatString.StartsWith(':')) return "{0" + incompleteFormatString + "}"; // ":F0" -> "{0:F0}"
            return "{0," + incompleteFormatString + "}"; // "-8:F2" -> "{0,-8:F2}"
        }

        // Full format: "bla {format} bla"
        var fullFormatString = "";
        if (match.Groups["prefix"].Success) fullFormatString += match.Groups["prefix"].Value;

        fullFormatString += "{0";
        if (match.Groups["format"].Value.StartsWith(':')) fullFormatString += match.Groups["format"].Value; // ":F0" -> "{0:F0}"
        else fullFormatString += "," + match.Groups["format"].Value; // "-8:F2" -> "{0,-8:F2}"
        fullFormatString += "}";
        if (match.Groups["suffix"].Success) fullFormatString += match.Groups["suffix"].Value;

        return fullFormatString;
    }

    [GeneratedRegex("(?<prefix>.*?){(?<format>.+)}(?<suffix>.*)", RegexOptions.Singleline)]
    private static partial Regex FormatStringRegex();
}