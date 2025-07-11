// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SharpDeck.Events.Received;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class CommandItemSimHubControl : CommandItem
{
    public const string UiName = "SimHub Control";

    [ObservableProperty] private string _control = string.Empty;

    protected override string RawDisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(Control) ? Control : "SimHub Control";

    public static CommandItemSimHubControl Create()
    {
        return new CommandItemSimHubControl();
    }

    public override async Task Accept<TPayload>(ICommandVisitor visitor, StreamDeckAction action, ActionEventArgs<TPayload> args)
    {
        await visitor.Visit(this, action, args);
    }
}