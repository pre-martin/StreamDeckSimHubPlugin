// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using streamdeck_client_csharp;
using streamdeck_client_csharp.Events;
using StreamDeckSimHub.Tools;

namespace StreamDeckSimHub.Actions;

/// <summary>
/// This action sends a key stroke to the active window and it can update its state from a SimHub property.
/// </summary>
public class HotkeyAction : BaseAction
{
    private class ActionSettings
    {
        [JsonProperty]
        public string Hotkey { get; set; } = string.Empty;

        [JsonProperty]
        public string SimHubProperty { get; set; } = string.Empty;

        [JsonProperty]
        public bool Ctrl { get; set; }

        [JsonProperty]
        public bool Alt { get; set; }

        [JsonProperty]
        public bool Shift { get; set; }

        internal static ActionSettings Default()
        {
            return new ActionSettings();
        }
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly string _context;
    private readonly StreamDeckConnection _streamDeckConnection;
    private readonly SimHubConnection _simHubConnection;

    private ActionSettings _actionSettings;
    private Keyboard.VirtualKeyShort? _vks;
    private Keyboard.ScanCodeShort? _scs;
    private bool _state;

    public HotkeyAction(string context, AppearancePayload eventPayload, StreamDeckConnection streamDeckConnection,
        SimHubConnection simHubConnection)
    {
        _context = context;
        _actionSettings = ActionSettings.Default();
        _streamDeckConnection = streamDeckConnection;
        _simHubConnection = simHubConnection;
        _simHubConnection.PropertyChangedEvent += PropertyChangedEvent;
        SetSettings(FromJson(eventPayload.Settings));
    }

    /// <summary>
    /// Called when the value of a SimHub property has changed.
    /// </summary>
    private async void PropertyChangedEvent(object? sender, SimHubConnection.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _actionSettings.SimHubProperty)
        {
            Logger.Info($"Property {e.PropertyName} changed to '{e.PropertyValue}'");
            // see https://github.com/pre-martin/SimHubPropertyServer/blob/main/Property/SimHubProperty.cs, "TypeToString()"
            switch (e.PropertyType)
            {
                case "boolean":
                    _state = e.PropertyValue == "True";
                    break;
                case "integer":
                case "long":
                    _state = e.PropertyValue != null && long.Parse(e.PropertyValue, CultureInfo.InvariantCulture) > 0;
                    break;
                default:
                    _state = false;
                    break;
            }

            await _streamDeckConnection.SetStateAsync((uint)(_state ? 1 : 0), _context);
        }
    }

    public override void ReceivedSettings(ReceiveSettingsPayload eventPayload)
    {
        SetSettings(FromJson(eventPayload.Settings));
    }

    public override void KeyDown(KeyPayload eventPayload)
    {
        if (_actionSettings.Ctrl) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (_actionSettings.Alt) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (_actionSettings.Shift) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
        if (_vks.HasValue && _scs.HasValue) Keyboard.KeyDown(_vks.Value, _scs.Value);
    }

    public override async void KeyUp(KeyPayload eventPayload)
    {
        if (_vks.HasValue && _scs.HasValue) Keyboard.KeyUp(_vks.Value, _scs.Value);
        if (_actionSettings.Ctrl) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (_actionSettings.Alt) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (_actionSettings.Shift) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
        // Stream Deck always toggle the state for each keypress (at "key up", to be precise). So we have to set the
        // state again to the correct one, after Stream Deck has done its toggling stuff.
        await _streamDeckConnection.SetStateAsync((uint)(_state ? 1 : 0), _context);
    }

    public override void Destroy()
    {
        _simHubConnection.PropertyChangedEvent -= PropertyChangedEvent;
        _simHubConnection.Unsubscribe(_actionSettings.SimHubProperty).Wait();
    }

    private void SetSettings(ActionSettings ac)
    {
        Logger.Info(
            $"Modifiers: Ctrl: {ac.Ctrl}, Alt: {ac.Alt}, Shift: {ac.Shift}, Hotkey: {ac.Hotkey}, SimHubProperty: {ac.SimHubProperty}");

        // Unsubscribe previous SimHub property.
        if (!string.IsNullOrEmpty(_actionSettings.SimHubProperty))
        {
            _simHubConnection.Unsubscribe(_actionSettings.SimHubProperty).Wait();
        }

        this._actionSettings = ac;

        this._vks = null;
        this._scs = null;
        if (!string.IsNullOrEmpty(ac.Hotkey))
        {
            var virtualKeyShort = KeyboardUtils.FindVirtualKey(ac.Hotkey);
            if (virtualKeyShort == null)
            {
                Logger.Error($"Could not find VirtualKeyCode for hotkey '{ac.Hotkey}'");
                return;
            }

            var scanCodeShort =
                KeyboardUtils.MapVirtualKey((uint)virtualKeyShort, KeyboardUtils.MapType.MAPVK_VK_TO_VSC);
            if (scanCodeShort == 0)
            {
                Logger.Error($"Could not find ScanCode for hotkey '{ac.Hotkey}'");
                return;
            }

            this._vks = virtualKeyShort;
            this._scs = (Keyboard.ScanCodeShort)scanCodeShort;
        }

        // Subscribe SimHub property.
        if (!string.IsNullOrEmpty(ac.SimHubProperty))
        {
            _simHubConnection.Subscribe(ac.SimHubProperty).Wait();
        }
    }

    private ActionSettings FromJson(JObject? jsonObject)
    {
        if (jsonObject != null && jsonObject.Count > 0)
        {
            try
            {
                return jsonObject.ToObject<ActionSettings>() ?? ActionSettings.Default();
            }
            catch (Exception e)
            {
                Logger.Warn(e,
                    $"Could not deserialize JSON into ActionSettings: {jsonObject.ToString(Formatting.None)}");
                return ActionSettings.Default();
            }
        }

        return ActionSettings.Default();
    }
}