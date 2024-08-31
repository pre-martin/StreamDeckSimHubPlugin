
class SimHubRole {

    static placeholder = '----'; // use same default value as wotever

    addRoleName = (selectElement, roleName) => {
        const option = document.createElement('option');
        option.value = roleName;
        option.text = roleName;
        selectElement.add(option);
    }

    updateSimHubRoles = (elementId, roleList, currentRole) => {
        const selectElement = document.getElementById(elementId);

        let seenCurrentRole = false;
        selectElement.innerHTML = '';

        this.addRoleName(selectElement, SimHubRole.placeholder);
        seenCurrentRole = SimHubRole.placeholder === currentRole;
        roleList.forEach(roleName => {
            this.addRoleName(selectElement, roleName);
            if (roleName === currentRole) {
                seenCurrentRole = true;
            }
        });
        if (!seenCurrentRole && currentRole) {
            this.addRoleName(selectElement, currentRole);
        }
        selectElement.value = currentRole ? currentRole : SimHubRole.placeholder;
    }

    /**
     * Returns the list of available SimHub Control Mapper roles by fetching them from SimHub.
     */
    fetchSimHubRoles = async () => {
        try {
            const abortController = new AbortController();
            const abortTimer = setTimeout(() => abortController.abort(), 2000);
            const response = await fetch('http://localhost:8888/api/ControlMapper/GetRoles', {
                signal: abortController.signal
            });
            clearTimeout(abortTimer);

            if (response.ok) {
                return await response.json();
            } else {
                console.log('Server error while retrieving mapper roles from SimHub: ' + response.status + ' ' + response.statusText);
                $PI.logMessage('Server error while retrieving mapper roles from SimHub: ' + response.status + ' ' + response.statusText);
            }
        } catch (error) {
            console.error('Could not fetch mapper roles from SimHub', error);
            $PI.logMessage('Could not fetch mapper roles from SimHub: ' + error);
        }
        return [];
    }
}

const $SimHubRole = new SimHubRole();
