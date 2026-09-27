// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NLog;

namespace StreamDeckSimHub.Plugin.SimHub;

/// <summary>
/// Constants for all built-in plugin properties.
/// </summary>
public static class BuiltInProperties
{
    public const string Prefix = "StreamDeckSimHub.";
    public const string ConnectionConnected = $"{Prefix}Connection.Connected";
    public const string SubscriptionsCount = $"{Prefix}Subscriptions.Count";
}

/// <summary>
/// Manages built-in plugin properties that behave exactly like SimHub properties but are produced
/// locally within the plugin.
/// </summary>
public class BuiltInPropertyManager : IPropertySource
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>Stores the latest value for each known built-in property.</summary>
    private readonly Dictionary<string, PropertyChangedArgs> _values = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Stores the list of receivers per property name.</summary>
    private readonly Dictionary<string, List<IPropertyChangedReceiver>> _subscriptions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sets a built-in property value and notifies all subscribers if the value changed.
    /// </summary>
    public async Task SetProperty(string propertyName, IComparable? value)
    {
        List<IPropertyChangedReceiver> receivers;
        PropertyChangedArgs args;

        await _semaphore.WaitAsync();
        try
        {
            // Return if the value did not change.
            if (_values.TryGetValue(propertyName, out var existing) &&
                Equals(existing.PropertyValue, value))
            {
                return;
            }

            Logger.Debug($"Built-in property changed: {propertyName} = {value}");
            args = new PropertyChangedArgs(propertyName, PropertyType.Boolean, value);
            _values[propertyName] = args;

            // No subscribers? -> Return
            if (!_subscriptions.TryGetValue(propertyName, out var subs))
            {
                return;
            }

            receivers = subs.ToList();
        }
        finally
        {
            _semaphore.Release();
        }

        // Notify subscribers
        foreach (var receiver in receivers)
        {
            await receiver.PropertyChanged(args);
        }
    }

    public async Task Subscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver)
    {
        PropertyChangedArgs? currentValue;

        await _semaphore.WaitAsync();
        try
        {
            if (!_subscriptions.TryGetValue(propertyName, out var receivers))
            {
                receivers = [];
                _subscriptions[propertyName] = receivers;
            }

            if (!receivers.Contains(propertyChangedReceiver))
            {
                receivers.Add(propertyChangedReceiver);
            }

            _values.TryGetValue(propertyName, out currentValue);
        }
        finally
        {
            _semaphore.Release();
        }

        // Deliver the current value immediately, like SimHubConnection does.
        if (currentValue != null)
        {
            await propertyChangedReceiver.PropertyChanged(currentValue);
        }
    }

    public async Task Unsubscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_subscriptions.TryGetValue(propertyName, out var receivers))
            {
                receivers.Remove(propertyChangedReceiver);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public PropertyChangedArgs? GetProperty(string propertyName)
    {
        _values.TryGetValue(propertyName, out var args);
        return args;
    }
}
