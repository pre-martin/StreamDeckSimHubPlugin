// Copyright (C) 2022 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using Newtonsoft.Json;
using streamdeck_client_csharp.Events;

namespace StreamDeckSimHub.Tools;

/// <summary>
/// Extension methods for Stream Deck API payload classes.
/// </summary>
internal static class PayloadExtensions
{
    internal static string ToStringEx(this AppearancePayload? ap)
    {
        return ap == null
            ? "null"
            : $"Settings: {ap.Settings.ToString(Formatting.None)}, Coordinates: {ap.Coordinates.Rows},{ap.Coordinates.Columns}, State: {ap.State}, IsInMultiAction: {ap.IsInMultiAction}";
    }

    internal static string ToStringEx(this ReceiveSettingsPayload? rp)
    {
        return rp == null
            ? "null"
            : $"Settings: {rp.Settings.ToString(Formatting.None)}, Coordinates: {rp.Coordinates.Rows},{rp.Coordinates.Columns}, IsInMultiAction: {rp.IsInMultiAction}";
    }

    internal static string ToStringEx(this KeyPayload? kp)
    {
        return kp == null
            ? "null"
            : $"Settings: {kp.Settings.ToString(Formatting.None)}, Coordinates: {kp.Coordinates.Rows},{kp.Coordinates.Columns}, State: {kp.State}, UserDesiredState: {kp.UserDesiredState}, IsInMultiAction: {kp.IsInMultiAction}";
    }

}