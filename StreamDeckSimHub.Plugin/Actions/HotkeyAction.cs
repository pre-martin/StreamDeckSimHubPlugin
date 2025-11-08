// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.PropertyInspectors;
using StreamDeckSimHub.Plugin.PropertyLogic;
using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Arguments sent from the Property Inspector for the event "fetchShakeItBassStructure" and "fetchShakeItMotorsStructure".
/// We send it back in the answer so that the PI knows for which element this answer is.
/// </summary>
public class FetchShakeItStructureArgs
{
    public string SourceId { get; set; } = string.Empty;
}

/// <summary>
/// Extends <c>HotkeyBaseAction</c> with expressions for the state of the Hotkey, ShakeIt features and a title that can be
/// bound to a SimHub property.
/// </summary>
[StreamDeckAction("net.planetrenner.simhub.hotkey")]
public class HotkeyAction : HotkeyBaseAction<HotkeyActionSettings>
{
    private readonly ShakeItStructureFetcher _shakeItStructureFetcher;
    private readonly DisplayManager _displayManager;

    public HotkeyAction(
        ISimHubConnection simHubConnection, PropertyComparer propertyComparer, ShakeItStructureFetcher shakeItStructureFetcher
    ) : base(simHubConnection, propertyComparer, true)
    {
        _shakeItStructureFetcher = shakeItStructureFetcher;
        _displayManager = new DisplayManager(simHubConnection, DisplayChangedFunc);
    }

    private async Task DisplayChangedFunc(IComparable? value, string format)
    {
        Logger.LogDebug("DisplayChangedFunc ({coords}): value {value} with format {format}", Coordinates, value, format);
        await SetTitle(value, format);
    }

    /// <summary>
    /// Method to handle the event "fetchShakeItBassStructure" from the Property Inspector. Fetches the ShakeIt Bass structure
    /// from SimHub and sends the result through the event "shakeItBassStructure" back to the Property Inspector.
    /// </summary>
    [PropertyInspectorMethod("fetchShakeItBassStructure")]
    public async Task FetchShakeItBassStructure(FetchShakeItStructureArgs args)
    {
        try
        {
            var profiles = await _shakeItStructureFetcher.FetchBassStructure();
            await SendToPropertyInspectorAsync(new { message = "shakeItBassStructure", profiles, args.SourceId });
        }
        catch (Exception e)
        {
            Logger.LogError("Exception while fetching ShakeIt Bass structure: {exMessage}", e.Message);
        }
    }

    /// <summary>
    /// Method to handle the event "fetchShakeItMotorsStructure" from the Property Inspector. Fetches the ShakeIt Motors structure
    /// from SimHub and sends the result through the event "shakeItMotorsStructure" back to the Property Inspector.
    /// </summary>
    [PropertyInspectorMethod("fetchShakeItMotorsStructure")]
    public async Task FetchShakeItMotorsStructure(FetchShakeItStructureArgs args)
    {
        try
        {
            var profiles = await _shakeItStructureFetcher.FetchMotorsStructure();
            await SendToPropertyInspectorAsync(new { message = "shakeItMotorsStructure", profiles, args.SourceId });
        }
        catch (Exception e)
        {
            Logger.LogError("Exception while fetching ShakeIt Motors structure: {exMessage}", e.Message);
        }
    }

    protected override async Task SetSettings(HotkeyActionSettings ac, bool forceSubscribe)
    {
        await _displayManager.HandleDisplayProperties(ac.TitleSimHubProperty, ac.TitleFormat, forceSubscribe);

        await base.SetSettings(ac, forceSubscribe);
    }

    protected override async Task Unsubscribe()
    {
        _displayManager.Deactivate();
        await base.Unsubscribe();
    }

    private async Task SetTitle(IComparable? value, string format)
    {
        var displayValue = value ?? string.Empty;
        try
        {
            await SetTitleAsync(string.Format(format, displayValue));
            Logger.LogDebug("SetTitle ({coors}): displayValue {displayValue} with format {format}", Coordinates, displayValue, format);
        }
        catch (FormatException)
        {
            await SetTitleAsync(displayValue.ToString());
            Logger.LogDebug("SetTitle ({coords}): displayValue {displayValue}", Coordinates, displayValue);
        }
    }
}