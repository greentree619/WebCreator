import React, { useEffect, useState, useRef } from 'react'
import { NavLink } from 'react-router-dom'
import { useSelector, useDispatch } from 'react-redux'
import {
  CContainer,
  CHeader,
  CHeaderBrand,
  CHeaderDivider,
  CHeaderNav,
  CHeaderToggler,
  CNavLink,
  CNavItem,
  CCol,
  CBadge,
  CButton,
  CAvatar,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilBell, cilBellExclamation, cilEnvelopeOpen, cilList, cilMenu } from '@coreui/icons'

import { AppBreadcrumb } from './index'
import { AppHeaderDropdown } from './header/index'
import { logo } from 'src/assets/brand/logo'
import { loadFromLocalStorage, alertConfirmOption, getProjectState } from 'src/utility/common'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const AppHeader = () => {
  const dispatch = useDispatch()
  const headerRef = useRef()
  const sidebarShow = useSelector((state) => state.sidebarShow)
  const activeDomainName = useSelector((state) => state.activeDomainName)
  const activeDomainIp = useSelector((state) => state.activeDomainIp)
  const activeMainTab = useSelector((state) => state.activeMainTab)
  const activeDomainId = useSelector((state) => state.activeDomainId)
  const activeZoneId = useSelector((state) => state.activeZoneId)
  const activeZoneName = useSelector((state) => state.activeZoneName)
  const activeZoneStatus = useSelector((state) => state.activeZoneStatus)
  const isOnScrapping= useSelector((state) => state.isOnScrapping)
  const isOnAFScrapping= useSelector((state) => state.isOnAFScrapping)
  const isOnPublish= useSelector((state) => state.isOnPublish)
  const activeProject= useSelector((state) => state.activeProject)
  const curProjectArticleList= useSelector((state) => state.curProjectArticleList)
  const isLoadingAllArticle= useSelector((state) => state.isLoadingAllArticle)
  const notification = useSelector((state) => state.notification)
  const [curDomainName, setCurDomainName] = useState(activeDomainName)
  
  let preLoadAsyncId = 0
  let prevDomainId = ''

  useEffect(() => {
    //console.log("AppHeader ->", isOnScrapping, isOnAFScrapping, isOnPublish, activeDomainName, activeDomainIp)

    if(activeDomainIp == "0.0.0.0"){
      var s3Host = loadFromLocalStorage('s3host')
      var s3Name = (s3Host.name == null || s3Host.name.length == 0) ? activeDomainName : s3Host.name
      var s3Region = s3Host.region == null ? "us-east-2" : s3Host.region
      setCurDomainName(`${s3Name}.s3.${s3Region}.amazonaws.com`)
    }
    else setCurDomainName(activeDomainName)

    console.log( "AppHeader hieght ->", headerRef.current.getBoundingClientRect().height )
    dispatch({ type: 'set', headerHeight: headerRef.current.getBoundingClientRect().height })

    if(prevDomainId != activeDomainId)
    {
        if(preLoadAsyncId != 0) clearTimeout( preLoadAsyncId )
        preLoadAsyncId = setTimeout(() => {
          preLoadArticlesByDomain( activeDomainId )
        }, 0)
    }
    prevDomainId = activeDomainId
  }, [isOnScrapping, isOnAFScrapping, isOnPublish, activeProject])

  const preLoadArticlesByDomain = async ( domainId ) => {
    console.log( "preLoadArticlesByDomain" )
    const projectId = domainId

    var curPage = 1
    dispatch({ type: 'set', curProjectArticleList: [] })
    dispatch({ type: 'set', curSearchArticleList: [] })
    dispatch({ type: 'set', isLoadingAllArticle: true })
    var articlelst = [];
    while( true )
    {
      const response = await fetch(
        `${process.env.REACT_APP_SERVER_URL}article/` +
          (projectId != '' ? projectId + '/0/' : '') +
          curPage +
          '/50?keyword=',
      )
      const data = await response.json()
      if (response.status === 200) 
      {
        console.log( "data.total", data );
        var articlelst = [...articlelst, ...data.data]
        console.log( "curProjectArticleList=>", articlelst, data.total, curPage );
        dispatch({ type: 'set', curProjectArticleList: articlelst })
        if( data.total <= curPage ) break
      }
      else break      
      curPage++
    }
    dispatch({ type: 'set', isLoadingAllArticle: false })
    console.log('locading complete: ', isLoadingAllArticle, "wowwwoowowo")
    preLoadAsyncId = 0
  }

  const showNotification = () => {
    notification.map((item, index) => {
      toast.info(item, alertConfirmOption);
      // console.log(item)
    })
    dispatch({ type: 'set', notification: [] })
  }

  return (
    <>
      <ToastContainer
        position="top-right"
        autoClose={5000}
        hideProgressBar={false}
        newestOnTop={false}
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="colored"
      />
      <CHeader position="sticky" className="mb-4" ref={headerRef}>
        <CContainer fluid>
          <CHeaderToggler
            className="ps-1"
            onClick={() => dispatch({ type: 'set', sidebarShow: !sidebarShow })}
          >
            <CIcon icon={cilMenu} size="lg" />
          </CHeaderToggler>
          <CHeaderBrand className="mx-auto d-md-none" to="/">
            <CIcon icon={logo} height={48} alt="Logo" />
          </CHeaderBrand>
          <CHeaderNav className="d-none d-md-flex me-auto root-menu">
            <CNavItem>
              <CNavLink to="/dashboard" component={NavLink} className={((activeMainTab == "dashboard") ? "active" : "")}>
                Dashboard
              </CNavLink>
            </CNavItem>
            {/* <CNavItem>
            <CNavLink href="#">Users</CNavLink>
          </CNavItem>
          <CNavItem>
            <CNavLink href="#">Settings</CNavLink>
          </CNavItem> */}
            <CNavItem>
              <CNavLink href={'#/article/setting/?domainId=' + activeDomainId} className={((activeMainTab == "article-forge") ? "active" : "")}>
                AF Setting
              </CNavLink>
            </CNavItem>
            <CNavItem>
              <CNavLink href={'#/openai/setting/?domainId=' + activeDomainId} className={((activeMainTab == "openai") ? "active" : "")}>
                OpenAI Setting
              </CNavLink>
            </CNavItem>
          </CHeaderNav>
          <CHeaderNav>
            {activeDomainName.length > 0 && (
              <CCol xs="auto">
                <CBadge color={activeZoneStatus == 'active' ? "success" : "dark"} shape="rounded-pill">{curDomainName}</CBadge>
                &nbsp;
                <CBadge color={isOnScrapping ? "success" : "dark"} shape="rounded-pill">Query Scrap</CBadge>
                &nbsp;
                <CBadge color={isOnAFScrapping ? "success" : "dark"} shape="rounded-pill">Scrapping</CBadge>
                &nbsp;
                <CBadge color={isOnPublish ? "success" : "dark"} shape="rounded-pill">Publish</CBadge>
              </CCol>
            )}
            {/* <CNavItem>
            <CNavLink href={void(0)}>
              <CIcon icon={cilBell} size="lg" />
            </CNavLink>
          </CNavItem> */}
            {/* <CNavItem>
            <CNavLink href="#">
              <CIcon icon={cilList} size="lg" />
            </CNavLink>
          </CNavItem>
          <CNavItem>
            <CNavLink href="#">
              <CIcon icon={cilEnvelopeOpen} size="lg" />
            </CNavLink>
          </CNavItem> */}
          </CHeaderNav>
          <CHeaderNav>
            <CNavItem>
              <CNavLink href={void (0)} onClick={() => showNotification()}>
                {notification.length > 0 ? (<CAvatar color="danger" textColor="white">
                  <CIcon icon={cilBell} size="lg" />
                </CAvatar>) : (<CIcon icon={cilBell} size="lg" />)}
              </CNavLink>
            </CNavItem>
          </CHeaderNav>
          <CHeaderNav className="ms-3">
            <AppHeaderDropdown />
          </CHeaderNav>
        </CContainer>
        <CHeaderDivider />
        {activeDomainName.length > 0 && (
          <CContainer fluid>
            <AppBreadcrumb />
          </CContainer>
        )}
      </CHeader>
    </>
  )
}

export default AppHeader
