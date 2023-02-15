import { createStore, combineReducers } from 'redux'

export function saveToLocalStorage(state, storeID='state'){
    try {
        localStorage.setItem(storeID, JSON.stringify(state));
    } catch (e) {
        console.error(e);
    }
};

export function loadFromLocalStorage(storeID='state'){
    try {
        const stateStr = localStorage.getItem(storeID);
        return stateStr ? JSON.parse(stateStr) : undefined;
    } catch (e) {
        console.error(e);
        return undefined;
    }
};

export function clearLocalStorage(storeID='state'){
    try {
        //REFME localStorage.clear()
        localStorage.setItem(storeID, null);
    } catch (e) {
        console.error(e);
    }
};

export const globalRegionMap = [
    { region: 'US East (N. Virginia)', value: 'us-east-1'},
    { region: 'US East (Ohio)', value: 'us-east-2'},
    { region: 'US West (N. California)', value: 'us-west-1'},
    { region: 'US West (Oregon)', value: 'us-west-2'},
    { region: 'Asia Pacific (Mumbai)', value: 'ap-south-1'},
    { region: 'Asia Pacific (Osaka)', value: 'ap-northeast-3'},
    { region: 'Asia Pacific (Seoul)', value: 'ap-northeast-2'},
    { region: 'Asia Pacific (Singapore)', value: 'ap-southeast-1'},
    { region: 'Asia Pacific (Sydney)', value: 'ap-southeast-2'},
    { region: 'Asia Pacific (Tokyo)', value: 'ap-northeast-1'},
    { region: 'Canada (Central)', value: 'ca-central-1'},
    { region: 'Europe (Frankfurt)', value: 'eu-central-1'},
    { region: 'Europe (Ireland)', value: 'eu-west-1'},
    { region: 'Europe (London)', value: 'eu-west-2'},
    { region: 'Europe (Paris)', value: 'eu-west-3'},
    { region: 'Europe (Stockholm)', value: 'eu-north-1'},
    { region: 'South America (São Paulo)', value: 'sa-east-1'},
  ]

  export const alertConfirmOption = {
    position: "top-right",
    autoClose: 5000,
    hideProgressBar: true,
    closeOnClick: true,
    pauseOnHover: true,
    draggable: true,
    progress: undefined,
    theme: "colored",
    }