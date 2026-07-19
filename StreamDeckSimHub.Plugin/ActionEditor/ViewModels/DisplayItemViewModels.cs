// Copyright (C) 2026 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SixLabors.Fonts;
using StreamDeckSimHub.Plugin.ActionEditor.Dialogs;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.ActionEditor.Views.Controls;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model.Modifiers;
using StreamDeckSimHub.Plugin.Tools;
using Color = SixLabors.ImageSharp.Color;
using Point = SixLabors.ImageSharp.Point;
using Size = SixLabors.ImageSharp.Size;

namespace StreamDeckSimHub.Plugin.ActionEditor.ViewModels;

/// <summary>
/// Base ViewModel for all DisplayItems
/// </summary>
#pragma warning disable CS9113 // Parameter is unread.
public abstract partial class DisplayItemViewModel(DisplayItem model, IViewModel parentViewModel, byte? _)
#pragma warning restore CS9113 // Parameter is unread.
    : ItemViewModel(model, parentViewModel), IDataErrorInfo
{
    protected DisplayItemViewModel(DisplayItem model, IViewModel parentViewModel) : this(model, parentViewModel, null)
    {
        Modifiers = new ObservableCollection<ModifierViewModel>(model.Modifiers.Select(ModifierToViewModel));
        Modifiers.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasModifiers));

        if (model is IAcceptsModifierBlink) AvailableModifiers.Add(ModifierBlink.UiName);
        if (model is IAcceptsModifierColor) AvailableModifiers.Add(ModifierColor.UiName);
        CanAddModifier = AvailableModifiers.Count > 0;
    }

    [ObservableProperty] private int _selectedTabIndex;

    #region Element Data

    [ObservableProperty] private float _transparency = model.DisplayParameters.Transparency;

    [ObservableProperty]
    private string _transparencyText = model.DisplayParameters.Transparency.ToString(CultureInfo.InvariantCulture);

    [ObservableProperty] private int _posX = model.DisplayParameters.Position.X;
    [ObservableProperty] private int _posY = model.DisplayParameters.Position.Y;
    [ObservableProperty] private int? _sizeWidth = model.DisplayParameters.Size?.Width;
    [ObservableProperty] private int? _sizeHeight = model.DisplayParameters.Size?.Height;
    [ObservableProperty] private int _rotation = model.DisplayParameters.Rotation;

    partial void OnTransparencyChanged(float value)
    {
        model.DisplayParameters.Transparency = value;
    }

    partial void OnTransparencyTextChanged(string value)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            Transparency = parsed;
        }
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

        OnDisplaySizeChanged();
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

        OnDisplaySizeChanged();
    }

    partial void OnRotationChanged(int value)
    {
        model.DisplayParameters.Rotation = value;
    }

    #endregion

    #region Modifiers

    public ObservableCollection<ModifierViewModel> Modifiers { get; } = [];

    public bool HasModifiers => Modifiers.Count > 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsModifierSelected))]
    private ModifierViewModel? _selectedModifier;

    public bool IsModifierSelected => SelectedModifier != null;

    public ObservableCollection<string> AvailableModifiers { get; } = [];

    [ObservableProperty] private bool _canAddModifier;

    [RelayCommand]
    private void AddModifier(string type)
    {
        switch (type)
        {
            case ModifierBlink.UiName:
                AddModifier(ModifierBlink.Create());
                break;
            case ModifierColor.UiName:
                AddModifier(ModifierColor.Create());
                break;
        }
    }

    private void AddModifier(Modifier modifier)
    {
        model.Modifiers.Add(modifier);
        var vm = ModifierToViewModel(modifier);
        Modifiers.Add(vm);
        SelectedModifier = vm;
    }

    private ModifierViewModel ModifierToViewModel(Modifier modifier)
    {
        return modifier switch
        {
            ModifierBlink modifierBlink => new ModifierBlinkViewModel(modifierBlink, ParentViewModel),
            ModifierColor colorModifier => new ModifierColorViewModel(colorModifier, ParentViewModel),
            _ => throw new InvalidOperationException($"Unknown Modifier type: {modifier.GetType().FullName}")
        };
    }

    public void RemoveModifier(ModifierViewModel item)
    {
        // Remove from the underlying model
        var modifier = item.GetModel();
        model.Modifiers.Remove(modifier);

        // Remove from the ViewModel collection
        Modifiers.Remove(item);

        // Clear selection if this was the selected item
        if (SelectedModifier == item)
        {
            SelectedModifier = null;
        }

    }

    #endregion

    #region DragDrop

    /// <summary>
    /// Updates the underlying model when Modifiers are reordered
    /// </summary>
    public void UpdateModifiersOrder()
    {
        // Update the underlying model's Modifiers list to match the order in the ViewModel
        // We'll create a new list with the same items but in the new order
        var newList = Modifiers.Select(modifierVm => modifierVm.GetModel()).ToList();

        // Clear and repopulate the original list to maintain the reference
        model.Modifiers.Clear();
        foreach (var item in newList)
        {
            model.Modifiers.Add(item);
        }
    }

    #endregion

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            if (columnName == nameof(TransparencyText))
            {
                if (string.IsNullOrWhiteSpace(TransparencyText)) return "Transparency value is required.";

                if (!float.TryParse(TransparencyText, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                    || parsed < 0f || parsed > 1f)
                {
                    return "Invalid transparency value. Please enter a valid number.";
                }
            }

            return ValidateColumn(columnName);
        }
    }

    /// <summary>
    /// Hook for subclasses if they want to participate in data validation.
    /// </summary>
    protected virtual string ValidateColumn(string columnName)
    {
        return string.Empty;
    }

    /// <summary>
    /// Hook for subclasses if they depend on display size (SizeWidth/SizeHeight).
    /// </summary>
    protected virtual void OnDisplaySizeChanged()
    {
    }
}

