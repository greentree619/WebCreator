import { createStore } from 'redux'

const initialState = {
  sidebarShow: true,
  activeDomainId: '',
  activeDomainName: '',
  activeDomainIp: '',
  activeZoneId: '',
  activeZoneName: '',
  activeZoneStatus: '',
  activeProject: {},
}

const changeState = (state = initialState, { type, ...rest }) => {
  switch (type) {
    case 'set':
      return { ...state, ...rest }
    default:
      return state
  }
}

const store = createStore(changeState)
export default store
