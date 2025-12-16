// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System;
using System.IO;

namespace StreamDeckSimHub.Installer.Tools
{
    public static class Configuration
    {
        public const string StreamDeckProcessName = "StreamDeck";
        public const string PluginProcessName = "StreamDeckSimHub";

        public const string StreamDeckRegistryFolder = @"HKEY_CURRENT_USER\SOFTWARE\Elgato Systems GmbH\StreamDeck";
        public const string StreamDeckSetupRegistryFolder = @"HKEY_CURRENT_USER\SOFTWARE\Elgato Systems GmbH\StreamDeck (Setup)";
        public const string StreamDeckRegistryInstallFolder = "InstallDir";
        public static readonly string StreamDeckDefaultInstallFolder = Path.Combine(@"C:\", "Program Files", "Elgato", "StreamDeck");

        public static readonly string AppDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly string StreamDeckPluginDir = Path.Combine(AppDataRoaming, "Elgato", "StreamDeck", "Plugins");

        public const string PluginDirName = "net.planetrenner.simhub.sdPlugin";
        public const string PluginZipName = "net.planetrenner.simhub.streamDeckPlugin";

        public const string SimHubRegistryFolder = @"HKEY_CURRENT_USER\Software\SimHub";
        public const string SimHubRegistryInstallFolder = "InstallDirectory";
        public static readonly string SimHubDefaultInstallFolder = Path.Combine(@"C:\", "Program Files (x86)", "SimHub");

        public const string SimHubPluginUrl = "https://github.com/pre-martin/SimHubPropertyServer";
        public static readonly Version RequiredSimHubPluginVersion = new Version(1, 13, 0);
    }
}