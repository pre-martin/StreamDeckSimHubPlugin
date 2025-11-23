// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Net.Http;
using Newtonsoft.Json;

namespace StreamDeckSimHub.Plugin.Tools.AutoUpdate;

public class AutoUpdater
{
    private const string GitHubApiUrl = "https://api.github.com/repos/pre-martin/StreamDeckSimHubPlugin/releases/latest";
    private const string UserAgent = "StreamDeckSimHubPlugin-Updater";

    /// <summary>
    /// Checks GitHub for the latest version of the plugin.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If deserialization does not return an object</exception>
    public async Task<GitHubVersionInfo> GetLatestVersion()
    {
        HttpResponseMessage response;
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            response = await httpClient.GetAsync(GitHubApiUrl);
            response.EnsureSuccessStatusCode();
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<GitHubVersionInfo>(content) ??
               throw new InvalidOperationException("No version info returned from update check.");
    }
}