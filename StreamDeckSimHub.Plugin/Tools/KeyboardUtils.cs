// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Runtime.InteropServices;

namespace StreamDeckSimHub.Plugin.Tools;

public record KeyCode
{
    public Keyboard.VirtualKeyShort Vks;
    public Keyboard.ScanCodeShort Scs;
}

public record Hotkey
{
    public bool Ctrl;
    public bool Alt;
    public bool Shift;
    public KeyCode? KeyCode;
}

public interface IKeyboardUtils
{
    void KeyDown(Hotkey? hotkey);
    void KeyUp(Hotkey? hotkey);
}

public class KeyboardUtils : IKeyboardUtils
{
    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, MapType uMapType);

    private enum MapType : uint
    {
        MAPVK_VK_TO_VSC = 0,
        MAPVK_VSC_TO_VK = 1,
        MAPVK_VK_TO_CHAR = 2,
        MAPVK_VSC_TO_VK_EX = 3,
        MAPVK_VK_TO_VSC_EX = 4
    }


    /// <summary>
    /// Tries to find a given key in the enum <c>VirtualKeyShort</c>.
    /// </summary>
    private static Keyboard.VirtualKeyShort? FindVirtualKey(string key)
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

    public static Hotkey? CreateHotkey(bool ctrl, bool alt, bool shift, string key)
    {
        if (key == string.Empty) return null;

        var hotkey = new Hotkey { Ctrl = ctrl, Alt = alt, Shift = shift };

        var virtualKeyShort = FindVirtualKey(key);
        if (virtualKeyShort != null)
        {
            var scanCodeShort = MapVirtualKey((uint)virtualKeyShort, MapType.MAPVK_VK_TO_VSC);
            if (scanCodeShort != 0)
            {
                hotkey.KeyCode = new KeyCode { Vks = virtualKeyShort.Value, Scs = (Keyboard.ScanCodeShort)scanCodeShort };
            }

            return hotkey;
        }

        return null;
    }

    public void KeyDown(Hotkey? hotkey)
    {
        if (hotkey == null) return;
        if (hotkey.Ctrl) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (hotkey.Alt) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (hotkey.Shift) Keyboard.KeyDown(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
        if (hotkey.KeyCode != null) Keyboard.KeyDown(hotkey.KeyCode.Vks, hotkey.KeyCode.Scs);
    }

    public void KeyUp(Hotkey? hotkey)
    {
        if (hotkey == null) return;
        if (hotkey.KeyCode != null) Keyboard.KeyUp(hotkey.KeyCode.Vks, hotkey.KeyCode.Scs);
        if (hotkey.Ctrl) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LCONTROL, Keyboard.ScanCodeShort.LCONTROL);
        if (hotkey.Alt) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LMENU, Keyboard.ScanCodeShort.LMENU);
        if (hotkey.Shift) Keyboard.KeyUp(Keyboard.VirtualKeyShort.LSHIFT, Keyboard.ScanCodeShort.LSHIFT);
    }
}