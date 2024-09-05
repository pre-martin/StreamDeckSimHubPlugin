let localSettings = {
    noFlag: 'flags/flag-none.svg',
    blackFlag: 'flags/flag-black.svg',
    blueFlag: 'flags/flag-blue.svg',
    checkeredFlag: 'flags/flag-checkered.svg',
    greenFlag: 'flags/flag-green.svg',
    orangeFlag: 'flags/flag-orange.svg',
    whiteFlag: 'flags/flag-white.svg',
    yellowFlag: 'flags/flag-yellow.svg',
}

$PI.onConnected(async jsn => {
    loadSettings(jsn.actionInfo.payload.settings);

    $PI.onSendToPropertyInspector(jsn.actionInfo.action, jsn => {
        if (jsn.payload?.message === 'customImages') {
            fillImageSelectBoxes(jsn.payload.images);
        }
    });
});

const loadSettings = (settings) => {
    // As we do not yet have the list of available images, we cannot write the settings directly into the DOM - the selected
    // image is not yet in the DOM tree. Therefore, we store the settings locally.
    Object.keys(settings).forEach((key) => {
        if (key in settings) {
            localSettings[key] = settings[key];
        }
    });
}

const saveSettings = () => {
    domToLocalSettings(localSettings);
    $PI.setSettings(localSettings);

    Object.keys(localSettings).forEach(key => {
        updatePreviewImage(key, localSettings[key]);
    });
}

const fillImageSelectBoxes = (images) => {
    console.log('Received images from backend', images);
    fillImageSelectBox('noFlag', images);
    fillImageSelectBox('blackFlag', images);
    fillImageSelectBox('blueFlag', images);
    fillImageSelectBox('checkeredFlag', images);
    fillImageSelectBox('greenFlag', images);
    fillImageSelectBox('orangeFlag', images);
    fillImageSelectBox('whiteFlag', images);
    fillImageSelectBox('yellowFlag', images);
}

const fillImageSelectBox = (id, images) => {
    const selected = localSettings[id];
    const selectElement = document.getElementById(id);
    images.forEach(image => {
        addSelectOption(selectElement, image);
    });

    selectElement.value = selected;
    if (!selectElement.value) {
        // This means that the "selected" image is not in the list "images".
        setErrorPreviewImage(id);
    } else {
        updatePreviewImage(id, selected);
    }
}

const setErrorPreviewImage = (id) => {
    const previewElement = document.getElementById('preview' + id.charAt(0).toUpperCase() + id.substring(1));
    if (previewElement) {
        previewElement.src = '../images/icons/undefined.svg';
    }
}

const updatePreviewImage = (id, selected) => {
    const previewElement = document.getElementById('preview' + id.charAt(0).toUpperCase() + id.substring(1));
    if (previewElement) {
        previewElement.src = '../images/custom/' + selected;
    }
}
