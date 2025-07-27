// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using StreamDeckSimHub.Plugin.SimHub.ShakeIt;

namespace StreamDeckSimHub.Plugin.ActionEditor.Dialogs;

public partial class ShakeItBrowser : Window
{
    public EffectsContainerBase? SelectedItem { get; set; }

    public ShakeItBrowser(IList<Profile> shakeItProfiles)
    {
        InitializeComponent();
        DataContext = new ShakeItBrowserViewModel(shakeItProfiles);
        SelectButton.IsEnabled = false;
    }

    private void SelectButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = SelectedItem != null;
        Close();
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is EffectsContainerBase container)
        {
            SelectedItem = container;
            SelectButton.IsEnabled = true;
        }
        else
        {
            SelectedItem = null; // everything else, especially Profile!
            SelectButton.IsEnabled = false;
        }
    }
}

public class ShakeItBrowserViewModel(IList<Profile> shakeItProfiles)
{
    public IList<Profile> ShakeItProfiles { get; } = shakeItProfiles;
}