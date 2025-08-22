// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using StreamDeckSimHub.Plugin.SimHub.ShakeIt;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public interface IViewModel
{
    Window ParentWindow { get; }

    /// <summary>
    /// Fetches the Control Mapper Roles from SimHub and updates all relevant child view models.
    /// </summary>
    Task FetchControlMapperRoles();

    /// <summary>
    /// Fetches the ShakeIt Bass profiles from SimHub and returns them to the caller.
    /// </summary>
    Task<IList<Profile>> FetchShakeItBassProfiles();

    /// <summary>
    /// Fetches the ShakeIt Motors profiles from SimHub and returns them to the caller.
    /// </summary>
    Task<IList<Profile>> FetchShakeItMotorsProfiles();
}