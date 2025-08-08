// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub.ShakeIt;

public class EffectsContainerBase : IEffectElement
{
    public EffectsContainerBase(string id, string name)
    {
        Id = id;
        Name = name;
        Type = "EffectsContainerBase";
    }

    public string Id { get; }
    public string Name { get; }
    public IEffectElement? Parent { get; set; }
    public string Type { get; }

    public override string ToString()
    {
        return $"EffectsContainerBase Id='{Id}' Name='{Name}'";
    }
}