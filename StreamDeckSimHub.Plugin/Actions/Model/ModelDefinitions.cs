// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions.Model;

public enum StreamDeckAction
{
    KeyDown = 0,
    KeyUp = 1,
    DialLeft = 2,
    DialRight = 3,
    DialDown = 4,
    DialUp = 5,
    TouchTap = 6,
}

public enum ScaleType
{
    None = 0,
    ToSize = 1,
    ToDevice = 2,
}