// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public class SettingsViewModelDesignTime() : SettingsViewModel(new Settings
{
    KeySize = new SixLabors.ImageSharp.Size(StreamDeckKeyInfoBuilder.DefaultKeyInfo.KeySize),
    DisplayItems = [DisplayItemText.Create(), DisplayItemValue.Create()],
    Commands = new SortedDictionary<StreamDeckAction, ObservableCollection<CommandItem>>
    {
        [StreamDeckAction.KeyDown] = [CommandItemKeypress.Create()],
        [StreamDeckAction.KeyUp] = [CommandItemKeypress.Create(), CommandItemSimHubControl.Create()]
    },
});