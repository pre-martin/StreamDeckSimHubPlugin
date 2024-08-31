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
        } else if (jsn.payload?.message === 'shakeItMotorsStructure') {
            $ShakeIt.showShakeItStructure('sim', jsn.payload.profiles, jsn.payload.sourceId);
        } else {
            console.log('Received unknown message from plugin', jsn);
            $PI.logMessage('Received unknown message from plugin');
        }
    });

    const roleList = await $SimHubRole.fetchSimHubRoles();
    $SimHubRole.updateSimHubRoles('simhubRole', roleList, jsn.actionInfo.payload.settings['simhubRole']);
});

function loadSettings(settings) {
    toggleLongKeypress(settings['hasLongKeypress']);
    if (!('longKeypressShortHoldTime' in settings) || !settings['longKeypressShortHoldTime']) {
        settings['longKeypressShortHoldTime'] = 50;
    }
    if (!('longKeypressTimeSpan' in settings) || !settings['longKeypressTimeSpan']) {
        settings['longKeypressTimeSpan'] = 500;
    }

    restoreSettings(settings);
    $ShakeIt.resolvePropertyNameFromCache(document.getElementById('simhubProperty'));
    $ShakeIt.resolvePropertyNameFromCache(document.getElementById('titleSimhubProperty'));
}

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

function saveSettings() {
    const settingIds = [
        'hotkey', 'ctrl', 'alt', 'shift',
        'simhubControl', 'simhubRole',
        'simhubProperty', 'simhubPropertyClearNameCache',
        'hasLongKeypress', 'longHotkey', 'longCtrl', 'longAlt', 'longShift', 'longKeypressShortHoldTime', 'longKeypressTimeSpan',
        'titleSimhubProperty', 'titleSimhubPropertyClearNameCache', 'titleFormat'
    ];

    let payload = buildSettings(settingIds);
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
