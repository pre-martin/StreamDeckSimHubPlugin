// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.Fonts;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.Tools;
using Color = SixLabors.ImageSharp.Color;
using Point = SixLabors.ImageSharp.Point;
using Size = SixLabors.ImageSharp.Size;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

/// <summary>
/// Base ViewModel for all DisplayItems
/// </summary>
public abstract partial class DisplayItemViewModel(DisplayItem model, Window parentWindow) : ItemViewModel(model, parentWindow)
{
    [ObservableProperty] private float _transparency = model.DisplayParameters.Transparency;
    [ObservableProperty] private int _posX = model.DisplayParameters.Position.X;
    [ObservableProperty] private int _posY = model.DisplayParameters.Position.Y;
    [ObservableProperty] private int? _sizeWidth = model.DisplayParameters.Size?.Width;
    [ObservableProperty] private int? _sizeHeight = model.DisplayParameters.Size?.Height;
    [ObservableProperty] private int _rotation = model.DisplayParameters.Rotation;

    partial void OnTransparencyChanged(float value)
    {
        model.DisplayParameters.Transparency = value;
    }

    partial void OnPosXChanged(int value)
    {
        model.DisplayParameters.Position = new Point(value, PosY);
    }

    partial void OnPosYChanged(int value)
    {
        model.DisplayParameters.Position = new Point(PosX, value);
    }

    partial void OnSizeWidthChanged(int? value)
    {
        if (value.HasValue)
        {
            SizeHeight ??= value.Value; // if width is set, ensure that height is also set.
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
            SizeWidth ??= value.Value; // if height is set, ensure that width is also set.
            model.DisplayParameters.Size = new Size(SizeWidth.Value, value.Value);
        }
        else
        {
            SizeWidth = null;
            model.DisplayParameters.Size = null;
        }
    }

    partial void OnRotationChanged(int value)
    {
        model.DisplayParameters.Rotation = value;
    }
}

/// <summary>
/// ViewModel for DisplayItemImage
/// </summary>
public partial class DisplayItemImageViewModel(DisplayItemImage model, ImageManager imageManager, Window parentWindow)
    : DisplayItemViewModel(model, parentWindow)
{
    public override ImageSource? Icon => ParentWindow.FindResource("DiInsertPhotoOutlinedGray") as ImageSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))] // see DisplayItemImage.DisplayName which uses RelativePath
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
    : DisplayItemViewModel(model, parentWindow), IFontSelectable, IColorSelectable
{
    public override ImageSource? Icon => ParentWindow.FindResource("DiTextFieldsGray") as ImageSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))] // see DisplayItemText.DisplayName which uses Text
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

    public string FontAsString => $"{model.Font.Family.Name}, {model.Font.Size}, {model.Font.FontStyle().ToString()}";

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

/// <summary>
/// ViewModel for DisplayItemValue
/// </summary>
public partial class DisplayItemValueViewModel : DisplayItemViewModel, IFontSelectable, IColorSelectable
{
    private readonly NCalcHandler _ncalcHandler = new();
    private readonly DisplayItemValue _model;

    public DisplayItemValueViewModel(DisplayItemValue model, Window parentWindow) : base(model, parentWindow)
    {
        _model = model;
        _propertyString = model.NCalcPropertyHolder.ExpressionString;
        _displayFormat = model.DisplayFormat;
        _font = model.Font;
        _imageSharpColor = model.Color;

        // Populate the error message if the condition string is invalid, so that we have it right when the view is displayed.
        try
        {
            _ncalcHandler.Parse(_propertyString, out _);
        }
        catch (Exception e)
        {
            PropertyErrorMessage = _ncalcHandler.BuildNCalcErrorMessage(e);
        }
    }

    public override ImageSource? Icon => ParentWindow.FindResource("DiAttachMoneyGray") as ImageSource;

    [ObservableProperty]
    private string _propertyString;

    partial void OnPropertyStringChanged(string value)
    {
        PropertyErrorMessage = _ncalcHandler.UpdateNCalcHolder(value, _model.NCalcPropertyHolder);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PropertyErrorVisibility))]
    private string? _propertyErrorMessage;

    public Visibility PropertyErrorVisibility => PropertyErrorMessage is not null ? Visibility.Visible : Visibility.Collapsed;

    [ObservableProperty] private string _displayFormat;

    partial void OnDisplayFormatChanged(string value)
    {
        _model.DisplayFormat = value;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FontAsString))]
    private Font _font;

    partial void OnFontChanged(Font value)
    {
        _model.Font = value;
    }

    public string FontAsString => $"{_model.Font.Family.Name}, {_model.Font.Size}, {_model.Font.FontStyle().ToString()}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorHex))]
    [NotifyPropertyChangedFor(nameof(ColorAsWpf))]
    private Color _imageSharpColor;

    public string ColorHex => $"#{_model.Color.ToHexWithoutAlpha()}";

    public System.Windows.Media.Color ColorAsWpf => ImageSharpColor.ToWpfColor();

    partial void OnImageSharpColorChanged(Color value)
    {
        _model.Color = value;
    }
}