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
    toggleLongKeypress(settings['hasLongKeypress']);
    if (!('longKeypressShortHoldTime' in settings) || !settings['longKeypressShortHoldTime']) {
        settings['longKeypressShortHoldTime'] = 50;
    }
    if (!('longKeypressTimeSpan' in settings) || !settings['longKeypressTimeSpan']) {
        settings['longKeypressTimeSpan'] = 500;
    }

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

    $ShakeIt.resolvePropertyNameFromCache(document.getElementById('simhubProperty'));
    $ShakeIt.resolvePropertyNameFromCache(document.getElementById('titleSimhubProperty'));
}

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

function saveSettings() {
    const settingIds = [
        'hotkey', 'ctrl', 'alt', 'shift',
        'simhubControl',
        'simhubProperty', 'simhubPropertyClearNameCache',
        'hasLongKeypress', 'longHotkey', 'longCtrl', 'longAlt', 'longShift', 'longKeypressShortHoldTime', 'longKeypressTimeSpan',
        'titleSimhubProperty', 'titleSimhubPropertyClearNameCache', 'titleFormat'
    ];

    let payload = {};
    for (const id of settingIds) {
        const element = document.getElementById(id);
        if (!element) {
            console.log('Save: Could not find element ' + id + ' on page!');
            $PI.logMessage('Save: Could not find element ' + id + ' on page');
            continue;
        }
        if (element.getAttribute('type') === 'checkbox') {
            payload[id] = element.checked;
        } else {
            payload[id] = element.value;
        }
    }

    // Adjust flat object so that it fits to the server side JSON object. This also means that these settings will be saved
    // twice, but that is not a problem.
    payload['longKeypressSettings'] = {};
    payload['longKeypressSettings']['hotkey'] = payload['longHotkey'];
    payload['longKeypressSettings']['ctrl'] = payload['longCtrl'];
    payload['longKeypressSettings']['alt'] = payload['longAlt'];
    payload['longKeypressSettings']['shift'] = payload['longShift'];

    $PI.setSettings(payload);
}

/**
 * Shows or hides the area of "Long Keypress" depending on the parameter "show".
 */
const toggleLongKeypress = (show) => {
    const divElement = document.getElementById('longKeypressDiv')
    if (divElement) {
        if (show) {
            divElement.classList.remove('hidden');
        }
        else {
            divElement.classList.add('hidden');
        }
    }
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
