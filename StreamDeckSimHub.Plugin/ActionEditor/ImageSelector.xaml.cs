// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StreamDeckSimHub.Plugin.Tools;
using SystemColors = System.Windows.SystemColors;

namespace StreamDeckSimHub.Plugin.ActionEditor;

public partial class ImageSelector : Window
{
    private readonly ImageManager _imageManager;
    private readonly Window _parentWindow;
    private bool _windowAlreadyPositioned;
    private Image? _previouslySelectedImage;
    public string RelativePath { get; set; }

    public ImageSelector(ImageManager imageManager, string relativePath, Window parentWindow)
    {
        _imageManager = imageManager;
        _parentWindow = parentWindow;
        RelativePath = relativePath;
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
            ImagesPanel.Children.Clear();

            var fileNames = _imageManager.ListCustomImages(selectedDirectory);
            foreach (var fileName in fileNames)
            {
                var image = _imageManager.GetCustomImage(fileName, StreamDeckKeyInfoBuilder.DefaultKeyInfo);
                var bitmapImage = _imageManager.ImageUtils.FromImage(image);

                var imageControl = new Image
                {
                    Source = bitmapImage,
                    Width = 72,
                    Margin = new Thickness(3),
                    ToolTip = fileName
                };
                imageControl.MouseLeftButtonDown += ImageControlOnMouseLeftButtonDown;

                var isSelectedFile = fileName == RelativePath;
                if (isSelectedFile) _previouslySelectedImage = imageControl;

                var border = new Border
                {
                    Child = imageControl,
                    BorderThickness = new Thickness(3),
                    BorderBrush = isSelectedFile ? SystemColors.HighlightBrush : Brushes.Gray,
                    Background = Brushes.Black // emulate Stream Deck key background color
                };

                ImagesPanel.Children.Add(border);
            }
        }
    }

    private void ImageControlOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Image image)
        {
            if (_previouslySelectedImage is { Parent: Border previousBorder })
            {
                previousBorder.BorderBrush = Brushes.Gray;
            }

            RelativePath = image.ToolTip as string ?? string.Empty;
            if (image.Parent is Border border) border.BorderBrush = SystemColors.HighlightBrush;

            _previouslySelectedImage = image;
        }
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = RelativePath != string.Empty ? true : false;
        Close();
    }
}