// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StreamDeckSimHub.Installer.Tools;

namespace StreamDeckSimHub.Installer.Actions
{
    public class CheckDotnetRuntime : AbstractInstallerAction
    {
        private readonly Regex _dotnetDesktop = new Regex(@"Microsoft.WindowsDesktop.App (\d+\.\d+\.\d+).*", RegexOptions.IgnoreCase);
        public readonly Version DotnetRequired = new Version(8, 0, 21);
        private const string BaseUrl = "https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/8.0.21/";
        private const string InstallerName = "windowsdesktop-runtime-8.0.21-win-x64.exe";
        private const string InstallerHash = "10b837434f08ea2bae299fda5665e4e0539116d52b4101202a1e4255ad40474329d41a82bb3129ce44d61aaa49c92d0b6ae9cdfa04fee88e6239d26beff65775";
        private readonly string _installerFile = Path.Combine(Path.GetTempPath(), InstallerName);

        public override string Name => "Checking .NET Desktop Runtime version";

        protected override async Task<ActionResult> ExecuteInternal()
        {
            if (FindRuntime()) return ActionResult.Success;

            if (!await DownloadRuntime()) return ActionResult.Error;

            return Install() ? ActionResult.Success : ActionResult.Warning;
        }

        private bool FindRuntime()
        {
            try
            {
                var exitCode = ProcessTools.RunCommand($"dotnet --list-runtimes", out var output);
                LogInfo($"\"dotnet --list-runtimes\" exited with code {exitCode}");
                var result = false;
                foreach (var line in output)
                {
                    var match = _dotnetDesktop.Match(line);
                    if (match.Groups.Count >= 2)
                    {
                        var candidate = new Version(match.Groups[1].Value);
                        if (candidate.Major == DotnetRequired.Major && candidate >= DotnetRequired)
                        {
                            SetAndLogInfo($"Found .NET Desktop Runtime version {candidate}");
                            result = true;
                            // Continue to check for other versions, so we get all versions in the log.
                        }
                    }
                }

                if (result) return true;

                SetAndLogInfo(".NET Desktop Runtime not found");
                return false;
            }
            catch (Exception e)
            {
                SetAndLogError(e, "Failed to determine installed .NET Desktop Runtime");
                return false;
            }
        }

        private async Task<bool> DownloadRuntime()
        {
            try
            {
                SetAndLogInfo($"Downloading .NET Desktop Runtime {DotnetRequired}");

                var webClient = new WebClient();
                await webClient.DownloadFileTaskAsync(BaseUrl + InstallerName, _installerFile);
                using (var sha512 = SHA512.Create())
                {
                    using (var fileStream = File.OpenRead(_installerFile))
                    {
                        var calculatedChecksum = sha512.ComputeHash(fileStream);
                        var calculatedChecksumString = BitConverter.ToString(calculatedChecksum).Replace("-", string.Empty).ToLowerInvariant();
                        if (calculatedChecksumString != InstallerHash)
                        {
                            SetAndLogError("Invalid checksum for downloaded file");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                SetAndLogError(e, "Failed to download .NET Desktop Runtime");
                return false;
            }
        }

        private bool Install()
        {
            SetAndLogInfo($"Installing .NET Desktop Runtime {DotnetRequired}");
            // see https://learn.microsoft.com/en-us/dotnet/core/install/windows#command-line-options
            var exitCode = ProcessTools.RunCommand($"{_installerFile} /install /quiet /norestart", out var output);
            LogInfo($"Installer exited with code {exitCode}");
            File.Delete(_installerFile);
            if (exitCode == 0 || exitCode == 3010)
            {
                SetAndLogInfo($"Installed .NET Desktop Runtime {DotnetRequired}");
                return true;
            }

            SetAndLogInfo($"Possible probleme while installing .NET Desktop Runtime {DotnetRequired}: Exit code {exitCode}");
            return false;
        }
   }
}