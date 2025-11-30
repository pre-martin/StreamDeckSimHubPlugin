// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.ObjectModel;
using SixLabors.ImageSharp;

namespace StreamDeckSimHub.Plugin.Actions.GenericButton.Model;

/// <summary>
/// Scales display items by a given factor.
/// </summary>
public class ItemScaler : IDisplayItemVisitor
{
    public async Task ScaleDisplayItems(Collection<DisplayItem> settingsDisplayItems, float scaleFactorX, float scaleFactorY)
    {
        foreach (var displayItem in settingsDisplayItems)
        {
            await displayItem.Accept(this, new ItemScalerArgs(scaleFactorX, scaleFactorY));
        }
    }

    private void ScaleDisplayParameters(DisplayParameters displayParameters, ItemScalerArgs args)
    {
        displayParameters.Position = new Point(
            (int)(args.ScaleFactorX * displayParameters.Position.X),
            (int)(args.ScaleFactorY * displayParameters.Position.Y));

        if (displayParameters.Size != null)
        {
            displayParameters.Size = new Size(
                (int)(args.ScaleFactorX * displayParameters.Size.Value.Width),
                (int)(args.ScaleFactorY * displayParameters.Size.Value.Height));
        }
    }

    #region IDisplayItemVisitor

    public Task Visit(DisplayItemImage displayItem, IVisitorArgs? args)
    {
        var scalerArgs = args as ItemScalerArgs ?? new ItemScalerArgs(1, 1);
        ScaleDisplayParameters(displayItem.DisplayParameters, scalerArgs);
        return Task.CompletedTask;
    }

    public Task Visit(DisplayItemText displayItem, IVisitorArgs? args)
    {
        var scalerArgs = args as ItemScalerArgs ?? new ItemScalerArgs(1, 1);
        ScaleDisplayParameters(displayItem.DisplayParameters, scalerArgs);
        // Font size is automatically scaled during rendering (see ButtonRendererImageSharp#ScaleFont).
        return Task.CompletedTask;
    }

    public Task Visit(DisplayItemValue displayItem, IVisitorArgs? args)
    {
        var scalerArgs = args as ItemScalerArgs ?? new ItemScalerArgs(1, 1);
        ScaleDisplayParameters(displayItem.DisplayParameters, scalerArgs);
        // Font size is automatically scaled during rendering (see ButtonRendererImageSharp#ScaleFont).
        return Task.CompletedTask;
    }

    #endregion
}

internal record ItemScalerArgs(float ScaleFactorX, float ScaleFactorY) : IVisitorArgs;