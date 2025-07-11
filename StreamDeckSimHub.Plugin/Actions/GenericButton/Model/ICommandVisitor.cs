// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck.Events.Received;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public interface ICommandVisitor
{
    Task Visit<TPayload>(CommandItemKeypress command, StreamDeckAction action, ActionEventArgs<TPayload> args);
    Task Visit<TPayload>(CommandItemSimHubControl command, StreamDeckAction action, ActionEventArgs<TPayload> args);
    Task Visit<TPayload>(CommandItemSimHubRole command, StreamDeckAction action, ActionEventArgs<TPayload> args);
}