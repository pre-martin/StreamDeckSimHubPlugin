// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommandLine;

namespace StreamDeckSimHub;

/// <summary>
/// Command line arguments passed by Stream Deck to this plugin.
/// </summary>
public class StreamDeckOptions
{
    [Option("port", Required = true)]
    public int Port { get; set; }

    [Option("pluginUUID", Required = true)]
    public string PluginUuid { get; set; } = "";

    [Option("registerEvent", Required = true)]
    public string RegisterEvent { get; set; } = "";

    [Option("info", Required = true)]
    public string Info { get; set; } = "";
}
