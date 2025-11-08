// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub;

public interface ISimHubConnection
{
    const string DefaultEmptyRole = "----";

    Task Subscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver);
    Task Unsubscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver);
    PropertyChangedArgs? GetProperty(string propertyName);
    Task SendTriggerInputPressed(string simHubControl);
    Task SendTriggerInputReleased(string simHubControl);
    Task<bool> SendControlMapperRole(string ownerId, string roleName, bool isStart);
    Task<List<string>> FetchControlMapperRoles();
}