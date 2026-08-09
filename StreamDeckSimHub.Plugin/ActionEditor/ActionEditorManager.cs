// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.Concurrent;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.ActionEditor;

/// <summary>
/// Manages the visible action editors.
/// </summary>
public class ActionEditorManager : IRecipient<GenericButtonEditorClosedEvent>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ConcurrentDictionary<string, GenericButtonEditor> _actionEditors = new();
    private readonly SettingsConverter _settingsConverter;
    private readonly ImageManager _imageManager;
    private readonly ISimHubConnection _simHubConnection;
    private readonly ShakeItStructureFetcher _shakeItStructureFetcher;

    public ActionEditorManager(
        SettingsConverter settingsConverter, ImageManager imageManager, ISimHubConnection simHubConnection,
        ShakeItStructureFetcher shakeItStructureFetcher)
    {
        _settingsConverter = settingsConverter;
        _imageManager = imageManager;
        _simHubConnection = simHubConnection;
        _shakeItStructureFetcher = shakeItStructureFetcher;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public Window ShowGenericButtonEditor(string actionUuid, Settings settings)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            if (_actionEditors.TryGetValue(actionUuid, out var editor))
            {
                Logger.Debug("Showing existing editor for action {ActionUuid}", actionUuid);
                BringToFront(editor);
                return editor;
            }

            Logger.Debug("Showing new editor for action {ActionUuid}", actionUuid);
            var newEditor = new GenericButtonEditor(actionUuid)
            {
                WindowStartupLocation =
                    WindowStartupLocation.CenterScreen, // show on the same screen as the Stream Deck software
            };
            var viewModel = new SettingsViewModel(settings, _settingsConverter, _imageManager, _simHubConnection,
                _shakeItStructureFetcher, newEditor);
            newEditor.SetViewModel(viewModel);
            _actionEditors.TryAdd(actionUuid, newEditor);
            newEditor.Show();

            // Without these three lines, the window appears in the background
            BringToFront(newEditor);
            return newEditor;
        });
    }

    public void RemoveGenericButtonEditor(string actionUuid)
    {
        if (_actionEditors.TryRemove(actionUuid, out var editor))
        {
            Logger.Debug("Removed existing editor for action {ActionUuid}", actionUuid);
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