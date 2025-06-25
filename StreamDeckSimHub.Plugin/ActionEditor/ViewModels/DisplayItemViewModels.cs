// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

/// <summary>
/// Base ViewModel for all DisplayItems
/// </summary>
public abstract class DisplayItemViewModel(DisplayItem model, Window parentWindow) : ItemViewModel(model)
{
    protected readonly Window ParentWindow = parentWindow;
}

/// <summary>
/// ViewModel for DisplayItemImage
/// </summary>
public partial class DisplayItemImageViewModel(DisplayItemImage model, ImageManager imageManager, Window parentWindow)
    : DisplayItemViewModel(model, parentWindow)
{
    public override string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Image" : Name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImageSource))]
    [NotifyPropertyChangedFor(nameof(Resolution))]
    private string _relativePath = model.RelativePath;

    partial void OnRelativePathChanged(string value)
    {
        model.RelativePath = value;
    }

    public BitmapImage ImageSource => imageManager.ImageUtils.FromImage(model.Image);

    public string Resolution => Path.GetExtension(RelativePath).Equals(".svg", StringComparison.InvariantCultureIgnoreCase)
        ? string.Empty
        : $"{model.Image.Width} x {model.Image.Height}";

    [RelayCommand]
    private void SelectImage()
    {
        var imageSelector = new ImageSelector(imageManager, RelativePath, ParentWindow);
        if (imageSelector.ShowDialog() == true)
        {
            RelativePath = imageSelector.RelativePath;
        }
    }
}

/// <summary>
/// ViewModel for DisplayItemText
/// </summary>
public class DisplayItemTextViewModel(DisplayItemText model, Window parentWindow) : DisplayItemViewModel(model, parentWindow)
{
    public override string DisplayName => "Text";
    // Add properties specific to DisplayItemText here
}

/// <summary>
/// ViewModel for DisplayItemValue
/// </summary>
public class DisplayItemValueViewModel(DisplayItemValue model, Window parentWindow) : DisplayItemViewModel(model, parentWindow)
{
    public override string DisplayName => "Value";
    // Add properties specific to DisplayItemValue here
}