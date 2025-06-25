// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

public abstract partial class ItemViewModel(Item model) : ObservableObject
{
    /// <summary>
    /// How shall the element be displayed/called in the UI?
    /// </summary>
    public abstract string DisplayName { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _name = model.Name;

    partial void OnNameChanged(string value)
    {
        model.Name = value;
    }
}
