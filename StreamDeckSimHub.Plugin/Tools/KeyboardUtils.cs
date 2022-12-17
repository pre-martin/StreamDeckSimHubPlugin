// Copyright (C) 2022 Martin Renner
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

    /// <summary>
    /// Tries to find a given key in the enum <c>VirtualKeyShort</c>.
    /// </summary>
    internal static Keyboard.VirtualKeyShort? FindVirtualKey(string key)
    {
        // Try direct match
        var result = Enum.TryParse(key, true, out Keyboard.VirtualKeyShort vks);
        // Try "KEY_"
        if (!result) result = Enum.TryParse($"KEY_{key}", true, out vks);
        return result ? vks : null;
    }
}
