// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck.Enums;
using SharpDeck.Events.Received;
using ShimSkiaSharp;

namespace StreamDeckSimHub.Plugin.Tools;

/// <summary>
/// Holds information about a specific Stream Deck Key or Dial.
/// </summary>
public class StreamDeckKeyInfo(DeviceType deviceType, bool isDial, (int width, int height) keySize, bool isHighRes)
{
    public DeviceType DeviceType { get; set; } = deviceType;
    public bool IsDial { get; set; } = isDial;
    public (int width, int height) KeySize { get; set; } = keySize;
    public bool IsHighRes { get; set; } = isHighRes;
}

/// <summary>
/// Creates instance of <c>StreamDeckKeyInfo</c>.
/// </summary>
public abstract class StreamDeckKeyInfoBuilder
{
    private static readonly IdentifiableDeviceInfo DefaultDevice = new()
    {
        Id = "dummy", Name = "Stream Deck", Size = new Size() { Columns = 5, Rows = 3 }, Type = DeviceType.StreamDeck,
    };

    public static StreamDeckKeyInfo Build(RegistrationInfo registrationInfo, string deviceId, Controller controller)
    {
        // Actually, it should not happen that we cannot find the ID.
        var deviceInfo = registrationInfo.Devices.FirstOrDefault(deviceInfo => deviceInfo.Id == deviceId) ?? DefaultDevice;

        var isDial = controller == Controller.Encoder;
        (int width, int height) keySize;
        var isHighRes = false;
        if (isDial)
        {
            // SD+ Slot (https://docs.elgato.com/sdk/plugins/layouts-sd+)
            keySize = (200, 100);
            isHighRes = true;
        }
        else
        {
            if (deviceInfo.Type == DeviceType.StreamDeckXL || deviceInfo.Type == DeviceType.StreamDeckPlus)
            {
                // SD XL or SD+ (https://docs.elgato.com/sdk/plugins/style-guide#sizes)
                keySize = (144, 144);
                isHighRes = true;
            }
            else
            {
                // SD or SD Mini (https://docs.elgato.com/sdk/plugins/style-guide#sizes)
                keySize = (72, 72);
            }
        }

        return new StreamDeckKeyInfo(deviceInfo?.Type ?? DefaultDevice.Type, isDial, keySize, isHighRes);
    }
}
