// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class CommandItemSimHubRole : CommandItem, ICommandItemLong
{
    public const string UiName = "SimHub Role";

    [ObservableProperty] private string _role = string.Empty;

    [ObservableProperty] private bool _longEnabled;

    protected override string RawDisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(Role) ? Role : "SimHub Role";

    public static CommandItemSimHubRole Create()
    {
        return new CommandItemSimHubRole();
    }

    public override async Task Accept(ICommandVisitor visitor, StreamDeckAction action, IVisitorArgs? args = null)
    {
        await visitor.Visit(this, action, args);
    }
}