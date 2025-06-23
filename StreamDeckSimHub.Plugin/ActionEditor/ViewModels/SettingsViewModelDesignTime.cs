// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public class SettingsViewModelDesignTime() : SettingsViewModel(Settings)
{
    private static readonly Settings Settings = new()
    {
        KeySize = new Size(StreamDeckKeyInfoBuilder.DefaultKeyInfo.KeySize)
    };

    static SettingsViewModelDesignTime()
    {
        Settings.AddDisplayItem(DisplayItemText.Create());
        Settings.AddDisplayItem(DisplayItemValue.Create());
        Settings.AddCommandItem(StreamDeckAction.KeyDown, CommandItemKeypress.Create());
        Settings.AddCommandItem(StreamDeckAction.KeyUp, CommandItemKeypress.Create());
        Settings.AddCommandItem(StreamDeckAction.KeyUp, CommandItemSimHubControl.Create());
    }
};