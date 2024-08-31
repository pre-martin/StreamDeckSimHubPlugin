$PI.onConnected(async jsn => {
    loadSettings(jsn.actionInfo.payload.settings);

    // Event handler that handles the events from our child window (sib.html).
    window.addEventListener('message', (event) => {
        // We do not check the origin, because this data is not confidential and we are in a trusted environment.
        console.log('Received message (from child window):', event.data);
        if (event.data.message === 'shakeItSelected') {
            $ShakeIt.shakeItSelected(event.data.sourceId, event.data.prefix, event.data.itemId, event.data.itemName, event.data.property);
        }
    });


    // Handler for events that are sent from the plugin to this Property Inspector.
    $PI.onSendToPropertyInspector(jsn.actionInfo.action, jsn => {
        if (jsn.payload?.message === 'shakeItBassStructure') {
            $ShakeIt.showShakeItStructure('sib', jsn.payload.profiles, jsn.payload.sourceId);
        }
        else if (jsn.payload?.message === 'shakeItMotorsStructure') {
            $ShakeIt.showShakeItStructure('sim', jsn.payload.profiles, jsn.payload.sourceId);
        } else {
            console.log('Received unknown message from plugin', jsn);
            $PI.logMessage('Received unknown message from plugin');
        }
    });

    const roleList = await $SimHubRole.fetchSimHubRoles();
    $SimHubRole.updateSimHubRoles('simhubRole', roleList, jsn.actionInfo.payload.settings['simhubRole']);
    $SimHubRole.updateSimHubRoles('simhubRoleTouchTap', roleList, jsn.actionInfo.payload.settings['simhubRoleTouchTap']);
    $SimHubRole.updateSimHubRoles('simhubRoleLeft', roleList, jsn.actionInfo.payload.settings['simhubRoleLeft']);
    $SimHubRole.updateSimHubRoles('simhubRoleRight', roleList, jsn.actionInfo.payload.settings['simhubRoleRight']);
});

function loadSettings(settings) {
    restoreSettings(settings);

    $ShakeIt.resolvePropertyNameFromCache(document.getElementById('simhubProperty'));
    $ShakeIt.resolvePropertyNameFromCache(document.getElementById('displaySimhubProperty'));
}

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

function saveSettings() {
    const settingIds = [
        'hotkeyLeft', 'ctrlLeft', 'altLeft', 'shiftLeft', 'simhubControlLeft', 'simhubRoleLeft',
        'hotkeyRight', 'ctrlRight', 'altRight', 'shiftRight', 'simhubControlRight', 'simhubRoleRight',
        'hotkey', 'ctrl', 'alt', 'shift', 'simhubControl', 'simhubRole',
        'hotkeyTouchTap', 'ctrlTouchTap', 'altTouchTap', 'shiftTouchTap', 'simhubControlTouchTap', 'simhubRoleTouchTap',
        'simhubProperty', 'simhubPropertyClearNameCache',
        'displaySimhubProperty', 'displaySimhubPropertyClearNameCache', 'displayFormat'
    ];

    const payload = buildSettings(settingIds);
    $PI.setSettings(payload);
}

/**
 * Should be called, when a field value changes, which could contain a "ShakeIt" Guid.
 */
const propertyFieldChanged = (source) => {
    $ShakeIt.resolvePropertyNameFromCache(source);
    saveSettingsDelayed();
}

const fetchShakeItBassStructure = (sourceId) => {
    $ShakeIt.fetchBassStructure(sourceId, $PI);
}

const fetchShakeItMotorsStructure = (sourceId) => {
    $ShakeIt.fetchMotorsStructure(sourceId, $PI);
}
