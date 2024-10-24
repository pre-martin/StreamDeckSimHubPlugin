// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StreamDeckSimHub.Installer.Tools
{
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
        public static Process GetProcess(string processName)
        {
            return Process.GetProcessesByName(processName).FirstOrDefault();
        }

        /// <summary>
        /// Starts a new process.
        /// </summary>
        public static void StartProcess(string fileName, string workingDirectory = null)
        {
            var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory();
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }

        /// <summary>
        /// Runs a command via <c>cmd.exe</c> and returns its exit code as well as its output as a string array - each line
        /// one entry in the array.
        /// </summary>
        public static int RunCommand(string command, out string[] output)
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {command}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            var stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            output = stdout.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return process.ExitCode;
        }
    }
}