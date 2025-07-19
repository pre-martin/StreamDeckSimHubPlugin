// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public interface IVisitorArgs;

public interface ICommandVisitor
{
    Task Visit(CommandItemKeypress command, StreamDeckAction action, IVisitorArgs? args);
    Task Visit(CommandItemSimHubControl command, StreamDeckAction action, IVisitorArgs? args);
    Task Visit(CommandItemSimHubRole command, StreamDeckAction action, IVisitorArgs? args);
}