

$PI.onConnected(jsn => {
    loadSettings(jsn.actionInfo.payload.settings);
});

function loadSettings(settings) {
    restoreSettings(settings);
}

const saveSettingsDelayed = Utils.debounce(500, () => saveSettings());

function saveSettings() {
    const settingIds = ['hotkey', 'ctrl', 'alt', 'shift', 'simhubControl', 'simhubProperty'];

    const payload = buildSettings(settingIds);
    $PI.setSettings(payload);
}
