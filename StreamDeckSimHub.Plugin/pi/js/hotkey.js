$PI.onConnected(jsn => {
    loadSettings(jsn.actionInfo.payload.settings);


    // Event handler that handles the events from our child window (sib.html).
    window.addEventListener('message', (event) => {
        // We do not check the origin, because this data is not confidential and we are in a trusted environment.
        console.log('Received message', event.data);
        if (event.data.message === 'sibSelected') {
            shakeItBassSelected(event.data.sourceId, event.data.itemId, event.data.itemName, event.data.property);
        }
    });


    // Handler for events that are sent from the plugin to this Property Inspector.
    $PI.onSendToPropertyInspector(jsn.actionInfo.action, jsn => {
        if (jsn.payload?.message === 'shakeItBassStructure') {
            showShakeItBassStructure(jsn.payload.profiles, jsn.payload.sourceId);
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

/**
 * Request the plugin to load the ShakeIt Bass structure (and show it afterwards, which is triggered via an event from the plugin).
 * We send "sourceId" all the way down, so that we know at the end, which UI element triggered the request.
 * @param sourceId The source element, which shall be updated, when an element was selected later on.
 */
function fetchShakeItBassStructure(sourceId) {
    $PI.sendToPlugin({Event: 'fetchShakeItBassStructure', SourceId: sourceId});
}

/**
 * Shows the given ShakeIt Bass structure in a new window.
 */
function showShakeItBassStructure(profiles, sourceId) {
    console.log('Showing ShakeIt Bass structure for element ' + sourceId);
    console.log(profiles);

    window.sib = window.open('components/sib.html', 'SIB');
    window.sib.profiles = profiles;
    window.sib.sourceId = sourceId;
}

/**
 * A ShakeIt Bass element was selected. Insert it into the field specified by "sourceId".
 */
function shakeItBassSelected(sourceId, itemId, itemName, property) {
    const element = document.getElementById(sourceId);
    if (!element) return;

    const newProp = `sib.${itemId}.${property}`;
    const regex = /sib.[a-f0-9\-]+\.[a-z]+/i
    if (regex.test(element.value)) {
        element.value = element.value.replace(regex, newProp);
    }
    else {
        element.value = newProp;
    }
    element.dispatchEvent(new Event('input'));
}