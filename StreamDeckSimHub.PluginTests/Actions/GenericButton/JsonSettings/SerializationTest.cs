// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.Model;

namespace StreamDeckSimHub.PluginTests.Actions.GenericButton.JsonSettings;

public class SerializationTest
{
    // see SharpDeck.Connectivity.Net.StreamDeckWebSocketConnection
    private JsonSerializerSettings DefaultJsonSettings => new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Formatting = Formatting.None,
    };

    [Test]
    public void SerializeDeserialize()
    {
        var settingsDto = BuildSettingsDto();
        settingsDto.SerializeItemsToStrings();

        // see SharpDeck.StreamDeckAction#SetSettingsAsync()
        var serialized = JObject.FromObject(settingsDto, JsonSerializer.Create(DefaultJsonSettings));

        Assert.That(serialized.ContainsKey("keySize"), Is.True);
        Assert.That(serialized.ContainsKey("displayItemsString"), Is.True);
        Assert.That(serialized.ContainsKey("commandItemsString"), Is.True);

        var text = JsonConvert.SerializeObject(serialized, DefaultJsonSettings);

        // see SharpDeck.Connectivity.Net.StreamDeckWebSocketConnection#WebSocket_MessageReceived
        var deserialized = JObject.Parse(text);
        // see SharpDeck.Events.Received.SettingsPayload#GetSettings<T>()
        var newSettingsDto = deserialized.ToObject<SettingsDto>();

        Assert.That(newSettingsDto, Is.Not.Null);
        newSettingsDto.DeserializeItemsFromStrings();

        Assert.That(newSettingsDto.KeySize, Is.Not.Null);
        Assert.That(newSettingsDto.KeySize.Width, Is.EqualTo(140));
        Assert.That(newSettingsDto.KeySize.Height, Is.EqualTo(100));
        Assert.That(newSettingsDto.DisplayItems, Is.Not.Null);
        Assert.That(newSettingsDto.DisplayItems, Has.Count.EqualTo(1));
        Assert.That(newSettingsDto.DisplayItems[0], Is.TypeOf<DisplayItemImageDto>());
        Assert.That(newSettingsDto.CommandItems, Is.Not.Null);
        Assert.That(newSettingsDto.CommandItems, Has.Count.EqualTo(3));
        Assert.That(newSettingsDto.CommandItems[nameof(StreamDeckAction.KeyDown)], Has.Count.EqualTo(1));
        Assert.That(newSettingsDto.CommandItems[nameof(StreamDeckAction.KeyDown)][0], Is.TypeOf<CommandItemSimHubRoleDto>());
        Assert.That(newSettingsDto.CommandItems[nameof(StreamDeckAction.DialLeft)], Has.Count.EqualTo(2));
        Assert.That(newSettingsDto.CommandItems[nameof(StreamDeckAction.DialLeft)][0], Is.TypeOf<CommandItemSimHubControlDto>());
        Assert.That(newSettingsDto.CommandItems[nameof(StreamDeckAction.DialLeft)][1], Is.TypeOf<CommandItemSimHubRoleDto>());
        Assert.That(newSettingsDto.CommandItems[nameof(StreamDeckAction.TouchTap)], Is.Empty);
    }

    private SettingsDto BuildSettingsDto()
    {
        return new SettingsDto
        {
            KeySize = new SizeDto
            {
                Width = 140,
                Height = 100
            },
            DisplayItems = [
                new DisplayItemImageDto
                {
                    Name = "My Image",
                    DisplayParameters = new DisplayParametersDto(),
                    VisibilityConditions = [],
                    RelativePath = "images/icon.png"
                }
            ],
            CommandItems = new Dictionary<string, List<CommandItemDto>>
            {
                {
                    nameof(StreamDeckAction.KeyDown), [
                        new CommandItemSimHubRoleDto
                        {
                            Name = "My Role action",
                            ActiveConditions = [],
                            Role = "SomeRole"
                        }
                    ]
                },
                {
                    nameof(StreamDeckAction.DialLeft), [
                        new CommandItemSimHubControlDto
                        {
                            Name = "My Control action",
                            ActiveConditions = [],
                            Control = "SomeControl"
                        },
                        new CommandItemSimHubRoleDto
                        {
                            Name = "Another Role action",
                            ActiveConditions = [],
                            Role = "SomeOtherRole"
                        }
                    ]
                },
                { nameof(StreamDeckAction.TouchTap), [] }
            },
        };
    }
}