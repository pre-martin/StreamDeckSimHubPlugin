// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.ActionEditor.Dialogs;

public partial class ImageSelectorViewModel(ImageManager imageManager) : ObservableObject
{
    private CancellationTokenSource _cts = new();
    [ObservableProperty] private ObservableCollection<ImageViewModel> _images = new();

    public void SetFileNames(string[] fileNames, string selectedImage)
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();

        Images.Clear();
        foreach (var fileName in fileNames)
        {
            var imageViewModel = new ImageViewModel { FileName = fileName, IsSelected = fileName == selectedImage };
            Images.Add(imageViewModel);
        }

        // Load the images in the background
        _ = Task.Run(() => LoadImages(_cts.Token));
    }

    private Task LoadImages(CancellationToken ct)
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = ct };
        try
        {
            Parallel.ForEach(Images, options, (image) =>
            {
                var imageData = imageManager.GetCustomImage(image.FileName, StreamDeckKeyInfoBuilder.DefaultKeyInfo);
                var bitmapImage = imageManager.ImageUtils.FromImage(imageData);

                Application.Current.Dispatcher.Invoke(() => image.ImageSource = bitmapImage);
            });
        }
        catch (OperationCanceledException)
        {
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public partial class ImageViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName = string.Empty;
    [ObservableProperty] private ImageSource? _imageSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BorderBrush))]
    private bool _isSelected;

    public Brush BorderBrush => IsSelected ? SystemColors.HighlightBrush : Brushes.Gray;
}