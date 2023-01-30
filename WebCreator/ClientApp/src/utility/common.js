import { createStore, combineReducers } from 'redux'

export function saveToLocalStorage(state){
    try {
        localStorage.setItem('state', JSON.stringify(state));
    } catch (e) {
        console.error(e);
    }
};

export function loadFromLocalStorage(){
    try {
        const stateStr = localStorage.getItem('state');
        return stateStr ? JSON.parse(stateStr) : undefined;
    } catch (e) {
        console.error(e);
        return undefined;
    }
};

export function clearLocalStorage(){
    try {
        localStorage.clear()
    } catch (e) {
        console.error(e);
    }
};