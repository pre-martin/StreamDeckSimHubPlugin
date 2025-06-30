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
        private const string BaseUrl = "https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/8.0.15/";
        private const string InstallerName = "windowsdesktop-runtime-8.0.15-win-x64.exe";
        private const string InstallerHash = "c5f12718adcd48cf8689f080de7799071cbe8f35b0fc9ce7a80f13812137c868004ccd5ea035d8e443216e70e15fdfcf013556c7cb3b1b02636acb0323b3574e";
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
                        if (candidate.Major == _dotnetRequired.Major && candidate >= _dotnetRequired)
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