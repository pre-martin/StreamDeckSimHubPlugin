// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using System.Collections.Concurrent;
using NLog;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Tools;

/// <summary>
/// Manages the state of SimHub input triggers and control mapper roles. By using the method <see cref="Deactivate"/> the class
/// ensures that all active triggers and roles are released.
/// </summary>
public class SimHubManager(ISimHubConnection simHubConnection)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string Placeholder = "----";
    private readonly ConcurrentDictionary<string, byte> _activeTriggers = new();
    private readonly ConcurrentDictionary<(string owner, string role), byte> _activeRoles = new();

    public async Task Deactivate()
    {
        var activeTriggers = new List<string>(_activeTriggers.Keys);
        _activeTriggers.Clear();

        var activeRoles = new List<(string owner, string role)>(_activeRoles.Keys);
        _activeRoles.Clear();

        // Send "release" for all active triggers.
        foreach (var trigger in activeTriggers)
        {
            Logger.Warn($"SimHub trigger \"{trigger}\" still active. Sending \"released\" command");
            await TriggerInputReleased(trigger);
        }

        // Send "release" for all active roles.
        foreach (var activeRole in activeRoles)
        {
            Logger.Warn($"SimHub role \"{activeRole.role}\" still active. Sending \"released\" command");
            await RoleReleased(activeRole.owner, activeRole.role);
        }
    }

    public async Task TriggerInputPressed(string? simHubControl)
    {
        if (!string.IsNullOrWhiteSpace(simHubControl))
        {
            _activeTriggers[simHubControl] = 1;
            await simHubConnection.SendTriggerInputPressed(simHubControl);
        }
    }

    public async Task TriggerInputReleased(string? simHubControl)
    {
        if (!string.IsNullOrWhiteSpace(simHubControl))
        {
            _activeTriggers.Remove(simHubControl, out _);
            await simHubConnection.SendTriggerInputReleased(simHubControl);
        }
    }

    public async Task RolePressed(string ownerUuid, string? roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName) && roleName != Placeholder)
        {
            _activeRoles[(ownerUuid, roleName)] = 1;
            await simHubConnection.SendControlMapperRole(ownerUuid, roleName, true);
        }
    }

    public async Task RoleReleased(string ownerUuid, string? roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName) && roleName != Placeholder)
        {
            _activeRoles.Remove((ownerUuid, roleName), out _);
            await simHubConnection.SendControlMapperRole(ownerUuid, roleName, false);
        }
    }
}