/// <summary>
/// ViewModel for DisplayItemBox
/// </summary>
public partial class DisplayItemBoxViewModel(DisplayItemBox model, IViewModel parentViewModel)
    : DisplayItemViewModel(model, parentViewModel), IColorSelectable
{
    public override ImageSource? Icon => ParentViewModel.ParentWindow.FindResource(DisplayItemBox.UiIcon) as ImageSource;

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

    [ObservableProperty] private int _cornerRadius = model.CornerRadius;
    [ObservableProperty] private string _cornerRadiusText = model.CornerRadius.ToString(CultureInfo.InvariantCulture);
    [ObservableProperty] private bool _isFilled = model.IsFilled;
    [ObservableProperty] private int _borderWidth = model.BorderWidth;
    [ObservableProperty] private string _borderWidthText = model.BorderWidth.ToString(CultureInfo.InvariantCulture);

    public string CornerRadiusToolTip => BuildCornerRadiusToolTip();
    public string BorderWidthToolTip => "Only used when Fill is disabled.";

    partial void OnCornerRadiusChanged(int value)
    {
        if (value < 0)
        {
            CornerRadius = 0;
            return;
        }

        model.CornerRadius = value;
        var newTextValue = value.ToString(CultureInfo.InvariantCulture);
        if (!string.Equals(CornerRadiusText, newTextValue, StringComparison.Ordinal))
        {
            CornerRadiusText = newTextValue;
        }
    }

    partial void OnCornerRadiusTextChanged(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
        {
            CornerRadius = parsedValue;
        }
    }

    partial void OnIsFilledChanged(bool value)
    {
        model.IsFilled = value;
    }

    partial void OnBorderWidthChanged(int value)
    {
        if (value < 1)
        {
            BorderWidth = 1;
            return;
        }

        model.BorderWidth = value;
        var newTextValue = value.ToString(CultureInfo.InvariantCulture);
        if (!string.Equals(BorderWidthText, newTextValue, StringComparison.Ordinal))
        {
            BorderWidthText = newTextValue;
        }
    }

    partial void OnBorderWidthTextChanged(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
        {
            BorderWidth = parsedValue;
        }
    }

    protected override string ValidateColumn(string columnName)
    {
        if (columnName == nameof(CornerRadiusText)) return ValidateCornerRadiusText(CornerRadiusText);
        if (columnName == nameof(BorderWidthText)) return ValidateBorderWidthText(BorderWidthText);
        return string.Empty;
    }

    protected override void OnDisplaySizeChanged()
    {
        OnPropertyChanged(nameof(CornerRadiusToolTip));
    }

    private string BuildCornerRadiusToolTip()
    {
        if (SizeWidth.HasValue && SizeHeight.HasValue && SizeWidth.Value > 0 && SizeHeight.Value > 0)
        {
            var maxRadius = Math.Min(SizeWidth.Value, SizeHeight.Value) / 2f;
            return $"Effective max with current size: {maxRadius.ToString("0.##", CultureInfo.InvariantCulture)}";
        }

        return "Effective max is min(width, height) / 2.";
    }

    private static string ValidateCornerRadiusText(string cornerRadiusText)
    {
        if (string.IsNullOrWhiteSpace(cornerRadiusText))
        {
            return "Corner radius value is required.";
        }

        if (!int.TryParse(cornerRadiusText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return "Invalid corner radius value. Please enter an integer number.";
        }

        if (parsedValue < 0)
        {
            return "Corner radius must be greater than or equal to 0.";
        }

        return string.Empty;
    }

    private static string ValidateBorderWidthText(string borderWidthText)
    {
        if (string.IsNullOrWhiteSpace(borderWidthText))
        {
            return "Border width value is required.";
        }

        if (!int.TryParse(borderWidthText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return "Invalid border width value. Please enter an integer number.";
        }

        if (parsedValue < 1)
        {
            return "Border width must be greater than or equal to 1.";
        }

        return string.Empty;
    }
}

/// <summary>
/// ViewModel for DisplayItemImage
/// </summary>
public partial class DisplayItemImageViewModel(DisplayItemImage model, ImageManager imageManager, IViewModel parentViewModel)
    : DisplayItemViewModel(model, parentViewModel)
{
    public override ImageSource? Icon => ParentViewModel.ParentWindow.FindResource(DisplayItemImage.UiIcon) as ImageSource;

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
        var imageSelector = new ImageSelector(imageManager, RelativePath, ParentViewModel.ParentWindow);
        if (imageSelector.ShowDialog() == true)
        {
            RelativePath = imageSelector.RelativePath;
        }
    }
}

/// <summary>
/// ViewModel for DisplayItemText
/// </summary>
public partial class DisplayItemTextViewModel(DisplayItemText model, IViewModel parentViewModel)
    : DisplayItemViewModel(model, parentViewModel), IFontSelectable, IColorSelectable
{
    public override ImageSource? Icon => ParentViewModel.ParentWindow.FindResource(DisplayItemText.UiIcon) as ImageSource;

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
    private readonly DisplayItemValue _model;

    public DisplayItemValueViewModel(DisplayItemValue model, IViewModel parentViewModel) : base(model, parentViewModel)
    {
        _model = model;
        _expressionControlPropertyViewModel = new ExpressionControlViewModel(model.NCalcPropertyHolder)
        {
            ExpressionLabel = "Expression:",
            ExpressionToolTip = "Please enter a valid NCalc expression, that returns a value",
            Example = "round( [DataCorePlugin.GameData.Fuel], 1)",
            FetchShakeItProfilesCallback = FetchShakeItProfilesCallback
        };
        _displayFormat = model.DisplayFormat;
        _font = model.Font;
        _imageSharpColor = model.Color;
    }

    public override ImageSource? Icon => ParentViewModel.ParentWindow.FindResource(DisplayItemValue.UiIcon) as ImageSource;

    [ObservableProperty] private ExpressionControlViewModel _expressionControlPropertyViewModel;

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