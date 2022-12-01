// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using NLog;

namespace StreamDeckSimHub;

/// <summary>
/// Manages the TCP connection to SimHub. The class automatically manages reconnects.
/// </summary>
/// <remarks>The plugin "SimHubPropertyServer" is required to be installed in SimHub.</remarks>
public class SimHubConnection
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private TcpClient? _tcpClient;
    private long _connected;
    private readonly HashSet<string> _subscriptions = new();

    private bool Connected
    {
        get => Interlocked.Read(ref _connected) == 1;
        set => Interlocked.Exchange(ref _connected, Convert.ToInt64(value));
    }


    /// <summary>
    /// Parameters of the event, that gets fired when a new property value was received from SimHub.
    /// </summary>
    public class PropertyChangedEventArgs : EventArgs
    {
        public string PropertyName { get; init; } = string.Empty;
        public string PropertyType { get; init; } = string.Empty;
        public string? PropertyValue { get; init; } = string.Empty;
    }

    public event EventHandler<PropertyChangedEventArgs>? PropertyChangedEvent;

    public async void Run()
    {
        await ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        Logger.Info("Connecting to SimHub (Property Server plugin has to be installed in SimHub)...");
        Connected = false;

        while (!Connected)
        {
            _tcpClient = new TcpClient { ReceiveTimeout = (int)TimeSpan.FromSeconds(2).TotalMilliseconds };
            try
            {
                await _tcpClient.ConnectAsync("localhost", 18082).WaitAsync(TimeSpan.FromSeconds(4));
                var line = await new LineReader(_tcpClient.GetStream()).ReadLineAsync();
                if (line != null && line.StartsWith("SimHub Property Server"))
                {
                    Logger.Info($"Established connection to {line}");
                    _tcpClient.ReceiveTimeout = 0;
                    Connected = true;
                    foreach (var propertyName in _subscriptions)
                    {
                        await SendSubscribe(propertyName);
                    }
                    await ReadFromServer();
                }
            }
            catch (SocketException se)
            {
                Logger.Info($"Connection failed: {se.Message}");
            }
            catch (TimeoutException)
            {
                // Ignore exception and try again to connect.
            }

            if (!Connected)
            {
                Logger.Trace("Connection could not be opened. Waiting and trying again...");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }

    internal async Task Subscribe(string propertyName)
    {
        if (_subscriptions.Contains(propertyName))
        {
            Logger.Info($"Already subscribed to {propertyName}, ignoring");
            return;
        }

        _subscriptions.Add(propertyName);
        if (Connected)
        {
            await SendSubscribe(propertyName);
        }
        else
        {
            Logger.Info($"Queuing subscribe request for {propertyName}");
        }
    }

    internal async Task Unsubscribe(string propertyName)
    {
        var wasSubscribed = _subscriptions.Remove(propertyName);
        if (!wasSubscribed) return;

        if (Connected)
        {
            await SendUnsubscribe(propertyName);
        }
        else
        {
            Logger.Info($"Queuing unsubscribe request for {propertyName}");
        }
    }

    private async Task SendSubscribe(string propertyName)
    {
        Logger.Info($"Sending subscribe for {propertyName}");
        await WriteToServer($"subscribe {propertyName}");
    }

    private async Task SendUnsubscribe(string propertyName)
    {
        Logger.Info($"Sending unsubscribe from {propertyName}");
        await WriteToServer($"unsubscribe {propertyName}");
    }

    private void ParseProperty(string line)
    {
        var lineItems = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (lineItems.Length != 4)
        {
            Logger.Warn($"Could not parse property: {line}");
            return;
        }

        var name = lineItems[1];
        var type = lineItems[2];
        var value = lineItems[3];
        if (value == "(null)") value = null;
        PropertyChangedEvent?.Invoke(this,
            new PropertyChangedEventArgs { PropertyName = name, PropertyType = type, PropertyValue = value });
    }

    private async Task ReadFromServer()
    {
        Debug.Assert(_tcpClient != null, nameof(_tcpClient) + " != null");
        try
        {
            var reader = new LineReader(_tcpClient.GetStream());
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                Logger.Info($"Received from server: {line}");
                if (line.StartsWith("Property "))
                {
                    ParseProperty(line);
                }
            }
            // "line == null": End of stream. Fall through to "CloseAndReconnect".
            Logger.Info("Server closed connection");
        }
        catch (IOException ioe)
        {
            // IOException: Fall through to "CloseAndReconnect".
            Logger.Warn($"Received IOException while waiting for data: {ioe.Message}");
        }

        await CloseAndReconnect();
    }

    private async Task WriteToServer(string line)
    {
        if (!Connected || _tcpClient == null || !_tcpClient.Connected)
        {
            Logger.Warn(
                $"Cannot send to server (connected: {Connected}, tcpClient: {_tcpClient != null}, tcpClient.Connected: {_tcpClient?.Connected}");
            return;
        }

        var writer = new LineWriter(_tcpClient.GetStream());
        try
        {
            await writer.WriteLineAsync(line);
        }
        catch (IOException ioe)
        {
            Logger.Warn($"Received IOException while writing data: {ioe.Message}");
            await CloseAndReconnect();
        }
    }

    private async Task CloseAndReconnect()
    {
        _tcpClient?.Close();
        await ConnectAsync();
    }

    /// <summary>
    /// LineReader is reading lines from a stream. The lifecycle of the underlying stream has to be managed outside of this class!
    /// </summary>
    private class LineReader
    {
        private readonly StreamReader _streamReader;

        internal LineReader(Stream stream)
        {
            _streamReader = new StreamReader(stream, Encoding.ASCII);
        }

        internal async Task<string?> ReadLineAsync()
        {
            return await _streamReader.ReadLineAsync();
        }
    }

    /// <summary>
    /// LineWriter is writing lines to a stream. The lifecycle of the underlying stream has to be managed outside of this class!
    /// </summary>
    private class LineWriter
    {
        private readonly StreamWriter _streamWriter;

        internal LineWriter(Stream stream)
        {
            _streamWriter = new StreamWriter(stream, Encoding.ASCII);
        }

        internal async Task WriteLineAsync(string line)
        {
            await _streamWriter.WriteAsync(line);
            await _streamWriter.WriteAsync("\r\n");
            await _streamWriter.FlushAsync();
        }
    }
}