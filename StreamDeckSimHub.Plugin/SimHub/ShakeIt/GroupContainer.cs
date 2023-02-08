// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.SimHub.ShakeIt;

public class GroupContainer : EffectsContainerBase
{
    public GroupContainer(string id, string name) : base(id, name)
    {
    }

    public IList<EffectsContainerBase> EffectsContainers { get; } = new List<EffectsContainerBase>();

    public override string ToString()
    {
        return $"GroupContainer Id='{Id}' Name='{Name}' ({EffectsContainers.Count} children)";
    }

}
