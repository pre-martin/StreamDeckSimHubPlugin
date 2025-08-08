// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Newtonsoft.Json;

namespace StreamDeckSimHub.Plugin.SimHub.ShakeIt;

public interface IEffectElement
{
    public string Id { get; }
    public string Name { get; }
    [JsonIgnore] public IEffectElement? Parent { get; }
}