// Copyright (C) 2025 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

using StreamDeckSimHub.Plugin.SimHub;
using StreamDeckSimHub.Plugin.Tools;

namespace StreamDeckSimHub.PluginTests.Tools
{
    public class SimHubManagerTests
    {
        [Test]
        public async Task PressRoleAndTrigger_Deactivate_UnsubscribesBoth()
        {
            // Arrange
            var simHubConnectionMock = new Mock<ISimHubConnection>();
            var manager = new SimHubManager(simHubConnectionMock.Object);

            var trigger = "TestTrigger";
            var owner = "OwnerUuid";
            var role = "TestRole";

            // Press the trigger and the role
            await manager.TriggerInputPressed(trigger);
            await manager.RolePressed(owner, role);

            // Simulate that the user switched to another page or another profile before releasing the trigger and role
            await manager.Deactivate();

            // Verify correct calls to connection
            simHubConnectionMock.Verify(x => x.SendTriggerInputPressed(trigger), Times.Once);
            simHubConnectionMock.Verify(x => x.SendTriggerInputReleased(trigger), Times.Once);
            simHubConnectionMock.Verify(x => x.SendControlMapperRole(owner, role, true), Times.Once);
            simHubConnectionMock.Verify(x => x.SendControlMapperRole(owner, role, false), Times.Once);
        }
    }
}
