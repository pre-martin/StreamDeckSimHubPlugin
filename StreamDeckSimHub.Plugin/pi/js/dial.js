$PI.onConnected(jsn => {
    loadSettings(jsn.actionInfo.payload.settings);
});

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

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
}

function saveSettings() {
    const settingIds = [
        'hotkeyLeft', 'ctrlLeft', 'altLeft', 'shiftLeft', 'simhubControlLeft',
        'hotkeyRight', 'ctrlRight', 'altRight', 'shiftRight', 'simhubControlRight',
        'hotkey', 'ctrl', 'alt', 'shift', 'simhubControl',
        'displaySimhubProperty'
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
