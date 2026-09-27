// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub;

/// <summary>
/// Abstraction over a source of SimHub-style properties (either from SimHub via TCP or from internal plugin state).
/// </summary>
public interface IPropertySource
{
    Task Subscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver);
    Task Unsubscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver);
    PropertyChangedArgs? GetProperty(string propertyName);
}
