// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Controls;
using StreamDeckSimHub.Plugin.ActionEditor.Dialogs;

namespace StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;

public partial class ExpressionControl : UserControl
{
    public ExpressionControl()
    {
        InitializeComponent();
    }

    private async void ShakeItBrowser_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { CommandParameter: string type }) return;

            var viewModel = (ExpressionControlViewModel)DataContext;
            var profiles = await viewModel.FetchShakeItProfilesCallback(type);
            var shakeItBrowser = new ShakeItBrowser(profiles) { Owner = Window.GetWindow(this) };
            if (shakeItBrowser.ShowDialog() == true && shakeItBrowser.SelectedItem != null)
            {
                var selectedEffect = shakeItBrowser.SelectedItem;
                var caretIndex = ExpressionTextBox.CaretIndex;
                var insertedLength = viewModel.InsertShakeIt(type, caretIndex, selectedEffect);
                ExpressionTextBox.Focus();
                ExpressionTextBox.CaretIndex = caretIndex + insertedLength;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not fetch ShakeIt profiles from SimHub. Is SimHub running?\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}