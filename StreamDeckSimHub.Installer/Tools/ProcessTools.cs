// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Diagnostics;
using System.IO;

namespace StreamDeckSimHub.Installer.Tools;

public static class ProcessTools
{
    /// <summary>
    /// Is a given process running?
    /// </summary>
    public static bool IsProcessRunning(string processName)
    {
        return GetProcess(processName) != null;
    }

    /// <summary>
    /// Simple wrapper for <c>Process.GetProcessesByName()</c>.
    /// </summary>
    public static Process? GetProcess(string processName)
    {
        return Process.GetProcessesByName(processName).FirstOrDefault();
    }

    /// <summary>
    /// Starts a new process.
    /// </summary>
    public static void StartProcess(string fileName, string? workingDirectory = null)
    {
        var process = new Process();
        process.StartInfo.FileName = fileName;
        process.StartInfo.WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory();
        process.StartInfo.UseShellExecute = true;
        process.Start();
    }
}