// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Newtonsoft.Json;
using StreamDeckSimHub.Plugin.Actions.JsonSettings;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;

public class SettingsDto
{
    public required SizeDto KeySize { get; set; } = new();

    [JsonIgnore]
    public List<DisplayItemDto> DisplayItems { get; set; } = [];

    [JsonIgnore]
    public Dictionary<string, List<CommandItemDto>> CommandItems { get; set; } = [];

    // ReSharper disable once MemberCanBePrivate.Global (needed for System.Text.Json serialization)
    public string DisplayItemsString { get; set; } = string.Empty;
    // ReSharper disable once MemberCanBePrivate.Global (needed for System.Text.Json serialization)
    public string CommandItemsString { get; set; } = string.Empty;

    /// <summary>
    /// Newtonsoft.Json cannot deserialize object hierarchies with polymorphic types just by using annotations. Instead,
    /// the serializer needs to be configured accordingly, which is not possible, because the serializer is configured
    /// by SharpDeck.
    /// <p/>
    /// We therefore serialize the items to strings and deserialize them System.Text.Json.
    /// </summary>
    public void SerializeItemsToStrings()
    {
        DisplayItemsString = JsonSerializer.Serialize(DisplayItems);
        CommandItemsString = JsonSerializer.Serialize(CommandItems);
    }

    /// <summary>
    /// see <see cref="SerializeItemsToStrings"/> for details.
    /// </summary>
    public void DeserializeItemsFromStrings()
    {
        if (!string.IsNullOrEmpty(DisplayItemsString))
        {
            DisplayItems = JsonSerializer.Deserialize<List<DisplayItemDto>>(DisplayItemsString) ?? [];
        }

        if (!string.IsNullOrEmpty(CommandItemsString))
        {
            CommandItems = JsonSerializer.Deserialize<Dictionary<string, List<CommandItemDto>>>(CommandItemsString) ?? [];
        }
    }
}