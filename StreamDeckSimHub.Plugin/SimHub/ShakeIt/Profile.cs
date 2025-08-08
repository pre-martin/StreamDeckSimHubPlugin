// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub.ShakeIt;

public class Profile : IEffectElement
{
    public Profile(string id, string name)
    {
        Id = id;
        Name = name;
        Type = "Profile";
    }

    public string Id { get; }
    public string Name { get; }
    public IEffectElement? Parent { get; } = null;
    public string Type { get; }
    public IList<EffectsContainerBase> EffectsContainers { get; } = new List<EffectsContainerBase>();

    public override string ToString()
    {
        return $"Profile Id='{Id}' Name='{Name}' ({EffectsContainers.Count} children)";
    }
}