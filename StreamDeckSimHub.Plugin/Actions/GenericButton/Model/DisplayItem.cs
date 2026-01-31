// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public abstract partial class DisplayItem : Item
{
    [ObservableProperty] private DisplayParameters _displayParameters = new();

    public ObservableCollection<Modifier> Modifiers { get; set; } = [];

    protected DisplayItem()
    {
        Modifiers.CollectionChanged += (_, args) =>
        {
            if (args is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            {
                // Register on PropertyChanged of child Modifiers, so that we can propagate these changes
                foreach (var item in args.NewItems)
                {
                    if (item is Modifier modifier)
                    {
                        modifier.PropertyChanged += (sender, a) => OnPropertyChanged(a);
                    }
                }
            }

            OnPropertyChanged(nameof(Modifiers));
        };
    }

    public abstract Task Accept(IDisplayItemVisitor displayItemVisitor, IVisitorArgs? args = null);
}