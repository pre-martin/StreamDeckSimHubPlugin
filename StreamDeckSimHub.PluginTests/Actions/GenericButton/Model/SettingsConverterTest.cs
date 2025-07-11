// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.IO.Abstractions.TestingHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.GenericButton.Model;
using StreamDeckSimHub.Plugin.Actions.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.PluginTests.Actions.GenericButton.Model;

public class SettingsConverterTest
{
    private SettingsConverter _converter;

    // see SharpDeck.Connectivity.Net.StreamDeckWebSocketConnection
    private JsonSerializerSettings DefaultJsonSettings => new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Formatting = Formatting.None,
    };

    [SetUp]
    public void Setup()
    {
        var fileSystem = new MockFileSystem();
        var imageUtils = Mock.Of<ImageUtils>();
        var imageManager = new ImageManager(fileSystem, imageUtils);
        var ncalcHandler = new NCalcHandler();
        _converter = new SettingsConverter(imageManager, ncalcHandler);
    }

    [Test]
    public void SettingsToJsonAndBack()
    {
        // Arrange
        var settings = BuildSettings();

        // Serialize and Deserialize as in SerializationTest
        var dto = _converter.SettingsToDto(settings);
        var serialized = JObject.FromObject(dto, JsonSerializer.Create(DefaultJsonSettings));
        var text = JsonConvert.SerializeObject(serialized, DefaultJsonSettings);
        var deserialized = JObject.Parse(text);
        var newDto = deserialized.ToObject<SettingsDto>();
        Assert.That(newDto, Is.Not.Null);
        newDto.DeserializeItemsFromStrings();
        var newSettings = _converter.SettingsToModel(newDto, StreamDeckKeyInfoBuilder.DefaultKeyInfo);

        // Assert
        Assert.That(newSettings.KeySize, Is.EqualTo(settings.KeySize));
        Assert.That(newSettings.DisplayItems.Count, Is.EqualTo(settings.DisplayItems.Count));
        Assert.That(newSettings.DisplayItems[0].Name, Is.EqualTo(settings.DisplayItems[0].Name));
        Assert.That(newSettings.DisplayItems[1].Name, Is.EqualTo(settings.DisplayItems[1].Name));
        Assert.That(((DisplayItemText)newSettings.DisplayItems[1]).Font.Family.Name, Is.EqualTo("Arial"));
        Assert.That(((DisplayItemText)newSettings.DisplayItems[1]).Font.Size, Is.EqualTo(24f));
        Assert.That(((DisplayItemText)newSettings.DisplayItems[1]).Font.FontStyle(), Is.EqualTo(FontStyle.Italic));
        Assert.That(newSettings.CommandItems[StreamDeckAction.KeyDown][0].Name,
            Is.EqualTo(settings.CommandItems[StreamDeckAction.KeyDown][0].Name));
        Assert.That(newSettings.CommandItems[StreamDeckAction.DialLeft][0].Name,
            Is.EqualTo(settings.CommandItems[StreamDeckAction.DialLeft][0].Name));
        Assert.That(newSettings.CommandItems[StreamDeckAction.TouchTap][0].Name,
            Is.EqualTo(settings.CommandItems[StreamDeckAction.TouchTap][0].Name));
        Assert.That(newSettings.CommandItems[StreamDeckAction.TouchTap][1].Name,
            Is.EqualTo(settings.CommandItems[StreamDeckAction.TouchTap][1].Name));
    }

    [Test]
    public void DeserializeInvalidSettings()
    {
        var settingsDto = new SettingsDto
        {
            KeySize = new SizeDto { Width = 140, Height = 100 },
            DisplayItemsString = "invalid json",
            CommandItemsString = "invalid json"
        };

        var settings = _converter.SettingsToModel(settingsDto, StreamDeckKeyInfoBuilder.DefaultKeyInfo);

        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.KeySize, Is.EqualTo(new Size(140, 100)));
        Assert.That(settings.DisplayItems.Count, Is.EqualTo(0));
        Assert.That(settings.CommandItems.Count, Is.EqualTo(5)); // Default command items should be present
        Assert.That(settings.CommandItems[StreamDeckAction.KeyDown].Count, Is.EqualTo(0));
        Assert.That(settings.CommandItems[StreamDeckAction.DialLeft].Count, Is.EqualTo(0));
    }

    private Settings BuildSettings()
    {
        var settings = new Settings
        {
            KeySize = new Size(140, 100)
        };
        settings.DisplayItems.Add(new DisplayItemText { Name = "Text1", Text = "Hello" });
        settings.DisplayItems.Add(new DisplayItemText
            { Name = "Text2", Text = "World", Font = SystemFonts.CreateFont("Arial", 24f, FontStyle.Italic) });
        settings.CommandItems[StreamDeckAction.KeyDown].Add(new CommandItemKeypress { Name = "Cmd1", Key = "A" });
        settings.CommandItems[StreamDeckAction.DialLeft].Add(new CommandItemKeypress { Name = "Cmd2", Key = "B" });
        settings.CommandItems[StreamDeckAction.TouchTap]
            .Add(new CommandItemSimHubControl { Name = "Control1", Control = "ControlA" });
        settings.CommandItems[StreamDeckAction.TouchTap].Add(new CommandItemSimHubRole { Name = "Role1", Role = "RoleA" });

        return settings;
    }
}