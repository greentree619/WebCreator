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
  activeTab: 'project_add',
  activeMainTab: 'dashboard',
  isOnScrapping: false,
  isOnAFScrapping: false,
  isOnPublish: false,
  curProjectArticleList: [],
  curSearchArticleList: [],  
  isLoadingAllArticle: false,
  notification: [],
  headerHeight: 0
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
