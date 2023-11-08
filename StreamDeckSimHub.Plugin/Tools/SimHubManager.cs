// Copyright (C) 2023 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using NLog;
using StreamDeckSimHub.Plugin.SimHub;

namespace StreamDeckSimHub.Plugin.Tools;

/// <summary>
/// Manages the state of SimHub input triggers and control mapper roles.
/// </summary>
public class SimHubManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ISimHubConnection _simHubConnection;
    private const string Placeholder = "----";
    private string? _activeTrigger;
    private (string owner, string role)? _activeRole;

    public SimHubManager(ISimHubConnection simHubConnection)
    {
        _simHubConnection = simHubConnection;
    }

    public async Task Deactivate()
    {
        if (_activeTrigger != null)
        {
            Logger.Warn($"SimHub trigger \"{_activeTrigger}\" still active. Sending \"released\" command");
            await TriggerInputReleased(_activeTrigger);
        }
        if (_activeRole != null)
        {
            Logger.Warn($"SimHub role \"{_activeRole.Value.role}\" still active. Sending \"released\" command");
            await RoleReleased(_activeRole.Value.owner, _activeRole.Value.role);
        }
    }

    public async Task TriggerInputPressed(string? simHubControl)
    {
        if (!string.IsNullOrWhiteSpace(simHubControl))
        {
            _activeTrigger = simHubControl;
            await _simHubConnection.SendTriggerInputPressed(simHubControl);
        }
    }

    public async Task TriggerInputReleased(string? simHubControl)
    {
        if (!string.IsNullOrWhiteSpace(simHubControl))
        {
            _activeTrigger = null;
            await _simHubConnection.SendTriggerInputReleased(simHubControl);
        }
    }

    public async Task RolePressed(string ownerUuid, string? roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName) && roleName != Placeholder)
        {
            _activeRole = (ownerUuid, roleName);
            await _simHubConnection.SendControlMapperRole(ownerUuid, roleName, true);
        }
    }

    public async Task RoleReleased(string ownerUuid, string? roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName) && roleName != Placeholder)
        {
            _activeRole = null;
            await _simHubConnection.SendControlMapperRole(ownerUuid, roleName, false);
        }
    }
}
