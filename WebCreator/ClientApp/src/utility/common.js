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

export function deleteFromArray(ary, articleId){
    let idx = ary.findIndex((article) => article.id === articleId)
    ary.splice(idx, 1)
};

export function getPageFromArray(ary, pageNo, pageSize, keyword, state){
    var pageList = []
    var aryBuf = ary
    var total = 0;
    try {
        if(keyword.length > 0 || state > 0)
        {
            let { _data } = searchFromArray(ary, keyword, state)
            aryBuf = _data
        }

        total = Math.ceil( aryBuf.length / pageSize )
        pageList = aryBuf.slice(pageNo * pageSize, (pageNo + 1) * pageSize);
    } catch (e) {
        console.error(e)
    }
    return {_data: pageList, _curPage: pageNo, _total: total}
};

export function searchFromArray( ary, keyword, state ){
    var result = []
    ary.map((row) => {
        var index = row.title.indexOf( keyword )
        console.log(index)
        if(index >= 0 && (state == 0 || state == row.state))
            result.push(row)
    })
    return {_data: result}
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

export async function getProjectState(domainId) {
    var isOnScrapping = false
    var isOnAFScrapping = false
    var isOnPublish = false
    var scrappingMode = 0
    //console.log("getProjectState", domainId)
    try {
        if (domainId.length > 0) {
            const requestOptions = {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
            }

            const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/isscrapping/${domainId}`, requestOptions)
            let ret = await response.json()
            if (response.status === 200 && ret) {
                isOnScrapping = ret.serpapi
                isOnAFScrapping = ret.afapi
                isOnPublish = ret.publish
                scrappingMode = ret.scrappingScheduleMode
            }
        }
    } catch (e) {
        console.log(e);
        //setIsOnScrapping(false);
        //setIsOnAFScrapping(false);
    }

    return {
        isOnScrapping,
        isOnAFScrapping,
        isOnPublish,
        scrappingMode
    }
}