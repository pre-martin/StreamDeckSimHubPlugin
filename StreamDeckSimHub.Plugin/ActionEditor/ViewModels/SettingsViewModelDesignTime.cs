// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public class SettingsViewModelDesignTime() : SettingsViewModel(new Settings
{
    DisplayItems = [DisplayItemText.Create(), DisplayItemValue.Create()],
    Commands = new SortedDictionary<StreamDeckAction, List<CommandItem>>
    {
        [StreamDeckAction.KeyDown] = [CommandItemKeypress.Create()],
        [StreamDeckAction.KeyUp] = [CommandItemKeypress.Create(), CommandItemKeypress.Create()]
    },
});