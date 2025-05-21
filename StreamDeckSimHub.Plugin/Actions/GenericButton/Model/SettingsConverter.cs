// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NLog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class SettingsConverter(PropertyComparer propertyComparer, ImageManager imageManager)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public Settings ToSettings(SettingsDto dto, StreamDeckKeyInfo keyInfo)
    {
        var settings = new Settings
        {
            KeySize = new Size(dto.KeySize.Width, dto.KeySize.Height),
            KeyInfo = keyInfo, // TODO Required?
            DisplayItems = dto.DisplayItems
                .Select(di => ToDisplayItem(di, keyInfo))
                .Where(di => di != null)
                .ToList()!,
            Commands = ToCommands(dto.Commands)
        };

        return settings;
    }

    #region DisplayItemToModel

    private DisplayItem? ToDisplayItem(DisplayItemDto dto, StreamDeckKeyInfo keyInfo)
    {
        DisplayItem? displayItem = dto switch
        {
            DisplayItemImageDto imageDto => new DisplayItemImage
            {
                Image = imageManager.GetCustomImage(imageDto.RelativePath, keyInfo)
            },
            DisplayItemTextDto textDto => new DisplayItemText
            {
                Text = textDto.Text,
                Font = SystemFonts.Collection.TryGet(textDto.FontName, out var fontFamily)
                    ? fontFamily.CreateFont(textDto.FontSize)
                    : SystemFonts.CreateFont("Arial", 12),
                Color = Color.TryParseHex(textDto.Color, out var color) ? color : Color.White
            },
            DisplayItemValueDto valueDto => new DisplayItemValue
            {
                Property = valueDto.Property,
                DisplayFormat = valueDto.DisplayFormat,
                Font = SystemFonts.Collection.TryGet(valueDto.FontName, out var fontFamily)
                    ? fontFamily.CreateFont(valueDto.FontSize)
                    : SystemFonts.CreateFont("Arial", 12),
                Color = Color.TryParseHex(valueDto.Color, out var color) ? color : Color.White
            },
            _ => null
        };

        if (displayItem != null)
        {
            displayItem.Name = dto.Name;
            displayItem.DisplayParameters = ToDisplayParameters(dto.DisplayParameters);
            displayItem.VisibilityConditions = dto.VisibilityConditions.Select(propertyComparer.Parse).ToList();
            return displayItem;
        }

        Logger.Error($"Don't know how to convert DisplayItem of type {dto.GetType()}. Item will be ignored.");
        return null;
    }

    private DisplayParameters ToDisplayParameters(DisplayParametersDto dto)
    {
        return new DisplayParameters
        {
            Position = new Point(dto.Position.X, dto.Position.Y),
            Transparency = dto.Transparency,
            Rotation = dto.Rotation,
            Scale = Enum.TryParse(dto.Scale, out ScaleType scaleType) ? scaleType : ScaleType.None,
            Size = dto.Size != null ? new Size(dto.Size.Width, dto.Size.Height) : null
        };
    }

    #endregion

    #region CommandToModel

    private SortedDictionary<StreamDeckAction, SortedDictionary<int, CommandItem>> ToCommands(
        Dictionary<string, SortedDictionary<int, CommandItemDto>> commandDtos)
    {
        var commands = new SortedDictionary<StreamDeckAction, SortedDictionary<int, CommandItem>>();
        // Ensure that the model dictionary contains entries for all possible actions. So iterate by using the enum values.
        foreach (StreamDeckAction action in Enum.GetValues(typeof(StreamDeckAction)))
        {
            if (commandDtos.TryGetValue(action.ToString(), out var index2CommandDtoDict))
            {
                var index2CommandDict = new SortedDictionary<int, CommandItem>();
                foreach (var kvp in index2CommandDtoDict)
                {
                    var modelItem = ToCommandItem(kvp.Value);
                    if (modelItem != null)
                    {
                        index2CommandDict[kvp.Key] = modelItem;
                    }
                }
                commands[action] = index2CommandDict;
            }
            else
            {
                commands[action] = new SortedDictionary<int, CommandItem>();
            }
        }
        return commands;
    }

    private CommandItem? ToCommandItem(CommandItemDto dto)
    {
        CommandItem? commandItem = dto switch
        {
            CommandItemKeypressDto keypressDto => new CommandItemKeypress
            {
                Key = keypressDto.Key,
                ModifierCtrl = keypressDto.ModifierCtrl,
                ModifierAlt = keypressDto.ModifierAlt,
                ModifierShift = keypressDto.ModifierShift,
                Hotkey = KeyboardUtils.CreateHotkey(keypressDto.ModifierCtrl, keypressDto.ModifierAlt, keypressDto.ModifierShift,
                    keypressDto.Key),
            },
            CommandItemSimHubControlDto controlDto => new CommandItemSimHubControl
            {
                Control = controlDto.Control
            },
            CommandItemSimHubRoleDto roleDto => new CommandItemSimHubRole
            {
                Role = roleDto.Role
            },
            _ => null
        };

        if (commandItem != null)
        {
            commandItem.ActiveConditions = dto.ActiveConditions
                .Select(propertyComparer.Parse)
                .ToList();
            return commandItem;
        }

        Logger.Error($"Don't know how to convert CommandItem of type {dto.GetType()}. Item will be ignored.");
        return null;
    }

    #endregion
}

