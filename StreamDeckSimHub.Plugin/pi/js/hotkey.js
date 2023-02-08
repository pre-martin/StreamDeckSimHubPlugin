

$PI.onConnected(jsn => {
    loadSettings(jsn.actionInfo.payload.settings);
    $PI.onSendToPropertyInspector(jsn.actionInfo.action, jsn => {
        if (jsn.payload?.message === 'shakeItBassStructure') {
            console.log('Structure');
            console.log(jsn.payload.profiles);
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
            if (element.getAttribute('type') === 'checkbox') {
                element.checked = settings[id];
            } else {
                element.value = settings[id];
            }
        } catch (err) {
            $PI.logMessage('loadSettings failed for id ' + id + ': ' + err);
        }
    }
}

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

function saveSettings() {
    const settingIds = ['hotkey', 'ctrl', 'alt', 'shift', 'simhubControl', 'simhubProperty', 'titleSimhubProperty', 'titleFormat'];

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

function fetchShakeItBassStructure() {
    $PI.sendToPlugin({ Event: 'fetchShakeItBassStructure' });
}
