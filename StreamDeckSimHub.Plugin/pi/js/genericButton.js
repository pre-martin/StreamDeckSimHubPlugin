// Copyright (C) 2024 Martin Renner
// LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)

$PI.onConnected(async jsn => {
    updateNameField(jsn.actionInfo.payload.settings);
});

$PI.onDidReceiveSettings('net.planetrenner.simhub.generic-button', async jsn => {
    updateNameField(jsn.payload.settings);
});

function updateNameField(settings) {
    if ('name' in settings) {
        document.getElementById('name').value = settings.name;
    }
}

const openEditor = () => {
    $PI.sendToPlugin({Event: 'openEditor'});
}
