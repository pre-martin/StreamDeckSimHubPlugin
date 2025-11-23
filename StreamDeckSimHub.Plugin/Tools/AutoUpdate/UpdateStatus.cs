// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Tools.AutoUpdate;

/// <summary>
/// Stores the latest version information and any exception that occurred during update check.
/// </summary>
public static class UpdateStatus
{
    /// <summary>
    /// The latest version available, or null if not available.
    /// </summary>
    public static Version? LatestVersion { get; set; }

    /// <summary>
    /// The exception that occurred during update check, or null if successful.
    /// </summary>
    public static Exception? LatestVersionException { get; set; }
}