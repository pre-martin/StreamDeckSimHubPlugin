// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub.ShakeIt;

public class EffectsContainerBase
{
    public EffectsContainerBase(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; }
    public string Name { get; }

    public override string ToString()
    {
        return $"EffectsContainerBase Id='{Id}' Name='{Name}'";
    }
}