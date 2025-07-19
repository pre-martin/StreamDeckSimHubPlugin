// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class CommandItemSimHubControl : CommandItem, ICommandItemLong
{
    public const string UiName = "SimHub Control";

    [ObservableProperty] private string _control = string.Empty;

    [ObservableProperty] private bool _longEnabled;

    protected override string RawDisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(Control) ? Control : "SimHub Control";

    public static CommandItemSimHubControl Create()
    {
        return new CommandItemSimHubControl();
    }

    public override async Task Accept(ICommandVisitor visitor, StreamDeckAction action, IVisitorArgs? args = null)
    {
        await visitor.Visit(this, action, args);
    }
}