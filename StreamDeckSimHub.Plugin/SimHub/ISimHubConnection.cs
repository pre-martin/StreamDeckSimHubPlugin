// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub;

public interface ISimHubConnection
{
    Task Subscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver);
    Task Unsubscribe(string propertyName, IPropertyChangedReceiver propertyChangedReceiver);
    Task SendTriggerInputPressed(string simHubControl);
    Task SendTriggerInputReleased(string simHubControl);
    Task<bool> SendControlMapperRole(string ownerId, string roleName, bool isStart);
}