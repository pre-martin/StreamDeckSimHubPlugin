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
        private readonly Version _dotnetRequired = new Version(8, 0, 10);
        private const string BaseUrl = "https://download.visualstudio.microsoft.com/download/pr/f398d462-9d4e-4b9c-abd3-86c54262869a/4a8e3a10ca0a9903a989578140ef0499/";
        private const string InstallerName = "windowsdesktop-runtime-8.0.10-win-x64.exe";
        private const string InstallerHash = "914fb306fb1308c59e293d86c75fc4cca2cc72163c2af3e6eed0a30bec0a54a8f95d22ec6084fd9e1579cb0576ffa0942f513b7b4c6b4c3a2bc942fe21f0461d";
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
                foreach (var line in output)
                {
                    var match = _dotnetDesktop.Match(line);
                    if (match.Groups.Count >= 2)
                    {
                        var candidate = new Version(match.Groups[1].Value);
                        if (candidate >= _dotnetRequired)
                        {
                            SetAndLogInfo($"Found .NET Desktop Runtime version {candidate}");
                            return true;
                        }
                    }
                }

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
                SetAndLogInfo($"Downloading .NET Desktop Runtime {_dotnetRequired}");

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
            SetAndLogInfo($"Installing .NET Desktop Runtime {_dotnetRequired}");
            // see https://learn.microsoft.com/en-us/dotnet/core/install/windows#command-line-options
            var exitCode = ProcessTools.RunCommand($"{_installerFile} /install /quiet /norestart", out var output);
            LogInfo($"Installer exited with code {exitCode}");
            File.Delete(_installerFile);
            if (exitCode == 0 || exitCode == 3010)
            {
                SetAndLogInfo($"Installed .NET Desktop Runtime {_dotnetRequired}");
                return true;
            }

            SetAndLogInfo($"Possible probleme while installing .NET Desktop Runtime {_dotnetRequired}: Exit code {exitCode}");
            return false;
        }
   }
}