// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using streamdeck_client_csharp.Events;

namespace StreamDeckSimHub.Actions;

/// <summary>
/// Base class for our actions.
/// </summary>
public abstract class BaseAction
{
    public abstract void ReceivedSettings(ReceiveSettingsPayload eventPayload);
    public abstract void KeyDown(KeyPayload eventPayload);
    public abstract void KeyUp(KeyPayload eventPayload);
    public abstract void Destroy();
}