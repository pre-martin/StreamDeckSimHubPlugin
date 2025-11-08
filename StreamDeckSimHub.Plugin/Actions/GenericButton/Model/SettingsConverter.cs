// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using NLog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.ActionEditor.Tools;
using StreamDeckSimHub.Plugin.Actions.GenericButton.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.JsonSettings;
using StreamDeckSimHub.Plugin.Actions.Model;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

public class SettingsConverter(ImageManager imageManager, NCalcHandler ncalcHandler)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Have the settings been modified during the conversion from DTO to model?
    /// </summary>
    public bool SettingsModified { get; private set; }

    public Settings SettingsToModel(SettingsDto dto, StreamDeckKeyInfo keyInfo)
    {
        SettingsModified = false;
        try
        {
            dto.DeserializeItemsFromStrings();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to deserialize items from strings in SettingsDto (Name=\"{0}\")", dto.Name);
        }

        var settings = new Settings
        {
            Name = dto.Name,
            KeySize = new Size(dto.KeySize.Width, dto.KeySize.Height)
        };


        foreach (var displayItem in dto.DisplayItems.Select(di => DisplayItemToModel(di, keyInfo)).Where(di => di != null))
        {
            settings.DisplayItems.Add(displayItem!);
        }

        // To ensure that we only convert actions that are actually known, we iterate over the Settings, which contains all possible actions.
        foreach (var action in settings.CommandItems.Keys)
        {
            if (dto.CommandItems.ContainsKey(action.ToString()))
            {
                foreach (var commandItem in dto.CommandItems[action.ToString()].Select(CommandItemToModel)
                             .Where(ci => ci != null))
                {
                    settings.CommandItems[action].Add(commandItem!);
                }
            }
        }

        return settings;
    }

    public SettingsDto SettingsToDto(Settings settings)
    {
        var settingsDto = new SettingsDto
        {
            Name = settings.Name,
            KeySize = new SizeDto { Width = settings.KeySize.Width, Height = settings.KeySize.Height },
            DisplayItems = settings.DisplayItems
                .Select(DisplayItemToDto)
                .Where(di => di != null)
                .ToList()!,
            CommandItems = CommandsToDto(settings.CommandItems)
        };
        settingsDto.SerializeItemsToStrings();
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
                NCalcPropertyHolder = ExpressionToNCalcHolder(valueDto.Property, valueDto.PropertyShakeItDictionary),
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
            displayItem.NCalcConditionHolder = ExpressionToNCalcHolder(dto.ConditionsString, dto.ConditionsShakeItDictionary);
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
                Name = model.Name,
                DisplayParameters = DisplayParametersToDto(model.DisplayParameters),
                ConditionsString = model.NCalcConditionHolder.ExpressionString,
                ConditionsShakeItDictionary = model.NCalcConditionHolder.ShakeItDictionary,
                RelativePath = image.RelativePath,
            },
            DisplayItemText text => new DisplayItemTextDto
            {
                Name = model.Name,
                DisplayParameters = DisplayParametersToDto(model.DisplayParameters),
                ConditionsString = model.NCalcConditionHolder.ExpressionString,
                ConditionsShakeItDictionary = model.NCalcConditionHolder.ShakeItDictionary,
                Text = text.Text,
                FontName = text.Font.Family.Name,
                FontStyle = FontStyleToDto(text.Font),
                FontSize = text.Font.Size,
                Color = text.Color.ToHexWithoutAlpha()
            },
            DisplayItemValue value => new DisplayItemValueDto
            {
                Name = model.Name,
                DisplayParameters = DisplayParametersToDto(model.DisplayParameters),
                ConditionsString = model.NCalcConditionHolder.ExpressionString,
                ConditionsShakeItDictionary = model.NCalcConditionHolder.ShakeItDictionary,
                Property = value.NCalcPropertyHolder.ExpressionString,
                PropertyShakeItDictionary = value.NCalcPropertyHolder.ShakeItDictionary,
                DisplayFormat = value.DisplayFormat,
                FontName = value.Font.Family.Name,
                FontStyle = FontStyleToDto(value.Font),
                FontSize = value.Font.Size,
                Color = value.Color.ToHexWithoutAlpha()
            },
            _ => null
        };

        if (dto != null)
        {
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
        return font.FontStyle().ToString();
    }

    private DisplayParameters DisplayParametersToModel(DisplayParametersDto dto)
    {
        return new DisplayParameters
        {
            Transparency = dto.Transparency,
            Position = new Point(dto.Position.X, dto.Position.Y),
            Size = dto.Size != null ? new Size(dto.Size.Width, dto.Size.Height) : null,
            Scale = Enum.TryParse(dto.Scale, out ScaleType scaleType) ? scaleType : ScaleType.None,
            Rotation = dto.Rotation
        };
    }

    private DisplayParametersDto DisplayParametersToDto(DisplayParameters model)
    {
        return new DisplayParametersDto
        {
            Transparency = model.Transparency,
            Position = new PointDto { X = model.Position.X, Y = model.Position.Y },
            Size = model.Size != null ? new SizeDto { Width = model.Size.Value.Width, Height = model.Size.Value.Height } : null,
            Scale = model.Scale.ToString(),
            Rotation = model.Rotation,
        };
    }

    #endregion

    #region CommandToModel

    private Dictionary<string, List<CommandItemDto>> CommandsToDto(
        SortedDictionary<StreamDeckAction, ObservableCollection<CommandItem>> commands)
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
                LongEnabled = keypressDto.LongEnabled,
            },
            CommandItemSimHubControlDto controlDto => new CommandItemSimHubControl
            {
                Control = controlDto.Control,
                LongEnabled = controlDto.LongEnabled
            },
            CommandItemSimHubRoleDto roleDto => new CommandItemSimHubRole
            {
                Role = roleDto.Role,
                LongEnabled = roleDto.LongEnabled
            },
            _ => null
        };

        if (commandItem != null)
        {
            commandItem.Name = dto.Name;
            commandItem.NCalcConditionHolder = ExpressionToNCalcHolder(dto.ConditionsString, dto.ConditionsShakeItDictionary);
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
                Name = model.Name,
                ConditionsString = model.NCalcConditionHolder.ExpressionString,
                ConditionsShakeItDictionary = model.NCalcConditionHolder.ShakeItDictionary,
                Key = keypress.Key,
                ModifierCtrl = keypress.ModifierCtrl,
                ModifierAlt = keypress.ModifierAlt,
                ModifierShift = keypress.ModifierShift,
                LongEnabled = keypress.LongEnabled,
            },
            CommandItemSimHubControl control => new CommandItemSimHubControlDto
            {
                Name = model.Name,
                ConditionsString = model.NCalcConditionHolder.ExpressionString,
                ConditionsShakeItDictionary = model.NCalcConditionHolder.ShakeItDictionary,
                Control = control.Control,
                LongEnabled = control.LongEnabled
            },
            CommandItemSimHubRole role => new CommandItemSimHubRoleDto
            {
                Name = model.Name,
                ConditionsString = model.NCalcConditionHolder.ExpressionString,
                ConditionsShakeItDictionary = model.NCalcConditionHolder.ShakeItDictionary,
                Role = role.Role,
                LongEnabled = role.LongEnabled
            },
            _ => null
        };

        if (dto != null)
        {
            return dto;
        }

        Logger.Error($"Don't know how to convert CommandItem of type {model.GetType()}. Item will be ignored.");
        return null;
    }

    #endregion

    #region Common Converters

    private NCalcHolder ExpressionToNCalcHolder(string expressionString, Dictionary<string, List<ShakeItEntry>> shakeItDictionaryDto)
    {
        try
        {
            var usedProperties = ncalcHandler.Parse(expressionString, out var ncalcExpression);
            var ncalcHolder = new NCalcHolder
            {
                ExpressionString = expressionString,
                ShakeItDictionary = shakeItDictionaryDto,
                NCalcExpression = ncalcExpression
            };
            ncalcHolder.UsedProperties.UnionWith(usedProperties);
            // We remove unused ShakeIt properties from the dictionary here when loading.
            if (ncalcHandler.CleanupShakeItDictionary(ncalcHolder))
            {
                SettingsModified = true;
            }

            return ncalcHolder;
        }
        catch (Exception e)
        {
            Logger.Warn(e, $"Failed to create ConditionsHolder from conditions string: {expressionString}");
            return new NCalcHolder { ExpressionString = expressionString };
        }
    }

    #endregion
}