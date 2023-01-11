// Global web socket for Stream Deck Property Inspector
let websocket = null, uuid = null;

function connect(inPort, inPropertyInspectorUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inPropertyInspectorUUID;

    if (websocket) {
        websocket.close();
        websocket = null;
    }

    websocket = new WebSocket('ws://127.0.0.1:' + inPort);

    websocket.onopen = function () {
        const json = {
            'event': inRegisterEvent,
            'uuid': inPropertyInspectorUUID
        };
        websocket.send(JSON.stringify(json));
    }

    websocket.onmessage = function (inEvent) {
        const data = JSON.parse(inEvent.data);

        log('Ignored websocket message', inEvent);
    }

    const actionInfo = JSON.parse(inActionInfo);
    const settings = actionInfo.payload.settings;
    loadSettings(settings);
}


function debounce(func, timeout = 300) {
    let timer;
    return (...args) => {
        const context = this;
        console.log('Clearing timer', timer);
        clearTimeout(timer);
        timer = setTimeout(() => func.apply(context, args), timeout);
        console.log('Timer: ', timer);
    }
}


function log(inMessage, inObject) {
    const now = new Date();
    const stamp = now.toISOString();
    let msg = 'PI: ' + inMessage;
    if (inObject) {
        msg += ' ' + JSON.stringify(inObject);
    }

    const sendRemote = websocket && websocket.readyState === WebSocket.OPEN;

    if (sendRemote === false || true) {
        console.log('(no websocket)', stamp, msg);
    } else {
        console.log(stamp, msg);
        const json = {
            'event': 'logMessage',
            'payload': {
                'message': msg
            }
        };
        websocket.send(JSON.stringify(json));
    }
}
