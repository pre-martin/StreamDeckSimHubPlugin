// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;
using NLog;

namespace StreamDeckSimHub.Plugin.SimHub;


/// <summary>
/// Parameters of the event, that gets fired when a new property value was received from SimHub.
/// </summary>
public class PropertyChangedArgs
{
    public PropertyChangedArgs(string propertyName, PropertyType propertyType, IComparable? propertyValue = null)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        PropertyValue = propertyValue;
    }

    public string PropertyName { get; }
    public PropertyType PropertyType { get; }
    public IComparable? PropertyValue { get; }

    public PropertyChangedArgs Clone()
    {
        return new PropertyChangedArgs(PropertyName, PropertyType, PropertyValue);
    }
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
    Task PropertyChanged(PropertyChangedArgs args);
}

/// <summary>
/// Helper class which implements a <c>IPropertyChangedReceiver</c> and delegates the event to a function.
/// </summary>
public class PropertyChangedDelegate : IPropertyChangedReceiver
{
    private readonly Func<PropertyChangedArgs, Task> _action;

    public PropertyChangedDelegate(Func<PropertyChangedArgs, Task> action)
    {
        _action = action;
    }

    public async Task PropertyChanged(PropertyChangedArgs args)
    {
        await _action(args);
    }
}

/// <summary>
/// Holds information about the current state of a property and its subscribed "changed" receivers.
/// </summary>
public class PropertyInformation
{
    public PropertyChangedArgs? CurrentPropertyChangedValue { get; set; }
    public HashSet<IPropertyChangedReceiver> PropertyChangedReceivers { get; } = new();

    public static PropertyInformation WithReceiver(IPropertyChangedReceiver receiver)
    {
        var propInfo = new PropertyInformation();
        propInfo.PropertyChangedReceivers.Add(receiver);
        return propInfo;
    }
}

/// <summary>
/// Manages the TCP connection to SimHub Property Server. The class automatically manages reconnects.
/// </summary>
/// <remarks>
/// <p>The plugin "SimHubPropertyServer" is required to be installed in SimHub.</p>
/// <p>This connection does not support multiplexing. Thus it is only used to receive "Property Changed" messages from the
/// SimHub Property Server. Other receiving communication has to be handled in a different connection.</p>
/// </remarks>
public class SimHubConnection : ISimHubConnection
{
    private readonly ConnectionSettings _connectionSettings;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly PropertyParser _propertyParser;
    private TcpClient? _tcpClient;
    private long _connected;
    private readonly SemaphoreSlim _semaphore = new(1);
    // Mapping from SimHub property name to PropertyInformation.
    private readonly Dictionary<string, PropertyInformation> _subscriptions = new();
    private readonly HttpClient _apiClient = new() { Timeout = TimeSpan.FromSeconds(2) };

