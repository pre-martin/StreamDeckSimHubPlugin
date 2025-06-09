// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using NLog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class SettingsConverter(PropertyComparer propertyComparer, ImageManager imageManager)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public Settings SettingsToModel(SettingsDto dto, StreamDeckKeyInfo keyInfo)
    {
        var settings = new Settings
        {
            KeySize = new Size(dto.KeySize.Width, dto.KeySize.Height),
            KeyInfo = keyInfo, // TODO Required?
            DisplayItems = new ObservableCollection<DisplayItem>(
                dto.DisplayItems
                .Select(di => DisplayItemToModel(di, keyInfo))
                .Where(di => di != null)!),
            Commands = CommandsToModel(dto.Commands)
        };
        return settings;
    }

    public SettingsDto SettingsToDto(Settings settings)
    {
        var settingsDto = new SettingsDto
        {
            KeySize = new SizeDto { Width = settings.KeySize.Width, Height = settings.KeySize.Height },
            DisplayItems = settings.DisplayItems
                .Select(DisplayItemToDto)
                .Where(di => di != null)
                .ToList()!,
            Commands = CommandsToDto(settings.Commands)
        };
        return settingsDto;
    }

    #region DisplayItem

    private DisplayItem? DisplayItemToModel(DisplayItemDto dto, StreamDeckKeyInfo keyInfo)
    {
        DisplayItem? displayItem = dto switch
        {
            DisplayItemImageDto imageDto => new DisplayItemImage
            {
                Image = imageManager.GetCustomImage(imageDto.RelativePath, keyInfo),
                RelativePath = imageDto.RelativePath
            },
            DisplayItemTextDto textDto => new DisplayItemText
            {
                Text = textDto.Text,
                Font = FontToModel(textDto.FontName, textDto.FontSize, textDto.FontStyle),
                Color = Color.TryParseHex(textDto.Color, out var color) ? color : Color.White
            },
            DisplayItemValueDto valueDto => new DisplayItemValue
            {
                Property = valueDto.Property,
                DisplayFormat = valueDto.DisplayFormat,
                Font = FontToModel(valueDto.FontName, valueDto.FontSize, valueDto.FontStyle),
                Color = Color.TryParseHex(valueDto.Color, out var color) ? color : Color.White
            },
            _ => null
        };

        if (displayItem != null)
        {
            displayItem.Name = dto.Name;
            displayItem.DisplayParameters = DisplayParametersToModel(dto.DisplayParameters);
            displayItem.VisibilityConditions = new ObservableCollection<ConditionExpression>(dto.VisibilityConditions.Select(propertyComparer.Parse));
            return displayItem;
        }

        Logger.Error($"Don't know how to convert DisplayItemDto of type {dto.GetType()}. Item will be ignored.");
        return null;
    }

    private DisplayItemDto? DisplayItemToDto(DisplayItem model)
    {
        DisplayItemDto? dto = model switch
        {
            DisplayItemImage image => new DisplayItemImageDto
            {
                RelativePath = image.RelativePath,
            },
            DisplayItemText text => new DisplayItemTextDto
            {
                Text = text.Text,
                FontName = text.Font.Name,
                FontStyle = FontStyleToDto(text.Font),
                FontSize = text.Font.Size,
                Color = text.Color.ToHex()
            },
            DisplayItemValue value => new DisplayItemValueDto
            {
                Property = value.Property,
                DisplayFormat = value.DisplayFormat,
                FontName = value.Font.Name,
                FontStyle = FontStyleToDto(value.Font),
                FontSize = value.Font.Size,
                Color = value.Color.ToHex()
            },
            _ => null
        };

        if (dto != null)
        {
            dto.Name = model.Name;
            dto.DisplayParameters = DisplayParametersToDto(model.DisplayParameters);
            dto.VisibilityConditions = model.VisibilityConditions.Select(propertyComparer.ToParsableString).ToList();
            return dto;
        }

        Logger.Error($"Don't know how to convert DisplayItem of type {model.GetType()}. Item will be ignored.");
        return null;
    }

    private Font FontToModel(string fontName, float fontSize, string fontStyle)
    {
        return SystemFonts.Collection.TryGet(fontName, out var fontFamily)
            ? fontFamily.CreateFont(fontSize, Enum.TryParse(fontStyle, out FontStyle style) ? style : FontStyle.Regular)
            : SystemFonts.CreateFont("Arial", 12, FontStyle.Regular);
    }

    private string FontStyleToDto(Font font)
    {
        if (font is { IsBold: true, IsItalic: true }) return nameof(FontStyle.BoldItalic);
        if (font.IsBold) return nameof(FontStyle.Bold);
        if (font.IsItalic) return nameof(FontStyle.Italic);
        return nameof(FontStyle.Regular);
    }

    private DisplayParameters DisplayParametersToModel(DisplayParametersDto dto)
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

    private DisplayParametersDto DisplayParametersToDto(DisplayParameters model)
    {
        return new DisplayParametersDto
        {
            Position = new PointDto { X = model.Position.X, Y = model.Position.Y },
            Transparency = model.Transparency,
            Rotation = model.Rotation,
            Scale = model.Scale.ToString(),
            Size = model.Size != null ? new SizeDto { Width = model.Size.Value.Width, Height = model.Size.Value.Height } : null
        };
    }

    #endregion

    #region CommandToModel

    private SortedDictionary<StreamDeckAction, ObservableCollection<CommandItem>> CommandsToModel(Dictionary<string, List<CommandItemDto>> dtos)
    {
        var commands = new SortedDictionary<StreamDeckAction, ObservableCollection<CommandItem>>();
        // Ensure that the model dictionary contains entries for all possible actions. So iterate by using the enum values.
        foreach (StreamDeckAction action in Enum.GetValues(typeof(StreamDeckAction)))
        {
            if (dtos.TryGetValue(action.ToString(), out var commandItemDtos))
            {
                List<CommandItem> commandItems = commandItemDtos.Select(CommandItemToModel).Where(ci => ci != null).ToList()!;
                commands[action] = new ObservableCollection<CommandItem>(commandItems);
            }
            else
            {
                commands[action] = new ObservableCollection<CommandItem>();
            }
        }

        return commands;
    }

    private Dictionary<string, List<CommandItemDto>> CommandsToDto(SortedDictionary<StreamDeckAction, ObservableCollection<CommandItem>> commands)
    {
        var commandDtos = new Dictionary<string, List<CommandItemDto>>();
        foreach (var (action, commandItems) in commands)
        {
            var actionName = action.ToString();
            List<CommandItemDto> commandItemDtos = commandItems.Select(CommandItemToDto).Where(dto => dto != null).ToList()!;
            commandDtos[actionName] = commandItemDtos;
        }

        return commandDtos;
    }

    private CommandItem? CommandItemToModel(CommandItemDto dto)
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
            commandItem.Name = dto.Name;
            commandItem.ActiveConditions = new ObservableCollection<ConditionExpression>(
                dto.ActiveConditions
                .Select(propertyComparer.Parse));
            return commandItem;
        }

        Logger.Error($"Don't know how to convert CommandItemDto of type {dto.GetType()}. Item will be ignored.");
        return null;
    }

    private CommandItemDto? CommandItemToDto(CommandItem model)
    {
        CommandItemDto? dto = model switch
        {
            CommandItemKeypress keypress => new CommandItemKeypressDto
            {
                Key = keypress.Key,
                ModifierCtrl = keypress.ModifierCtrl,
                ModifierAlt = keypress.ModifierAlt,
                ModifierShift = keypress.ModifierShift
            },
            CommandItemSimHubControl control => new CommandItemSimHubControlDto
            {
                Control = control.Control
            },
            CommandItemSimHubRole role => new CommandItemSimHubRoleDto
            {
                Role = role.Role
            },
            _ => null
        };

        if (dto != null)
        {
            dto.Name = model.Name;
            dto.ActiveConditions = model.ActiveConditions
                .Select(propertyComparer.ToParsableString)
                .ToList();
            return dto;
        }

        Logger.Error($"Don't know how to convert CommandItem of type {model.GetType()}. Item will be ignored.");
        return null;
    }

    #endregion
}