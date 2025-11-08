// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StreamDeckSimHub.Plugin.ActionEditor.ViewModels;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.ActionEditor.Dialogs;

public partial class ImageSelector : Window
{
    private readonly ImageManager _imageManager;
    private readonly Window _parentWindow;
    private bool _windowAlreadyPositioned;
    public string RelativePath { get; private set; }

    public ImageSelector(ImageManager imageManager, string relativePath, Window parentWindow)
    {
        _imageManager = imageManager;
        _parentWindow = parentWindow;
        RelativePath = relativePath;
        DataContext = new ImageSelectorViewModel(imageManager);
        InitializeComponent();
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        if (_windowAlreadyPositioned) return;

        Top = _parentWindow.Top + 50;
        Left = _parentWindow.Left + 50;
        _windowAlreadyPositioned = true;
    }

    /// <summary>
    /// Fill directory selector and image preview.
    /// </summary>
    private void Window_Loaded(object sender, EventArgs e)
    {
        var selectedDirectory = RelativePath == string.Empty || RelativePath.IndexOf('/') < 0
            ? "/"
            : RelativePath[..RelativePath.LastIndexOf('/')];
        DirectoryComboBox.Items.Clear();
        var selectedIndex = 0;
        var directories = _imageManager.ListCustomImagesSubdirectories();
        for (var i = 0; i < directories.Length; i++)
        {
            DirectoryComboBox.Items.Add(directories[i]);
            if (directories[i] == selectedDirectory)
            {
                selectedIndex = i;
            }
        }

        DirectoryComboBox.SelectedIndex = selectedIndex;
    }

    private void DirectoryComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DirectoryComboBox.SelectedItem is string selectedDirectory)
        {
            var fileNames = _imageManager.ListCustomImages(selectedDirectory);
            ((ImageSelectorViewModel)DataContext).SetFileNames(fileNames, RelativePath);
        }
    }

    private void ImageControlOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Image { DataContext: ImageViewModel clickedImageViewModel } image)
        {
            var fileName = clickedImageViewModel.FileName;
            RelativePath = fileName;

            // Get the parent view model to update all items
            if (DataContext is ImageSelectorViewModel viewModel)
            {
                // Set IsSelected to true for the clicked image and false for all others
                foreach (var imageViewModel in viewModel.Images)
                {
                    imageViewModel.IsSelected = imageViewModel.FileName == fileName;
                }
            }
        }
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = RelativePath != string.Empty;
        Close();
    }
}