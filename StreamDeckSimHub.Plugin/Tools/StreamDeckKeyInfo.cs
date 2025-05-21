// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using SharpDeck.Enums;
using SharpDeck.Events.Received;

namespace StreamDeckSimHub.Plugin.Tools;

/// <summary>
/// Holds information about a specific Stream Deck Key or Dial.
/// </summary>
public class StreamDeckKeyInfo(DeviceType deviceType, bool isDial, SixLabors.ImageSharp.Size keySize, bool isHighRes)
{
    public DeviceType DeviceType { get; set; } = deviceType;
    public bool IsDial { get; set; } = isDial;
    public SixLabors.ImageSharp.Size KeySize { get; set; } = keySize;
    public bool IsHighRes { get; set; } = isHighRes;
}

/// <summary>
/// Creates instances of <c>StreamDeckKeyInfo</c>.
/// </summary>
public abstract class StreamDeckKeyInfoBuilder
{
    private static readonly IdentifiableDeviceInfo DefaultDevice = new()
    {
        Id = "dummy",
        Name = "Stream Deck",
        Size = new Size { Columns = 5, Rows = 3 },
        Type = DeviceType.StreamDeck,
    };

    /// <summary>
    /// Default key info for a Stream Deck MK2. Can be used to initialize variables.
    /// </summary>
    public static readonly StreamDeckKeyInfo DefaultKeyInfo = Build(DefaultDevice, Controller.Keypad);

    /// <summary>
    /// Creates instances of <c>StreamDeckKeyInfo</c>.
    /// </summary>
    /// <param name="registrationInfo">Usually passed as command line parameter to the plugin.</param>
    /// <param name="deviceId">Passed through the websocket to the plugin, e.g. with OnWillAppear</param>
    /// <param name="controller">Passed through the websocket to the plugin, e.g. with OnWillAppear</param>
    /// <returns></returns>
    public static StreamDeckKeyInfo Build(RegistrationInfo registrationInfo, string deviceId, Controller controller)
    {
        // Actually, it should not happen that we cannot find the ID.
        var deviceInfo = registrationInfo.Devices.FirstOrDefault(deviceInfo => deviceInfo.Id == deviceId) ?? DefaultDevice;

        return Build(deviceInfo, controller);
    }

    private static StreamDeckKeyInfo Build(IdentifiableDeviceInfo deviceInfo, Controller controller)
    {
        var isDial = controller == Controller.Encoder;
        SixLabors.ImageSharp.Size keySize;
        var isHighRes = false;
        if (isDial)
        {
            // SD+ Slot (https://docs.elgato.com/sdk/plugins/layouts-sd+)
            keySize = new(200, 100);
            isHighRes = true;
        }
        else
        {
            if (deviceInfo.Type == DeviceType.StreamDeckXL || deviceInfo.Type == DeviceType.StreamDeckPlus)
            {
                // SD XL or SD+ (https://docs.elgato.com/sdk/plugins/style-guide#sizes)
                keySize = new(144, 144);
                isHighRes = true;
            }
            else
            {
                // SD or SD Mini (https://docs.elgato.com/sdk/plugins/style-guide#sizes)
                keySize = new(72, 72);
            }
        }

        return new StreamDeckKeyInfo(deviceInfo.Type, isDial, keySize, isHighRes);
    }
}
