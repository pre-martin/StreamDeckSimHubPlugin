let localSettings = {
    noFlag: '@flags/flag-none.svg',
    noFlagFlash: false,
    noFlagFlashOn: '',
    noFlagFlashOff: '',
    blackFlag: '@flags/flag-black.svg',
    blackFlagFlash: false,
    blackFlagFlashOn: '',
    blackFlagFlashOff: '',
    blueFlag: '@flags/flag-blue.svg',
    blueFlagFlash: false,
    blueFlagFlashOn: '',
    blueFlagFlashOff: '',
    checkeredFlag: '@flags/flag-checkered.svg',
    checkeredFlagFlash: false,
    checkeredFlagFlashOn: '',
    checkeredFlagFlashOff: '',
    greenFlag: '@flags/flag-green.svg',
    greenFlagFlash: false,
    greenFlagFlashOn: '',
    greenFlagFlashOff: '',
    orangeFlag: '@flags/flag-orange.svg',
    orangeFlagFlash: false,
    orangeFlagFlashOn: '',
    orangeFlagFlashOff: '',
    whiteFlag: '@flags/flag-white.svg',
    whiteFlagFlash: false,
    whiteFlagFlashOn: '',
    whiteFlagFlashOff: '',
    yellowFlag: '@flags/flag-yellow.svg',
    yellowFlagFlash: false,
    yellowFlagFlashOn: '',
    yellowFlagFlashOff: '',
    yellowSec1: '@flags/flag-yellow-s1.png',
    yellowSec2: '@flags/flag-yellow-s2.png',
    yellowSec3: '@flags/flag-yellow-s3.png',
    yellowSecFlash: false,
    yellowSecFlashOn: '',
    yellowSecFlashOff: '',
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
    restoreSettings(settings);
    // As we do not yet have the list of available images, we cannot write the selected images into the DOM - they
    // are not yet in the DOM tree. Therefore, we store the settings locally.
    Object.keys(localSettings).forEach((key) => {
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
    fillImageSelectBox('yellowSec1', images);
    fillImageSelectBox('yellowSec2', images);
    fillImageSelectBox('yellowSec3', images);
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
    const previewElement = document.getElementById(id + 'Preview');
    if (previewElement) {
        previewElement.src = '../images/icons/undefined.svg';
    }
}

const updatePreviewImage = (id, selected) => {
    const previewElement = document.getElementById(id + 'Preview');
    if (previewElement) {
        previewElement.src = '../images/custom/' + selected;
    }
}
