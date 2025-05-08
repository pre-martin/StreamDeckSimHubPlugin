// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Runtime.InteropServices;

namespace StreamDeckSimHub.Plugin.Tools;

public static class KeyboardUtils
{
    [DllImport("user32.dll")]
    internal static extern uint MapVirtualKey(uint uCode, MapType uMapType);

    internal enum MapType : uint
    {
        MAPVK_VK_TO_VSC = 0,
        MAPVK_VSC_TO_VK = 1,
        MAPVK_VK_TO_CHAR = 2,
        MAPVK_VSC_TO_VK_EX = 3,
        MAPVK_VK_TO_VSC_EX = 4
    }

    internal record KeyCode
    {
        internal Keyboard.VirtualKeyShort vks;
        internal Keyboard.ScanCodeShort scs;
    }

    internal record Hotkey
    {
        internal bool ctrl;
        internal bool alt;
        internal bool shift;
        internal KeyCode? keyCode;
    }

    /// <summary>
    /// Tries to find a given key in the enum <c>VirtualKeyShort</c>.
    /// </summary>
    internal static Keyboard.VirtualKeyShort? FindVirtualKey(string key)
    {
        var result = false;
        Keyboard.VirtualKeyShort vks = 0;

        // Only try direct match if the key is not a number (like "1" or "2"). Otherwise "Enum.TryParse" will return
        // the nth enum value instead of the enum entry with the give name.
        if (!int.TryParse(key, out _))
        {
            // Try direct match
            result = Enum.TryParse(key, true, out vks);
        }

        // Try "KEY_"
        if (!result) result = Enum.TryParse($"KEY_{key}", true, out vks);
        return result ? vks : null;
    }

    internal static Hotkey? CreateHotkey(bool ctrl, bool alt, bool shift, string key)
    {
        if (key == string.Empty) return null;

        var hotkey = new Hotkey { ctrl = ctrl, alt = alt, shift = shift };

        var virtualKeyShort = FindVirtualKey(key);
        if (virtualKeyShort != null)
        {
            var scanCodeShort = MapVirtualKey((uint)virtualKeyShort, KeyboardUtils.MapType.MAPVK_VK_TO_VSC);
            if (scanCodeShort != 0)
            {
                hotkey.keyCode = new KeyCode { vks = virtualKeyShort.Value, scs = (Keyboard.ScanCodeShort)scanCodeShort };
            }
            return hotkey;
        }

        return null;
    }

    internal static void KeyDown(Hotkey? hotkey)
    {
        if (hotkey == null) return;
        if (hotkey.ctrl) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (hotkey.alt) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (hotkey.shift) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
        if (hotkey.keyCode != null) Keyboard.KeyDown(hotkey.keyCode.vks, hotkey.keyCode.scs);
    }

    internal static void KeyUp(Hotkey? hotkey)
    {
        if (hotkey == null) return;
        if (hotkey.keyCode != null) Keyboard.KeyUp(hotkey.keyCode.vks, hotkey.keyCode.scs);
        if (hotkey.ctrl) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (hotkey.alt) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (hotkey.shift) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
    }
}