
function connectElgatoStreamDeckSocket(inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo) {
    connect(inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo)
}

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
            log('loadSettings failed for id ' + id + ': ' + err);
        }
    }
}

const saveSettingsDelayed = debounce(() => saveSettings(), 500);

function saveSettings() {
    const settingIds = ['hotkey', 'ctrl', 'alt', 'shift', 'simhubControl', 'simhubProperty'];

    let payload = {};
    for (const id of settingIds) {
        const element = document.getElementById(id);
        if (element.getAttribute('type') === 'checkbox') {
            payload[id] = element.checked;
        } else {
            payload[id] = element.value;
        }
    }

    if (websocket) {
        const json = {
            'event': 'setSettings',
            'context': uuid,
            'payload': payload
        };
        log('setSettings with payload:', payload);
        websocket.send(JSON.stringify(json));
    }
}
