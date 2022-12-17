// Global web socket for Stream Deck Property Inspector
let websocket = null, uuid = null;

function connectElgatoStreamDeckSocket(inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inPropertyInspectorUUID;

    if (websocket) {
        websocket.close();
        websocket = null;
    }

    websocket = new WebSocket('ws://127.0.0.1:' + inPort);

    websocket.onopen = function () {
        const json = {
            'event': inRegisterEvent,
            'uuid': inPropertyInspectorUUID
        };
        websocket.send(JSON.stringify(json));
    }

    websocket.onmessage = function (inEvent) {
        const data = JSON.parse(inEvent.data);

        log('Ignored websocket message', inEvent);
    }

    const actionInfo = JSON.parse(inActionInfo);
    const settings = actionInfo.payload.settings;
    loadSettings(settings);
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

function debounce(func, timeout = 300) {
    let timer;
    return (...args) => {
        const context = this;
        console.log('Clearing timer', timer);
        clearTimeout(timer);
        timer = setTimeout(() => func.apply(context, args), timeout);
        console.log('Timer: ', timer);
    }
}

const saveSettingsDelayed = debounce(() => saveSettings(), 500);

function saveSettings() {
    const settingIds = ['hotkey', 'ctrl', 'alt', 'shift', 'simhubProperty'];

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
