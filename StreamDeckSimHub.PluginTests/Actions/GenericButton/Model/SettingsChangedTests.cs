// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.ComponentModel;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.PluginTests.Actions.GenericButton.Model;

public class SettingsChangedTests
{
    private readonly NCalcHandler _ncalcHandler = new();

    [Test]
    public void AddDisplayItem_MustFireEvent()
    {
        var settings = new Settings { KeySize = StreamDeckKeyInfoBuilder.DefaultKeyInfo.KeySize };
        PropertyChangedEventArgs? lastArgs = null;
        settings.SettingsChanged += (_, args) => lastArgs = args;

        // Add new DisplayItem
        settings.DisplayItems.Add(DisplayItemText.Create());
        Assert.That(lastArgs, Is.Not.Null);
        Assert.That(lastArgs.PropertyName, Is.EqualTo(nameof(settings.DisplayItems)));
    }

    [Test]
    public void ModifyDisplayItem_Condition_MustFireEvent()
    {
        var settings = new Settings { KeySize = StreamDeckKeyInfoBuilder.DefaultKeyInfo.KeySize };
        // Add new DisplayItem
        settings.DisplayItems.Add(DisplayItemText.Create());

        PropertyChangedEventArgs? lastArgs = null;
        settings.SettingsChanged += (_, args) => lastArgs = args;

        // Add a condition just as the viewmodel would do (see ItemViewModel.cs#OnConditionStringChanged)
        _ncalcHandler.UpdateNCalcHolder("1 > 0", settings.DisplayItems[0].NCalcConditionHolder);
        Assert.That(lastArgs, Is.Not.Null);
    }

    [Test]
    public void ModifyTextItem_Value_MustFireEvent()
    {
        var settings = new Settings { KeySize = StreamDeckKeyInfoBuilder.DefaultKeyInfo.KeySize };
        // Add new DisplayItem
        settings.DisplayItems.Add(DisplayItemValue.Create());

        PropertyChangedEventArgs? lastArgs = null;
        settings.SettingsChanged += (_, args) => lastArgs = args;

        // Add a condition just as the viewmodel would do (see DisplayItemValeViewModel.cs#OnPropertyStringChanged)
        _ncalcHandler.UpdateNCalcHolder("1", ((DisplayItemValue)settings.DisplayItems[0]).NCalcPropertyHolder);
        Assert.That(lastArgs, Is.Not.Null);
    }
}