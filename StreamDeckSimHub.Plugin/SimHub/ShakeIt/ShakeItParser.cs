// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NLog;

namespace StreamDeckSimHub.Plugin.SimHub.ShakeIt;

/// <summary>
/// Parses the ShakeIt Bass and ShakeIt Motors structure from SimHub.
/// </summary>
/// <remarks>The class is stateful.</remarks>
public class ShakeItParser
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Stack<(int depth, EffectsContainerBase effect)> _effectsStack = new();

    /// <summary>
    /// When parsing was done, this list contains all parsed Profiles and their structure.
    /// </summary>
    public IList<Profile> Profiles { get; } = new List<Profile>();

    /// <summary>
    /// Parses one line at a time and tries to reconstruct the structure.
    /// </summary>
    public void ParseLine(string line)
    {
        var trimmedLine = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmedLine)) return;

        var depth = ParseDepth(trimmedLine);
        switch (depth)
        {
            case < 0:
            {
                // Line without parseable depth: Ignore.
                Logger.Warn($"Invalid depth in line: {Sanitize(line)}");
                return;
            }
            case 0:
            {
                // Depth 0 is always a profile, which consists of "depth", "guid", "type" and "name"
                var lineItems = trimmedLine.Split(new[] { ' ' }, 4);
                if (lineItems.Length < 4)
                {
                    Logger.Warn($"Received invalid 'Profile' line, using placeholder profile: {Sanitize(line)}");
                    Profiles.Add(new Profile(Guid.NewGuid().ToString(), "Placeholder"));
                }
                else
                {
                    Profiles.Add(new Profile(lineItems[1], lineItems[3]));
                }

                _effectsStack.Clear();
                break;
            }
            default:
            {
                // Everything else is the structure, which consists of "depth", "guid", "type" and "name"
                var lineItems = trimmedLine.Split(new[] { ' ' }, 4);
                if (lineItems.Length < 4)
                {
                    Logger.Warn($"Received invalid effect group or effect. Ignoring this line: {Sanitize(line)}");
                    return;
                }

                // Pop everything from the stack which has a depth greater than the new one: It cannot be a parent any longer.
                while (_effectsStack.Count > 0 && depth <= _effectsStack.Peek().depth)
                {
                    _effectsStack.Pop();
                }

                var element = EffectOrGroup(lineItems[1], lineItems[2], lineItems[3]);
                if (depth == 1)
                {
                    Profiles[^1].EffectsContainers.Add(element);
                    _effectsStack.Push((depth, element));
                }
                else
                {
                    var parent = _effectsStack.Peek().effect as GroupContainer;
                    if (parent == null)
                    {
                        Logger.Warn($"Invalid structure. Expected a parent group: {Sanitize(line)}");
                        return;
                    }

                    parent.EffectsContainers.Add(element);
                    _effectsStack.Push((depth, element));
                }

                break;
            }
        }
    }

    private int ParseDepth(string line)
    {
        var idxSpace = line.IndexOf(": ", StringComparison.Ordinal);
        if (idxSpace < 0)
        {
            return -1;
        }

        var depthString = line[..idxSpace];
        if (int.TryParse(depthString, out var depth))
        {
            return depth;
        }

        return -1;
    }

    private EffectsContainerBase EffectOrGroup(string id, string type, string name)
    {
        return type switch
        {
            "GroupContainer" => new GroupContainer(id, name),
            "EffectsContainerBase" => new EffectsContainerBase(id, name),
            _ => new EffectsContainerBase(id, name)
        };
    }

    private string? Sanitize(string? s)
    {
        return s?.Replace(Environment.NewLine, "");
    }
}