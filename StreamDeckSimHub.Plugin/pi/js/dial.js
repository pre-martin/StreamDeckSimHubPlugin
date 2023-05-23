$PI.onConnected(jsn => {
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
});

function loadSettings(settings) {
    for (const id in settings) {
        try {
            const element = document.getElementById(id);
            if (!element) continue;

            if (element.getAttribute('type') === 'checkbox') {
                element.checked = settings[id];
            } else {
                element.value = settings[id];
            }
        } catch (err) {
            console.log('loadSettings failed for id ' + id + ': ' + err);
            $PI.logMessage('loadSettings failed for id ' + id + ': ' + err);
        }
    }

    $ShakeIt.resolvePropertyNameFromCache(document.getElementById('displaySimhubProperty'));
}

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

function saveSettings() {
    const settingIds = [
        'hotkeyLeft', 'ctrlLeft', 'altLeft', 'shiftLeft', 'simhubControlLeft',
        'hotkeyRight', 'ctrlRight', 'altRight', 'shiftRight', 'simhubControlRight',
        'hotkey', 'ctrl', 'alt', 'shift', 'simhubControl',
        'displaySimhubProperty', 'displaySimhubPropertyClearNameCache'
    ];

    let payload = {};
    for (const id of settingIds) {
        const element = document.getElementById(id);
        if (element.getAttribute('type') === 'checkbox') {
            payload[id] = element.checked;
        } else {
            payload[id] = element.value;
        }
    }

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
