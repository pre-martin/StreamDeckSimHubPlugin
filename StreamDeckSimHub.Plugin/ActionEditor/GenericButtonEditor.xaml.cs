// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

namespace StreamDeckSimHub.Plugin.ActionEditor;

public partial class GenericButtonEditor : Window
{
    private readonly string _actionUuid;

    public GenericButtonEditor(string actionUuid, Settings settings)
    {
        _actionUuid = actionUuid;
        InitializeComponent();
        DataContext = new SettingsViewModel(settings);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new GenericButtonEditorClosedEvent(_actionUuid));
    }
}
