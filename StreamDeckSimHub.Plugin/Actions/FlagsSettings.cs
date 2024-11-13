// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SixLabors.ImageSharp;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.Plugin.Actions;

/// <summary>
/// Configuration for a specific flag. Used in the plugin code, but not by the Property Inspector.
/// </summary>
public class FlagData
{
    public string FileName { get; set; } = string.Empty;
    public Image Image { get; set; } = ImageUtils.EmptyImage;
    public bool Flash { get; set; }
    public int FlashOn { get; set; }
    public int FlashOff { get; set; }
}

/// <summary>
/// Configuration for the sector flags. Used in the plugin code, but not by the Property Inspector.
/// </summary>
public class SectorFlagData : FlagData
{
    public string FileName2 { get; set; } = string.Empty;
    public Image Image2 { get; set; } = ImageUtils.EmptyImage;
    public string FileName3 { get; set; } = string.Empty;
    public Image Image3 { get; set; } = ImageUtils.EmptyImage;
}

/// <summary>
/// Configuration for all flags. Used in the plugin code, but not by the Property Inspector.
/// </summary>
public class Flags
{
    public FlagData NoFlag { get; } = new();
    public FlagData BlackFlag { get; } = new();
    public FlagData BlueFlag { get; } = new();
    public FlagData CheckeredFlag { get; } = new();
    public FlagData GreenFlag { get; } = new();
    public FlagData OrangeFlag { get; } = new();
    public FlagData WhiteFlag { get; } = new();
    public FlagData YellowFlag { get; } = new();
    public SectorFlagData SectorFlag { get; } = new();
}

/// <summary>
/// Settings for Flags Action, which are set in the Stream Deck UI with the Property Inspector.
/// </summary>
public class FlagsSettings
{
    public string NoFlag { get; set; } = "@flags/flag-none.svg";
    public bool NoFlagFlash { get; set; } = false;
    public int? NoFlagFlashOn { get; set; } = 0;
    public int? NoFlagFlashOff { get; set; } = 0;

    public string BlackFlag { get; set; } = "@flags/flag-black.svg";
    public bool BlackFlagFlash { get; set; } = false;
    public int? BlackFlagFlashOn { get; set; } = 0;
    public int? BlackFlagFlashOff { get; set; } = 0;

    public string BlueFlag { get; set; } = "@flags/flag-blue.svg";
    public bool BlueFlagFlash { get; set; } = false;
    public int? BlueFlagFlashOn { get; set; } = 0;
    public int? BlueFlagFlashOff { get; set; } = 0;

    public string CheckeredFlag { get; set; } = "@flags/flag-checkered.svg";
    public bool CheckeredFlagFlash { get; set; } = false;
    public int? CheckeredFlagFlashOn { get; set; } = 0;
    public int? CheckeredFlagFlashOff { get; set; } = 0;

    public string GreenFlag { get; set; } = "@flags/flag-green.svg";
    public bool GreenFlagFlash { get; set; } = false;
    public int? GreenFlagFlashOn { get; set; } = 0;
    public int? GreenFlagFlashOff { get; set; } = 0;

    public string OrangeFlag { get; set; } = "@flags/flag-orange.svg";
    public bool OrangeFlagFlash { get; set; } = false;
    public int? OrangeFlagFlashOn { get; set; } = 0;
    public int? OrangeFlagFlashOff { get; set; } = 0;

    public string WhiteFlag { get; set; } = "@flags/flag-white.svg";
    public bool WhiteFlagFlash { get; set; } = false;
    public int? WhiteFlagFlashOn { get; set; } = 0;
    public int? WhiteFlagFlashOff { get; set; } = 0;

    public string YellowFlag { get; set; } = "@flags/flag-yellow.svg";
    public bool YellowFlagFlash { get; set; } = false;
    public int? YellowFlagFlashOn { get; set; } = 0;
    public int? YellowFlagFlashOff { get; set; } = 0;

    public string YellowSec1 { get; set; } = "@flags/flag-yellow-s1.png";
    public string YellowSec2 { get; set; } = "@flags/flag-yellow-s2.png";
    public string YellowSec3 { get; set; } = "@flags/flag-yellow-s3.png";
    public bool YellowSecFlash { get; set; } = false;
    public int? YellowSecFlashOn { get; set; } = 0;
    public int? YellowSecFlashOff { get; set; } = 0;

    public override string ToString()
    {
        return
            $"No: {NoFlag}|{NoFlagFlash}|{NoFlagFlashOn}|{NoFlagFlashOff}, " +
            $"Black: {BlackFlag}|{BlackFlagFlash}|{BlackFlagFlashOn}|{BlackFlagFlashOff}, " +
            $"Blue: {BlueFlag}|{BlueFlagFlash}|{BlueFlagFlashOn}|{BlueFlagFlashOff}, " +
            $"Checkered: {CheckeredFlag}|{CheckeredFlagFlash}|{CheckeredFlagFlashOn}|{CheckeredFlagFlashOff}, " +
            $"Green: {GreenFlag}|{GreenFlagFlash}|{GreenFlagFlashOn}|{GreenFlagFlashOff}, " +
            $"Orange: {OrangeFlag}|{OrangeFlagFlash}|{OrangeFlagFlashOn}|{OrangeFlagFlashOff}, " +
            $"White: {WhiteFlag}|{WhiteFlagFlash}|{WhiteFlagFlashOn}|{WhiteFlagFlashOff}, " +
            $"Yellow: {YellowFlag}|{YellowFlagFlash}|{YellowFlagFlashOn}|{YellowFlagFlashOff}, " +
            $"YellowSec: {YellowSec1}|{YellowSec2}|{YellowSec3}|{YellowSecFlash}|{YellowSecFlashOn}|{YellowSecFlashOff}";
    }
}