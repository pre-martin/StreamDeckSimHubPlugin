// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public interface IVisitorArgs;

public interface ICommandItemVisitor
{
    Task Visit(CommandItemKeypress commandItem, StreamDeckAction action, IVisitorArgs? args);
    Task Visit(CommandItemSimHubControl commandItem, StreamDeckAction action, IVisitorArgs? args);
    Task Visit(CommandItemSimHubRole commandItem, StreamDeckAction action, IVisitorArgs? args);
}

public interface IDisplayItemVisitor
{
    Task Visit(DisplayItemImage displayItem, IVisitorArgs? args);
    Task Visit(DisplayItemText displayItem, IVisitorArgs? args);
    Task Visit(DisplayItemValue displayItem, IVisitorArgs? args);
}