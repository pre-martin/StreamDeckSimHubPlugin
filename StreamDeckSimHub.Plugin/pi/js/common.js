/**
 * Iterates over all elements in the array "settingIds", retrieves the values of their corresponding HTML elements, and stores
 * them in a payload structure. This structure can be saved by the Property Inspector.
 */
const buildSettings = (settingIds) => {
    let settings = {};
    for (const id of settingIds) {
        const element = document.getElementById(id);
        if (!element) {
            console.log('buildSettings: Could not find element ' + id + ' on page');
            $PI.logMessage('buildSettings: Could not find element ' + id + ' on page');
            continue;
        }
        if (element.getAttribute('type') === 'checkbox') {
            settings[id] = element.checked;
        } else {
            settings[id] = element.value;
        }
    }

    return settings;
}

/**
 * Iterates over the structure "settings". The keys are used to find corresponding HTML elements, the value is then set
 * as value of the HTML element.
 */
const restoreSettings = (settings) => {
    for (const id in settings) {
        try {
            const element = document.getElementById(id);
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