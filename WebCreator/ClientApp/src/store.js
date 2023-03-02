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
  isOnScrapping: false,
  isOnAFScrapping: false,
  isOnPublish: false,
  curProjectArticleList: [],
  curSearchArticleList: [],
  isLoadingAllArticle: false,
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
