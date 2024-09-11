// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;

namespace StreamDeckSimHub.Installer.Tools;

public static class Configuration
{
    public const string StreamDeckProcessName = "StreamDeck";
    public const string PluginProcessName = "StreamDeckSimHub";

    public const string StreamDeckRegistryFolder = @"HKEY_CURRENT_USER\SOFTWARE\Elgato Systems GmbH\StreamDeck";

    public static readonly string AppDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static readonly string StreamDeckPluginDir = Path.Combine(AppDataRoaming, "Elgato", "StreamDeck", "Plugins");

    public const string PluginDirName = "net.planetrenner.simhub.sdPlugin";
    public const string PluginZipName = "net.planetrenner.simhub.streamDeckPlugin";
}
