// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using NLog;

namespace StreamDeckSimHub.Plugin.SimHub;


/// <summary>
/// Parameters of the event, that gets fired when a new property value was received from SimHub.
/// </summary>
public class PropertyChangedArgs
{
    public string PropertyName { get; init; } = string.Empty;
    public PropertyType PropertyType { get; init; }
    public IComparable? PropertyValue { get; init; }
}

/// <summary>
/// This interface has to be implemented in order to receive events, when a new property value was received from SimHub.
/// </summary>
public interface IPropertyChangedReceiver
{
    /// <summary>
    /// Gets called with the data about the new property value.
    /// </summary>
    /// <remarks>
    /// Implementors should return fast. If more work has to be done, a Task should be started internally.
    /// </remarks>
    void PropertyChanged(PropertyChangedArgs args);
}

/// <summary>
/// Manages the TCP connection to SimHub. The class automatically manages reconnects.
/// </summary>
/// <remarks>The plugin "SimHubPropertyServer" is required to be installed in SimHub.</remarks>
public class SimHubConnection
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly PropertyParser _propertyParser;
    private TcpClient? _tcpClient;
    private long _connected;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
    // Mapping from SimHub property name to subscribers.
    private readonly Dictionary<string, HashSet<IPropertyChangedReceiver>> _subscriptions = new();

    public SimHubConnection(PropertyParser propertyParser)
    {
        _propertyParser = propertyParser;
    }

    private bool Connected
    {
        get => Interlocked.Read(ref _connected) == 1;
        set => Interlocked.Exchange(ref _connected, Convert.ToInt64(value));
    }

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
                    foreach (var propertyName in _subscriptions.Keys)
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

    internal async Task Subscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_subscriptions.TryGetValue(propertyName, out var receivers))
            {
                // We already have a subscription for this property. So just add the new action to the existing set.
                if (receivers.Contains(propertyChangedReceiver))
                {
                    Logger.Info($"Action is already subscribed to {propertyName}, ignoring subscribe request");
                }
                else
                {
                    receivers.Add(propertyChangedReceiver);
                    Logger.Info($"Adding action to existing subscription list for {propertyName}. Has now {receivers.Count} subscribers");
                }

                return;
            }

            // We have no subscription for this property: Add it to the list.
            _subscriptions.Add(propertyName, new HashSet<IPropertyChangedReceiver> { propertyChangedReceiver });
        }
        finally
        {
            _semaphore.Release();
        }

        if (Connected)
        {
            await SendSubscribe(propertyName);
        }
        else
        {
            Logger.Info($"Queued subscribe request for {propertyName}");
        }
    }

    internal async Task Unsubscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (!_subscriptions.TryGetValue(propertyName, out var receivers))
            {
                Logger.Info($"Not subscribed to {propertyName}, ignoring unsubscribe request");
                return;
            }

            var wasRemoved = receivers.Remove(propertyChangedReceiver);
            if (!wasRemoved)
            {
                Logger.Info($"Action was not subscribed to {propertyName}, ignoring unsubscribe request");
                return;
            }
            else
            {
                Logger.Info($"Removed action from existing subscription list for {propertyName}, remaining subscribers {receivers.Count}");
            }

            // If there are still subscriptions for this property: return
            if (receivers.Count > 0) return;
            // Otherwise remove the entry completely from the list.
            _subscriptions.Remove(propertyName);
        }
        finally
        {
            _semaphore.Release();
        }

        if (Connected)
        {
            await SendUnsubscribe(propertyName);
        }
    }

    internal async Task SendTriggerInput(string inputName)
    {
        if (Connected)
        {
            await WriteToServer($"trigger-input {inputName}");
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

    private async Task ParseProperty(string line)
    {
        var parserResult = _propertyParser.ParseLine(line);
        if (parserResult == null)
        {
            Logger.Warn($"Could not parse property: {line}");
            return;
        }

        var name = parserResult.Value.name;
        var type = parserResult.Value.type;
        var value = parserResult.Value.value;

        await _semaphore.WaitAsync();
        HashSet<IPropertyChangedReceiver>? receivers;
        try
        {
            if (!_subscriptions.TryGetValue(name, out receivers))
            {
                // This should not happen.
                Logger.Warn($"Received property value from SimHub, but we have no subscribers: {name}");
                return;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        var args = new PropertyChangedArgs { PropertyName = name, PropertyType = type, PropertyValue = value };
        foreach (var propertyChangedReceiver in receivers)
        {
            propertyChangedReceiver.PropertyChanged(args);
        }
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
                Logger.Debug($"Received from server: {line}");
                if (line.StartsWith("Property "))
                {
                    await ParseProperty(line);
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