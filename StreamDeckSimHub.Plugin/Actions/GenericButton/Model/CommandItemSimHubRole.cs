// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class CommandItemSimHubRole : CommandItem
{
    public const string UiName = "SimHub Role";

    [ObservableProperty] private string _role = string.Empty;

    protected override string RawDisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(Role) ? Role : "SimHub Role";

    public static CommandItemSimHubRole Create()
    {
        return new CommandItemSimHubRole
        {
            Role = string.Empty
        };
    }
}