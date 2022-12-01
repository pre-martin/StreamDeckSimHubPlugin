// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommandLine;
using NLog;
using StreamDeckSimHub;

// Main entry. Started by Stream Deck with appropriate arguments.

var logger = LogManager.GetCurrentClassLogger();

logger.Info($"Plugin loading with args: {string.Join(" ", args)}");

for (var count = 0; count < args.Length; count++)
{
    if (args[count].StartsWith("-") && !args[count].StartsWith("--"))
    {
        args[count] = $"-{args[count]}";
    }
}

var parser = new Parser(config =>
{
    config.EnableDashDash = true;
    config.CaseSensitive = false;
    config.CaseInsensitiveEnumValues = true;
    config.IgnoreUnknownArguments = true;
    config.HelpWriter = Console.Error;
});

var options = parser.ParseArguments<StreamDeckOptions>(args);
options.WithParsed(Plugin.Run);