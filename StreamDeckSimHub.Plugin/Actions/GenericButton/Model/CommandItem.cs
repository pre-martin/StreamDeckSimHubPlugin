// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck.Events.Received;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract class CommandItem : Item
{
    public abstract Task Accept<TPayload>(ICommandVisitor visitor, StreamDeckAction action, ActionEventArgs<TPayload> args);
}