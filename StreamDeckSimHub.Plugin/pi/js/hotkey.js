$PI.onConnected(jsn => {
    loadSettings(jsn.actionInfo.payload.settings);


    // Event handler that handles the events from our child window (sib.html).
    window.addEventListener('message', (event) => {
        // We do not check the origin, because this data is not confidential and we are in a trusted environment.
        console.log('Received message (from child window):', event.data);
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

    resolvePropertyNameFromCache(document.getElementById('simhubProperty'));
    resolvePropertyNameFromCache(document.getElementById('titleSimhubProperty'));
}

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

function saveSettings() {
    const settingIds = [
        'hotkey', 'ctrl', 'alt', 'shift',
        'simhubControl',
        'simhubProperty', 'simhubPropertyClearNameCache',
        'titleSimhubProperty', 'titleSimhubPropertyClearNameCache', 'titleFormat'
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
    resolvePropertyNameFromCache(source);
    saveSettingsDelayed();
}

/**
 * This function assumes, that "source" is an input field with correlating "ClearName" and "ClearNameCache" elements.
 *
 * If the cache contains a hint from "cryptic property name -> clear name", and the input field contains the "cryptic property name",
 * the "clear name" will be inserted into the "ClearName" element.
 *
 * @param source The input field with the SimHub property.
 */
const resolvePropertyNameFromCache = (source) => {
    if (!source) return;
    const sourceId = source.id;
    const clearNameElement = document.getElementById(sourceId + 'ClearName');
    const clearNameCacheElement = document.getElementById(sourceId + 'ClearNameCache');
    if (!clearNameElement || !clearNameCacheElement) return;

    clearNameElement.innerText = '';
    if (clearNameCacheElement.value.length > 0) {
        const cacheValue = clearNameCacheElement.value;
        const separator = cacheValue.indexOf('=');
        if (separator > 0 && separator < cacheValue.length + 1) {
            const propName = cacheValue.substring(0, separator);
            const propClearText = cacheValue.substring(separator + 1);
            if (source.value.includes(propName)) {
                clearNameElement.innerText = propClearText;
            }
        }
    }
}

/**
 * Request the plugin to load the ShakeIt Bass structure (and show it afterwards, which is triggered via an event from the plugin).
 * We send "sourceId" all the way down, so that we know at the end, which UI element triggered the request.
 * @param sourceId The source element, which shall be updated, when an element was selected later on.
 */
function fetchShakeItBassStructure(sourceId) {
    $PI.sendToPlugin({Event: 'fetchShakeItBassStructure', SourceId: sourceId});
}

const sibPropRegex = /sib.[a-f0-9\-]+\.[a-z]+/i

/**
 * Shows the given ShakeIt Bass structure in a new window.
 */
function showShakeItBassStructure(profiles, sourceId) {
    console.log('Showing ShakeIt Bass structure for element ' + sourceId + ':');
    console.log(profiles);

    // If there is already a window open: close it.
    if (window.sib) {
        window.sib.close();
    }

    // If there is already a ShakeIt property in the field, we will read its Guid. This will mark the element as "selected" in
    // the SIB browser. All parent elements of the "selected" element are expanded.
    const inputElement = document.getElementById(sourceId);
    if (inputElement && sibPropRegex.test(inputElement.value)) {
        const guid = inputElement.value.substring("sib.".length, "sib.".length + 36);

        // Create two new fields for each element:
        // - selected: If the Guid of the element is equal to the Guid in the input field
        // - expanded: If a child is "selected", the current element has to be "expanded". If the child is "expanded", the current too.
        // This is done with a depth-first recursion.
        const loop = (element) => {
            let anyChildSelected = false;
            if (element.effectsContainers) {
                element.effectsContainers.forEach(child => {
                    loop(child);
                    if (child.selected === true) {
                        element.expanded = true;
                        anyChildSelected = true;
                    }
                    if (child.expanded === true) {
                        anyChildSelected = true;
                    }
                });
            }
            element.selected = element.id === guid;
            element.expanded = anyChildSelected;
        };
        profiles.forEach(profile => loop(profile));
    }

    window.sib = window.open('components/sib.html', 'SIB');
    window.sib.profiles = profiles;
    window.sib.sourceId = sourceId;
}

/**
 * A ShakeIt Bass element was selected. Insert it into the field specified by "sourceId". If there is already a "sib" property
 * in this field (e.g. used with an expression), we just replace the property and keep the expression.
 */
function shakeItBassSelected(sourceId, itemId, itemName, property) {
    const inputElement = document.getElementById(sourceId);
    if (!inputElement) return;

    // Put this data into the "clear name" cache
    const clearNameCacheElement = document.getElementById(sourceId + 'ClearNameCache');
    if (clearNameCacheElement) {
        clearNameCacheElement.value = `sib.${itemId}=${itemName}`;
    }

    // Fill/replace the property name in the input field
    const newProp = `sib.${itemId}.${property}`;
    if (sibPropRegex.test(inputElement.value)) {
        inputElement.value = inputElement.value.replace(sibPropRegex, newProp);
    } else {
        inputElement.value = newProp;
    }

    // Trigger "input" event. This will also save the data in the clear name cache.
    inputElement.dispatchEvent(new Event('input'));
}