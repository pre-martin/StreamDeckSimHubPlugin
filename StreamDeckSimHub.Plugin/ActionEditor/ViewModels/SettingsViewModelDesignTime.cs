// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.Tools;
using Size = SixLabors.ImageSharp.Size;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public class SettingsViewModelDesignTime() : SettingsViewModel(Settings, null!, null!, null!, null!)
{
    private static readonly Settings Settings = new()
    {
        KeySize = new Size(StreamDeckKeyInfoBuilder.DefaultKeyInfo.KeySize)
    };

    static SettingsViewModelDesignTime()
    {
        Settings.DisplayItems.Add(DisplayItemText.Create());
        Settings.DisplayItems.Add(DisplayItemValue.Create());
        Settings.CommandItems[StreamDeckAction.KeyDown].Add(CommandItemKeypress.Create());
        Settings.CommandItems[StreamDeckAction.KeyUp].Add(CommandItemKeypress.Create());
        Settings.CommandItems[StreamDeckAction.KeyUp].Add(CommandItemSimHubControl.Create());
    }
};