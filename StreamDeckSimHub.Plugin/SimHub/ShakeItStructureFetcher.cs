// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using NLog;
using StreamDeckSimHub.Plugin.SimHub.ShakeIt;

namespace StreamDeckSimHub.Plugin.SimHub;

/// <summary>
/// Fetches the ShakeIt Bass structure from the SimHub Property Server.
/// </summary>
public class ShakeItStructureFetcher(IOptions<ConnectionSettings> connectionSettings)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ConnectionSettings _connectionSettings = connectionSettings.Value;

    /// <summary>
    /// Fetches the ShakeIt Bass structure from the SimHub Property Server.
    /// </summary>
    public async Task<IList<Profile>> FetchBassStructure()
    {
        return await FetchStructure("shakeit-bass-structure", "Bass");
    }

    /// <summary>
    /// Fetches the ShakeIt Motors structure from the SimHub Property Server.
    /// </summary>
    public async Task<IList<Profile>> FetchMotorsStructure()
    {
        return await FetchStructure("shakeit-motors-structure", "Motors");
    }

    private async Task<IList<Profile>> FetchStructure(string command, string loggingName)
    {
        Logger.Info($"Requesting ShakeIt {loggingName} structure");
        var shakeItParser = new ShakeItParser();

        using var tcpClient = new TcpClient();
        var connectTask = tcpClient.ConnectAsync(_connectionSettings.Host, _connectionSettings.Port);
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(4));
        if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
        {
            Logger.Warn("Timeout while connecting to SimHub Property Server");
            throw new TimeoutException("Timeout while connecting to SimHub Property Server");
        }

        await using var stream = tcpClient.GetStream();
        using var reader = new StreamReader(stream);
        await using var writer = new StreamWriter(stream);

        var line = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(4)).ConfigureAwait(false);
        if (line != null && line.StartsWith("SimHub Property Server"))
        {
            Logger.Info("Connected to SimHub Property Server");
            await WriteLine(writer, command);

            var receivedHeader = false;
            var receivedEnd = false;
            while ((line = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(4)).ConfigureAwait(false)) != null)
            {
                if (line.StartsWith("ShakeIt Bass structure") || line.StartsWith("ShakeIt Motors structure"))
                {
                    receivedHeader = true;
                }
                else if (line.StartsWith("End"))
                {
                    receivedEnd = true;
                    break;
                }
                else if (receivedHeader)
                {
                    shakeItParser.ParseLine(line);
                }
                else
                {
                    Logger.Warn($"Received unknown data from server. Ignoring.");
                }
            }

            await WriteLine(writer, "disconnect");

            if (!receivedEnd)
            {
                Logger.Warn("ShakeIt {loggingName} structure was not received. Aborting.");
                return new List<Profile>();
            }

            var profiles = shakeItParser.Profiles;
            Logger.Info($"Successfully parsed ShakeIt {loggingName} structure with {profiles.Count} profiles");
            return profiles;
        }

        return new List<Profile>();
    }

    private async Task WriteLine(StreamWriter writer, string line)
    {
        await writer.WriteAsync(line);
        await writer.WriteAsync("\r\n");
        await writer.FlushAsync();
    }
}