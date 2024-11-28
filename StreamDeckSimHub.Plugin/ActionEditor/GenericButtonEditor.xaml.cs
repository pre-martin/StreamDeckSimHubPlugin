// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using CommunityToolkit.Mvvm.Messaging;

namespace StreamDeckSimHub.Plugin.ActionEditor;

public partial class GenericButtonEditor : Window
{
    private readonly string _actionUuid;

    public GenericButtonEditor(string actionUuid)
    {
        _actionUuid = actionUuid;
        InitializeComponent();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new GenericButtonEditorClosedEvent(_actionUuid));
    }
}
