
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
