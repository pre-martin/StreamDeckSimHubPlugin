// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract class CommandItem : Item
{
    public abstract Task Accept(ICommandItemVisitor commandItemVisitor, StreamDeckAction action, IVisitorArgs? args = null);
}