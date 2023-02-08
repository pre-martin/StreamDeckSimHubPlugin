// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub.ShakeIt;

public class Profile
{
    public Profile(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; }
    public string Name { get; }
    public IList<EffectsContainerBase> EffectsContainer { get; } = new List<EffectsContainerBase>();

    public override string ToString()
    {
        return $"Profile Id='{Id}' Name='{Name}' ({EffectsContainer.Count} children)";
    }
}