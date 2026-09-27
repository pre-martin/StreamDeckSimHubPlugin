// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub;

/// <summary>
/// Routes property subscriptions either to <see cref="BuiltInPropertyManager"/> (for properties with the
/// <c>StreamDeckSimHub.</c> prefix) or to <see cref="ISimHubConnection"/> (for all other properties).
/// Consumers only need to know this single <see cref="IPropertySource"/>.
/// </summary>
public class PropertyRouter(ISimHubConnection simHubConnection, BuiltInPropertyManager builtInPropertyManager)
    : IPropertySource
{
    private bool IsBuiltIn(string propertyName) =>
        propertyName.StartsWith(BuiltInProperties.Prefix, StringComparison.OrdinalIgnoreCase);

    public Task Subscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver)
    {
        return IsBuiltIn(propertyName)
            ? builtInPropertyManager.Subscribe(propertyName, propertyChangedReceiver)
            : simHubConnection.Subscribe(propertyName, propertyChangedReceiver);
    }

    public Task Unsubscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver)
    {
        return IsBuiltIn(propertyName)
            ? builtInPropertyManager.Unsubscribe(propertyName, propertyChangedReceiver)
            : simHubConnection.Unsubscribe(propertyName, propertyChangedReceiver);
    }

    public PropertyChangedArgs? GetProperty(string propertyName)
    {
        return IsBuiltIn(propertyName)
            ? builtInPropertyManager.GetProperty(propertyName)
            : simHubConnection.GetProperty(propertyName);
    }
}
