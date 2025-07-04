// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class CommandItemSimHubControl : CommandItem
{
    public const string UiName = "SimHub Control";

    [ObservableProperty] private string _control = string.Empty;

    protected override string RawDisplayName => "SimHub Control";

    public static CommandItemSimHubControl Create()
    {
        return new CommandItemSimHubControl
        {
            Control = string.Empty
        };
    }
}