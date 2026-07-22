// Copyright (C) 2026 Martin Renner
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
        /// Is a given process running from within a specific directory?
        /// </summary>
        public static bool IsProcessRunningInDirectory(string processName, string directoryPath)
        {
            return GetProcessesInDirectory(processName, directoryPath).Any();
        }

        /// <summary>
        /// Simple wrapper for <c>Process.GetProcessesByName()</c>.
        /// </summary>
        public static Process GetProcess(string processName)
        {
            return Process.GetProcessesByName(processName).FirstOrDefault();
        }

        /// <summary>
        /// Returns all processes with the given name.
        /// </summary>
        public static Process[] GetProcesses(string processName)
        {
            return Process.GetProcessesByName(processName);
        }

        /// <summary>
        /// Returns all processes with the given name that are started from within the given directory.
        /// </summary>
        public static Process[] GetProcessesInDirectory(string processName, string directoryPath)
        {
            var normalizedDirectoryPath = NormalizeDirectoryPath(directoryPath);
            return GetProcesses(processName).Where(process => IsProcessInDirectory(process, normalizedDirectoryPath)).ToArray();
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

        private static bool IsProcessInDirectory(Process process, string normalizedDirectoryPath)
        {
            try
            {
                var executablePath = process.MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(executablePath))
                {
                    return false;
                }

                var normalizedProcessPath = Path.GetFullPath(executablePath);
                return normalizedProcessPath.StartsWith(normalizedDirectoryPath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizeDirectoryPath(string directoryPath)
        {
            var normalizedDirectoryPath = Path.GetFullPath(directoryPath);
            if (!normalizedDirectoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !normalizedDirectoryPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                normalizedDirectoryPath += Path.DirectorySeparatorChar;
            }

            return normalizedDirectoryPath;
        }
    }
}
