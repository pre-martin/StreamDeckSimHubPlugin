// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

/// <summary>
/// Does the command item support long press?
/// </summary>
public interface ICommandItemLong
{
    public bool LongEnabled { get; }
}
