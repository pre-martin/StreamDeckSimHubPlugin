

$PI.onConnected(jsn => {
    loadSettings(jsn.actionInfo.payload.settings);
});

function loadSettings(settings) {
    restoreSettings(settings);
}

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

function saveSettings() {
    const settingIds = ['simhubControl'];

    const payload = buildSettings(settingIds);
    $PI.setSettings(payload);
}
