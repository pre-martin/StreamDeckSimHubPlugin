class ShakeIt {
    /**
     * This function assumes, that "source" is an input field with correlating "ClearName" and "ClearNameCache" elements.
     *
     * If the cache contains a hint from "cryptic property name -> clear name", and the input field contains the "cryptic property name",
     * the "clear name" will be inserted into the "ClearName" element.
     *
     * @param source The input field with the SimHub property.
     */
    resolvePropertyNameFromCache = (source) => {
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
     * @param pi Instance of ELGSDPropertyInspector
     */
    fetchBassStructure = (sourceId, pi) => {
        pi.sendToPlugin({Event: 'fetchShakeItBassStructure', parameters: {args: {SourceId: sourceId}}});
    }

    /**
     * Request the plugin to load the ShakeIt Motors structure (and show it afterwards, which is triggered via an event from the plugin).
     * We send "sourceId" all the way down, so that we know at the end, which UI element triggered the request.
     * @param sourceId The source element, which shall be updated, when an element was selected later on.
     * @param pi Instance of ELGSDPropertyInspector
     */
    fetchMotorsStructure = (sourceId, pi) => {
        pi.sendToPlugin({Event: 'fetchShakeItMotorsStructure', parameters: {args: {SourceId: sourceId}}});
    }

    sibPropRegex = /sib\.([a-f0-9\-]+)\.([a-z]+)/i
    simPropRegex = /sim\.([a-f0-9\-]+)\.([a-z]+)/i

    /**
     * Shows the given ShakeIt Bass or ShakeIt Motors structure in a new window. 'prefix' is either 'sib' or 'sim'.
     */
    showShakeItStructure = (prefix, profiles, sourceId) => {
        console.log('Showing ShakeIt structure (prefix: ' + prefix + ') for element ' + sourceId + ':');
        console.log(profiles);

        // If there is already a window open: close it.
        if (window.sib) {
            window.sib.close();
        }

        // If there is already a ShakeIt property in the field, we will read its Guid. This will mark the element as "selected" in
        // the SIB browser. All parent elements of the "selected" element are expanded.
        let guid = null;
        let propertyName = null;
        const inputElement = document.getElementById(sourceId);
        if (inputElement) {
            const prefixLength = `${prefix}.`.length;

            const matchSib = inputElement.value.match(this.sibPropRegex);
            const matchSim = inputElement.value.match(this.simPropRegex);
            if (prefix === 'sib' && matchSib) {
                guid = matchSib[1];
                propertyName = matchSib[2];
            }
            if (prefix === 'sim' && matchSim) {
                guid = matchSim[1];
                propertyName = matchSim[2];
            }
        }
        if (guid != null && propertyName != null) {
            // Create three new fields for each element:
            // - selected    : true, if the Guid of the element is equal to the Guid in the input field
            // - selectedName: If "selected", the name of the selected property.
            // - expanded    : true, if a child is "selected", the current element has to be "expanded". If the child is "expanded", the current too.
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
                element.selectedName = element.selected ? propertyName.toLowerCase() : '';
                element.expanded = anyChildSelected;
            };
            profiles.forEach(profile => loop(profile));
        }

        window.sib = window.open('components/sib.html', 'SIB');
        window.sib.profiles = profiles;
        window.sib.sourceId = sourceId;
        window.sib.prefix = prefix;
    }

    /**
     * A ShakeIt Bass element was selected. Insert it into the field specified by "sourceId". If there is already a "sib" property
     * in this field (e.g. used with an expression), we just replace the property and keep the expression.
     */
    shakeItSelected = (sourceId, prefix, itemId, itemName, property) => {
        const inputElement = document.getElementById(sourceId);
        if (!inputElement) return;

        // Put this data into the "clear name" cache
        const clearNameCacheElement = document.getElementById(sourceId + 'ClearNameCache');
        if (clearNameCacheElement) {
            clearNameCacheElement.value = `${prefix}.${itemId}=${itemName}.${property}`;
        }

        // Fill/replace the property name in the input field
        const newProp = `${prefix}.${itemId}.${property}`;
        if (prefix === 'sib' && this.sibPropRegex.test(inputElement.value)) {
            inputElement.value = inputElement.value.replace(this.sibPropRegex, newProp);
        } else if (prefix === 'sim' && this.simPropRegex.test(inputElement.value)) {
            inputElement.value = inputElement.value.replace(this.simPropRegex, newProp);
        } else {
            inputElement.value = newProp;
        }

        // Trigger "input" event. This will also save the data in the clear name cache.
        inputElement.dispatchEvent(new Event('input'));
    }
}

const $ShakeIt = new ShakeIt();