    public SimHubConnection(IOptions<ConnectionSettings> connectionSettings, PropertyParser propertyParser)
    {
        _connectionSettings = connectionSettings.Value;
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
        Logger.Info($"ConnectionSettings: {_connectionSettings.Host}:{_connectionSettings.Port}");
        Connected = false;

        while (!Connected)
        {
            _tcpClient = new TcpClient();
            try
            {
                await _tcpClient.ConnectAsync(_connectionSettings.Host, _connectionSettings.Port).WaitAsync(TimeSpan.FromSeconds(4));
            }
            catch (Exception e)
            {
                Logger.Trace($"Connection could not be opened: {e.Message}");
                await Task.Delay(TimeSpan.FromSeconds(4));
                continue;
            }

            Logger.Debug("Connected to SimHub, waiting for handshake...");
            try
            {
                var line = await new LineReader(_tcpClient.GetStream()).ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(2));
                if (line != null && line.StartsWith("SimHub Property Server"))
                {
                    Logger.Info($"Established connection to {Sanitize(line)}");
                    Connected = true;
                }
            }
            catch (Exception e)
            {
                Logger.Info($"Connection failed: {e}");
            }

            if (Connected)
            {
                Logger.Info("Sending queued subscriptions and starting poll loop");
                try
                {
                    foreach (var propertyName in _subscriptions.Keys)
                    {
                        await SendSubscribe(propertyName);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Exception while subscribing queued subscriptions");
                }

                await ReadFromServer();
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(4));
            }
        }
    }

    public async Task Subscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_subscriptions.TryGetValue(propertyName, out var propInfo))
            {
                var receivers = propInfo.PropertyChangedReceivers;
                // We already have a subscription for this property. So just add the new action to the existing set.
                if (receivers.Contains(propertyChangedReceiver))
                {
                    Logger.Warn($"Action is already subscribed to {propertyName}, ignoring subscribe request");
                }
                else
                {
                    receivers.Add(propertyChangedReceiver);
                    Logger.Info($"Adding action to existing subscription list for {propertyName}. Has now {receivers.Count} subscribers");
                    if (propInfo.CurrentPropertyChangedValue != null)
                    {
                        await propertyChangedReceiver.PropertyChanged(propInfo.CurrentPropertyChangedValue);
                    }
                }

                return;
            }

            // We have no subscription for this property: Add it to the list.
            _subscriptions.Add(propertyName, PropertyInformation.WithReceiver(propertyChangedReceiver));
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

    public async Task Unsubscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (!_subscriptions.TryGetValue(propertyName, out var propInfo))
            {
                Logger.Warn($"Not subscribed to {propertyName}, ignoring unsubscribe request");
                return;
            }

            var receivers = propInfo.PropertyChangedReceivers;
            var wasRemoved = receivers.Remove(propertyChangedReceiver);
            if (!wasRemoved)
            {
                Logger.Warn($"Action was not subscribed to {propertyName}, ignoring unsubscribe request");
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

    public async Task SendTriggerInputPressed(string inputName)
    {
        await WriteToServer($"trigger-input-pressed {inputName}");
    }

    public async Task SendTriggerInputReleased(string inputName)
    {
        await WriteToServer($"trigger-input-released {inputName}");
    }

    public async Task<bool> SendControlMapperRole(string ownerId, string roleName, bool isStart)
    {
        var dict = new Dictionary<string, string>
        {
            { "ownerId", ownerId },
            { "roleName", roleName }
        };
        using var formContent = new FormUrlEncodedContent(dict);
        var action = isStart ? "StartRole" : "StopRole";
        try
        {
            using var response = await _apiClient.PostAsync($"http://localhost:8888/api/ControlMapper/{action}/", formContent);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception e)
        {
            Logger.Warn($"Could not send role to SimHub Control Mapper api: {e}");
            return false;
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
            Logger.Warn($"Could not parse property: {Sanitize(line)}");
            return;
        }

        var name = parserResult.Value.name;
        var type = parserResult.Value.type;
        var value = parserResult.Value.value;

        await _semaphore.WaitAsync();
        PropertyChangedArgs args;
        HashSet<IPropertyChangedReceiver>? receivers;
        try
        {
            if (!_subscriptions.TryGetValue(name, out var propInfo))
            {
                // This could happen, if SimHub was running, we then unsubscribe (change pages or profiles) while SimHub is not running.
                Logger.Warn($"Received property value from SimHub, but we have no subscribers: {name}");
                return;
            }

            // Save current property data for later usage.
            propInfo.CurrentPropertyChangedValue = new PropertyChangedArgs(name, type, value);

            // References for usage outside of semaphore. Clone the list of receivers to avoid concurrent modification.
            args = propInfo.CurrentPropertyChangedValue.Clone();
            receivers = new HashSet<IPropertyChangedReceiver>(propInfo.PropertyChangedReceivers);
        }
        finally
        {
            _semaphore.Release();
        }

        Logger.Debug($"Dispatching PropertyChanged to {receivers.Count} receivers");
        foreach (var propertyChangedReceiver in receivers)
        {
            await propertyChangedReceiver.PropertyChanged(args);
        }
        Logger.Debug("Dispatched PropertyChanged to receivers");
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
                Logger.Debug($"Received from server: {Sanitize(line)}");
                if (line.StartsWith("Property "))
                {
                    try
                    {
                        await ParseProperty(line);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"Unhandled exception while processing data from server. Received line was: \"{Sanitize(line)}\"");
                    }
                }
            }

            // "line == null": End of stream. Fall through to "CloseAndReconnect".
            Logger.Info("Server closed connection");
        }
        catch (IOException ioe)
        {
            // IOException: Fall through to "CloseAndReconnect".
            Logger.Warn($"Received IOException while waiting for data: {ioe}");
        }

        await CloseAndReconnect();
    }

    private async Task WriteToServer(string line)
    {
        Logger.Debug($"WriteToServer: {line}");
        if (!Connected || _tcpClient == null || !_tcpClient.Connected)
        {
            Logger.Warn(
                $"Cannot send to server (connected: {Connected}, tcpClient: {_tcpClient != null}, tcpClient.Connected: {_tcpClient?.Connected})");
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

    private string? Sanitize(string? s)
    {
        return s?.Replace(Environment.NewLine, "");
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