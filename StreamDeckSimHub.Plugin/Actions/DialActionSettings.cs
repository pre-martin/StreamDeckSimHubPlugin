// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.Actions;

public class DialActionSettings : HotkeySettings
{
    #region CCW
    public string HotkeyLeft { get; init; } = string.Empty;
    public bool CtrlLeft { get; init; }
    public bool AltLeft { get; init; }
    public bool ShiftLeft { get; init; }
    public string SimHubControlLeft { get; init; } = string.Empty;
    public string SimHubRoleLeft { get; init; } = string.Empty;
    #endregion

    #region CW
    public string HotkeyRight { get; init;  } = string.Empty;
    public bool CtrlRight { get; init; }
    public bool AltRight { get; init; }
    public bool ShiftRight { get; init; }
    public string SimHubControlRight { get; init; } = string.Empty;
    public string SimHubRoleRight { get; init; } = string.Empty;
    #endregion

    #region TouchTap
    public string HotkeyTouchTap { get; init;  } = string.Empty;
    public bool CtrlTouchTap { get; init; }
    public bool AltTouchTap { get; init; }
    public bool ShiftTouchTap { get; init; }
    public string SimHubControlTouchTap { get; init; } = string.Empty;
    public string SimHubRoleTouchTap { get; init; } = string.Empty;
    #endregion

    #region State
    public string SimHubProperty { get; init; } = string.Empty;
    #endregion

    #region Display
    public string DisplaySimHubProperty { get; init; } = string.Empty;
    public string DisplayFormat { get; init; } = string.Empty;
    #endregion

    public override string ToString()
    {
        return $"(Press: {base.ToString()}), " +
               $"(TouchTap: {HotkeyString(HotkeyTouchTap, CtrlTouchTap, AltTouchTap, ShiftTouchTap, SimHubControlTouchTap, SimHubRoleTouchTap)}), " +
               $"(CCW: {HotkeyString(HotkeyLeft, CtrlLeft, AltLeft, ShiftLeft, SimHubControlLeft, SimHubRoleLeft)}), " +
               $"(CW: {HotkeyString(HotkeyRight, CtrlRight, AltRight, ShiftRight, SimHubControlRight, SimHubRoleRight)}), " +
               $"(State: SimHubProperty: {SimHubProperty}), " +
               $"(Display: DisplaySimHubProperty: {DisplaySimHubProperty}, DisplayFormat: {DisplayFormat.Replace("\n", "<CR>")})";
    }

}
