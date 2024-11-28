// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.Concurrent;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using NLog;

namespace StreamDeckSimHub.Plugin.ActionEditor;

/// <summary>
/// Manages the visible action editors.
/// </summary>
public class ActionEditorManager : IRecipient<GenericButtonEditorClosedEvent>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentDictionary<string, GenericButtonEditor> _actionEditors = new();

    public ActionEditorManager()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void ShowGenericButtonEditor(string actionUuid)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_actionEditors.TryGetValue(actionUuid, out var editor))
            {
                BringToFront(editor);
            }
            else
            {
                var window = new GenericButtonEditor(actionUuid)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen // show on same screen as the Stream Deck software
                };
                _actionEditors.TryAdd(actionUuid, window);
                window.Show();

                // Without these three lines, the window appears in the background
                BringToFront(window);
            }
        });
    }

    public void RemoveGenericButtonEditor(string actionUuid)
    {
        if (_actionEditors.TryRemove(actionUuid, out var editor))
        {
            Application.Current.Dispatcher.Invoke(() => editor.Close());
        }
    }

    public void Receive(GenericButtonEditorClosedEvent message)
    {
        RemoveGenericButtonEditor(message.ActionUuid);
    }

    private void BringToFront(Window window)
    {
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }
}