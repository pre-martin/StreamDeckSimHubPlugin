// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.Fonts;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Tools;
using Color = SixLabors.ImageSharp.Color;
using Point = SixLabors.ImageSharp.Point;
using Size = SixLabors.ImageSharp.Size;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

/// <summary>
/// Base ViewModel for all DisplayItems
/// </summary>
public abstract partial class DisplayItemViewModel(DisplayItem model, Window parentWindow) : ItemViewModel(model)
{
    protected readonly Window ParentWindow = parentWindow;

    public abstract ImageSource? Icon { get; }

    [ObservableProperty] private int _posX = model.DisplayParameters.Position.X;
    [ObservableProperty] private int _posY = model.DisplayParameters.Position.Y;
    [ObservableProperty] private float _transparency = model.DisplayParameters.Transparency;
    [ObservableProperty] private int _rotation = model.DisplayParameters.Rotation;
    [ObservableProperty] private int? _sizeWidth = model.DisplayParameters.Size?.Width;
    [ObservableProperty] private int? _sizeHeight = model.DisplayParameters.Size?.Height;

    partial void OnPosXChanged(int value)
    {
        model.DisplayParameters.Position = new Point(value, PosY);
    }

    partial void OnPosYChanged(int value)
    {
        model.DisplayParameters.Position = new Point(PosX, value);
    }

    partial void OnTransparencyChanged(float value)
    {
        model.DisplayParameters.Transparency = value;
    }

    partial void OnRotationChanged(int value)
    {
        model.DisplayParameters.Rotation = value;
    }

    partial void OnSizeWidthChanged(int? value)
    {
        if (value.HasValue)
        {
            SizeHeight ??= 0;
            model.DisplayParameters.Size = new Size(value.Value, SizeHeight.Value);
        }
        else
        {
            SizeHeight = null;
            model.DisplayParameters.Size = null;
        }
    }

    partial void OnSizeHeightChanged(int? value)
    {
        if (value.HasValue)
        {
            SizeWidth ??= 0;
            model.DisplayParameters.Size = new Size(SizeWidth.Value, value.Value);
        }
        else
        {
            SizeWidth = null;
            model.DisplayParameters.Size = null;
        }
    }
}

/// <summary>
/// ViewModel for DisplayItemImage
/// </summary>
public partial class DisplayItemImageViewModel(DisplayItemImage model, ImageManager imageManager, Window parentWindow)
    : DisplayItemViewModel(model, parentWindow)
{
    public override ImageSource? Icon => ParentWindow.FindResource("DiInsertPhotoOutlinedGray") as ImageSource;

    public override string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(RelativePath) ? Path.GetFileNameWithoutExtension(RelativePath) : "Image";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
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
public partial class DisplayItemTextViewModel(DisplayItemText model, Window parentWindow)
    : DisplayItemViewModel(model, parentWindow)
{
    public override ImageSource? Icon => ParentWindow.FindResource("DiTextFieldsGray") as ImageSource;

    public override string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(Text) ? LineBreakRegex().Replace(Text, " ") : "Text";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _text = model.Text;

    partial void OnTextChanged(string value)
    {
        model.Text = value;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontAsString))]
    private Font _font = model.Font;

    partial void OnFontChanged(Font value)
    {
        model.Font = value;
    }

    public string FontAsString => $"{model.Font.Name}, {model.Font.Size}, {model.Font.FontStyle().ToString()}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorHex))]
    [NotifyPropertyChangedFor(nameof(ColorAsWpf))]
    private Color _imageSharpColor = model.Color;

    public string ColorHex => $"#{model.Color.ToHexWithoutAlpha()}";

    public System.Windows.Media.Color ColorAsWpf => ImageSharpColor.ToWpfColor();

    partial void OnImageSharpColorChanged(Color value)
    {
        model.Color = value;
    }

    [GeneratedRegex(@"\r\n?|\n")]
    private static partial Regex LineBreakRegex();
}

/// <summary>
/// ViewModel for DisplayItemValue
/// </summary>
public partial class DisplayItemValueViewModel(DisplayItemValue model, Window parentWindow)
    : DisplayItemViewModel(model, parentWindow)
{
    public override ImageSource? Icon => ParentWindow.FindResource("DiAttachMoneyGray") as ImageSource;

    public override string DisplayName => !string.IsNullOrWhiteSpace(Name) ? Name :
        !string.IsNullOrWhiteSpace(PropertyName) ? PropertyName : "Value";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string _propertyName = model.Property;

    partial void OnPropertyNameChanged(string value)
    {
        model.Property = value;
    }

    [ObservableProperty] private string _displayFormat = model.DisplayFormat;

    partial void OnDisplayFormatChanged(string value)
    {
        model.DisplayFormat = value;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontAsString))]
    private Font _font = model.Font;

    partial void OnFontChanged(Font value)
    {
        model.Font = value;
    }

    public string FontAsString => $"{model.Font.Name}, {model.Font.Size}, {model.Font.FontStyle().ToString()}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorHex))]
    [NotifyPropertyChangedFor(nameof(ColorAsWpf))]
    private Color _imageSharpColor = model.Color;

    public string ColorHex => $"#{model.Color.ToHexWithoutAlpha()}";

    public System.Windows.Media.Color ColorAsWpf => ImageSharpColor.ToWpfColor();

    partial void OnImageSharpColorChanged(Color value)
    {
        model.Color = value;
    }
}