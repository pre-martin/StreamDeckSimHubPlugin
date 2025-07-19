// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.PropertyLogic;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public partial class DisplayItemValue : DisplayItem
{
    public const string UiName = "Value";

    [ObservableProperty] private NCalcHolder _nCalcPropertyHolder;

    public DisplayItemValue()
    {
        // Set in constructor via the generated property to ensure that OnNCalcPropertyHolderChanged is called.
        NCalcPropertyHolder = new NCalcHolder();
    }

    partial void OnNCalcPropertyHolderChanged(NCalcHolder value)
    {
        value.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
        // No event handler on UsedProperties.CollectionChanged.
        // We rely only on the event of NCalcHolder.ExpressionString. This means that UsedProperties already has to contain
        // the new state when ExpressionString is being updated.
        //value.UsedProperties.CollectionChanged += (_, _) => OnPropertyChanged(nameof(NCalcHolder.UsedProperties));
    }

    [ObservableProperty] private string _displayFormat = string.Empty;
    [ObservableProperty] private Font _font = SystemFonts.CreateFont("Arial", 16, FontStyle.Regular);
    [ObservableProperty] private Color _color = Color.White;

    protected override string RawDisplayName => !string.IsNullOrWhiteSpace(Name) ? Name : "Value";

    public static DisplayItemValue Create()
    {
        return new DisplayItemValue();
    }
}

