// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract partial class DisplayItem : Item
{
    [ObservableProperty] private DisplayParameters _displayParameters = new();

    public abstract Task Accept(IDisplayItemVisitor displayItemVisitor, IVisitorArgs? args = null);
}