// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Diagnostics;
using NLog;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using StreamDeckSimHub.Actions;
using StreamDeckSimHub.Tools;

namespace StreamDeckSimHub;

/// <summary>
/// Main plugin class. Manages the connection to Stream Deck and to SimHub.
/// </summary>
public class Plugin
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SemaphoreSlim _instanceLock = new(1);
    private readonly Dictionary<string, BaseAction> _actionInstances = new();
    private StreamDeckConnection? _streamDeckConnection;
    private SimHubConnection? _simHubConnection;

    public static void Run(StreamDeckOptions streamDeckOptions)
    {
        var plugin = new Plugin();
        plugin.RunInternal(streamDeckOptions);
    }

    private void RunInternal(StreamDeckOptions streamDeckOptions)
    {
        // Connect to SimHub.
        _simHubConnection = new SimHubConnection();
        _simHubConnection.Run();

        // Connect to Stream Deck
        _streamDeckConnection = new StreamDeckConnection(streamDeckOptions.Port, streamDeckOptions.PluginUuid, streamDeckOptions.RegisterEvent);
        var connectEvent = new ManualResetEvent(false);
        var disconnectEvent = new ManualResetEvent(false);
        _streamDeckConnection.OnConnected += (_, _) => connectEvent.Set();
        _streamDeckConnection.OnDisconnected += (_, _) => disconnectEvent.Set();
        _streamDeckConnection.OnWillAppear += ConnectionOnWillAppear;
        _streamDeckConnection.OnWillDisappear += ConnectionOnWillDisappear;
        _streamDeckConnection.OnDidReceiveSettings += ConnectionOnDidReceiveSettings;
        _streamDeckConnection.OnKeyDown += ConnectionOnKeyDown;
        _streamDeckConnection.OnKeyUp += ConnectionOnKeyUp;
        _streamDeckConnection.Run();

        Logger.Info("Connecting to Stream Deck");
        if (connectEvent.WaitOne(TimeSpan.FromSeconds(10)))
        {
            Logger.Info("Connected to Stream Deck");

            while (!disconnectEvent.WaitOne(TimeSpan.FromSeconds(1)))
            {
            }
        }

        Logger.Info("Disonnected from Stream Deck - exiting");
    }

    /// <summary>
    /// Called by Stream Deck, when an instance of an action is displayed on Stream Deck.
    /// </summary>
    private async void ConnectionOnWillAppear(object? sender, StreamDeckEventReceivedEventArgs<WillAppearEvent> e)
    {
        Logger.Debug(
            $"Received OnWillAppear: Action: {e.Event.Action}, Context: {e.Event.Context}, Device: {e.Event.Device}, Payload: \"{e.Event.Payload.ToStringEx()}\"");

        await _instanceLock.WaitAsync();
        try
        {
            if (_actionInstances.ContainsKey(e.Event.Context))
            {
                Logger.Info($"Action instance is already known. Ignoring 'WillAppear' for this instance.");
                // TODO Could it be part of a multi action? What is the value of e.Event.Payload.IsInMultiAction in this case?
                return;
            }

            if (e.Event.Action == "net.planetrenner.simhub.hotkey")
            {
                Logger.Info($"Creating action HotkeyAction with Context {e.Event.Context}");
                Debug.Assert(_streamDeckConnection != null, nameof(_streamDeckConnection) + " != null");
                Debug.Assert(_simHubConnection != null, nameof(_simHubConnection) + " != null");
                var actionInstance = new HotkeyAction(e.Event.Context, e.Event.Payload, _streamDeckConnection, _simHubConnection);
                _actionInstances[e.Event.Context] = actionInstance;
            }
            else
            {
                Logger.Warn($"No action named \"{e.Event.Action}\" known.");
            }
        }
        finally
        {
            _instanceLock.Release();
        }
    }

    /// <summary>
    /// Called by Stream Deck, when an instance of an action ceases to be displayed on Stream Deck.
    /// </summary>
    private async void ConnectionOnWillDisappear(object? sender, StreamDeckEventReceivedEventArgs<WillDisappearEvent> e)
    {
        Logger.Debug(
            $"Received OnWillDisappear: Action: {e.Event.Action}, Context: {e.Event.Context}, Device: {e.Event.Device}, Payload: \"{e.Event.Payload.ToStringEx()}\"");

        await _instanceLock.WaitAsync();
        try
        {
            if (_actionInstances.ContainsKey(e.Event.Context))
            {
                var action = _actionInstances[e.Event.Context];
                Logger.Info($"Removing action {e.Event.Action} with Context {e.Event.Context} because it disappeared");
                action.Destroy();
                _actionInstances.Remove(e.Event.Context);
            }
            else
            {
                Logger.Warn($"Received OnWillDisappear for unkown action instance (Context: {e.Event.Context}, Action: {e.Event.Action})");
            }
        }
        finally
        {
            _instanceLock.Release();
        }
    }


    /// <summary>
    /// Called by Stream Deck, if either the plugin called <c>getSettings</c> or when the property inspector called <c>setSettings</c>.
    /// </summary>
    private async void ConnectionOnDidReceiveSettings(object? sender, StreamDeckEventReceivedEventArgs<DidReceiveSettingsEvent> e)
    {
        Logger.Debug(
        $"Received OnDidReceiveSettings: Action: {e.Event.Action}, Context: {e.Event.Context}, Device: {e.Event.Device}, Payload: \"{e.Event.Payload.ToStringEx()}\"");

        await _instanceLock.WaitAsync();
        try
        {
            if (_actionInstances.ContainsKey(e.Event.Context))
            {
                Logger.Info($"DidReceiveSettings for action {e.Event.Action} with Context {e.Event.Context}");
                var actionInstance = _actionInstances[e.Event.Context];
                actionInstance.ReceivedSettings(e.Event.Payload);
            }
        }
        finally
        {
            _instanceLock.Release();
        }
    }

    /// <summary>
    /// Called by Stream Deck, when the user presses a key.
    /// </summary>
    private async void ConnectionOnKeyDown(object? sender, StreamDeckEventReceivedEventArgs<KeyDownEvent> e)
    {
        Logger.Debug(
            $"Received OnKeyDown: Action: {e.Event.Action}, Context: {e.Event.Context}, Device: {e.Event.Device}, Payload: \"{e.Event.Payload.ToStringEx()}\"");

        await _instanceLock.WaitAsync();
        try
        {
            if (_actionInstances.ContainsKey(e.Event.Context))
            {
                Logger.Info($"KeyDown for action {e.Event.Action} with Context {e.Event.Context}");
                var actionInstance = _actionInstances[e.Event.Context];
                actionInstance.KeyDown(e.Event.Payload);
            }
        }
        finally
        {
            _instanceLock.Release();
        }
    }

    /// <summary>
    /// Called by Stream Deck, when the user releases a key.
    /// </summary>
    private async void ConnectionOnKeyUp(object? sender, StreamDeckEventReceivedEventArgs<KeyUpEvent> e)
    {
        Logger.Debug(
            $"Received OnKeyUp: Action: {e.Event.Action}, Context: {e.Event.Context}, Device: {e.Event.Device}, Payload: \"{e.Event.Payload.ToStringEx()}\"");

        await _instanceLock.WaitAsync();
        try
        {
            if (_actionInstances.ContainsKey(e.Event.Context))
            {
                Logger.Info($"KeyUp for action {e.Event.Action} with Context {e.Event.Context}");
                var actionInstance = _actionInstances[e.Event.Context];
                actionInstance.KeyUp(e.Event.Payload);
            }

        }
        finally
        {
            _instanceLock.Release();
        }
    }
}