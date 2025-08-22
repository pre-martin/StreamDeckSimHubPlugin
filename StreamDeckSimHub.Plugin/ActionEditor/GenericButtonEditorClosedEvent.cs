// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

namespace StreamDeckSimHub.Plugin.ActionEditor;

/// <summary>
/// Event to indicate that a editor was just closed.
/// </summary>
public class GenericButtonEditorClosedEvent(string actionUuid)
{
    public string ActionUuid { get; } = actionUuid;
}
