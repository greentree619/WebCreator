import React, { useEffect, useState } from 'react'
import { useLocation } from 'react-router-dom'

import routes from '../routes'

import { CBreadcrumb, CBreadcrumbItem } from '@coreui/react'
import { useDispatch, useSelector } from 'react-redux'
import {
  CContainer,
  CHeader,
  CHeaderBrand,
  CHeaderDivider,
  CHeaderNav,
  CHeaderToggler,
  CNavLink,
  CNavItem,
  CBadge,
  CRow,
  CCol,
} from '@coreui/react'
import { NavLink } from 'react-router-dom'
import { Outlet, Link } from 'react-router-dom'

const AppBreadcrumb = () => {
  let refreshIntervalId = 0
  const dispatch = useDispatch()
  const currentLocation = useLocation().pathname
  const activeDomainName = useSelector((state) => state.activeDomainName)
  const activeDomainIp = useSelector((state) => state.activeDomainIp)
  const activeDomainId = useSelector((state) => state.activeDomainId)
  const activeZoneId = useSelector((state) => state.activeZoneId)
  const activeZoneName = useSelector((state) => state.activeZoneName)
  const activeZoneStatus = useSelector((state) => state.activeZoneStatus)
  const activeTab = useSelector((state) => state.activeTab)
    //const activeProject = useSelector((state) => state.activeProject)

  const getRouteName = (pathname, routes) => {
    const currentRoute = routes.find((route) => route.path === pathname)
    return currentRoute ? currentRoute.name : false
  }

  const getBreadcrumbs = (location) => {
    const breadcrumbs = []
    location.split('/').reduce((prev, curr, index, array) => {
      const currentPathname = `${prev}/${curr}`
      const routeName = getRouteName(currentPathname, routes)
      routeName &&
        breadcrumbs.push({
          pathname: currentPathname,
          name: routeName,
          active: index + 1 === array.length ? true : false,
        })
      return currentPathname
    })
    return breadcrumbs
  }

  const breadcrumbs = getBreadcrumbs(currentLocation)
  const [isOnScrapping, setIsOnScrapping] = useState(false)
  const [isOnAFScrapping, setIsOnAFScrapping] = useState(false)
  const [scrappingMode, setScrappingMode] = useState(0)
  const [isOnPublish, setIsOnPublish] = useState(false)

  async function loadScrappingStatus() {
    //console.log("loadScrappingStatus->" + activeDomainId, activeDomainName);
    try {
      if (activeDomainId.length > 0) {
        const requestOptions = {
          method: 'GET',
          headers: { 'Content-Type': 'application/json' },
        }

        const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/isscrapping/${activeDomainId}`, requestOptions)
        let ret = await response.json()
        if (response.status === 200 && ret) {
          setIsOnScrapping(ret.serpapi);
          setIsOnAFScrapping(ret.afapi);
          setIsOnPublish(ret.publish);
          setScrappingMode(ret.scrappingScheduleMode);
          dispatch({ type: 'set', isOnScrapping: ret.serpapi, isOnAFScrapping: ret.afapi, isOnPublish: ret.publish })
        }
      }
      else {
        setIsOnScrapping(false);
        setIsOnAFScrapping(false);
        setIsOnPublish(false);
        setScrappingMode(0);
        dispatch({ type: 'set', isOnScrapping: false, isOnAFScrapping: false, isOnPublish: false })
      }
    } catch (e) {
      console.log(e);
      //setIsOnScrapping(false);
      //setIsOnAFScrapping(false);
    }
  }

  useEffect(() => {
    clearInterval( refreshIntervalId );
    refreshIntervalId = setInterval(loadScrappingStatus, 1000);
    return () => {
      //unmount
      clearInterval(refreshIntervalId);
      console.log('Bread Crumb project scrapping status interval cleared!!!');
    }
  }, [activeDomainId])

  const isSelctedTab = (urlToken) => {
    //console.log(activeTab);
    var activeClass = "";
    if(activeTab == urlToken) activeClass = "active border-bottom border-bottom-primary border-2";
    return "" + activeClass + "";
  }

  return (
    <>
      <CContainer>
        <CRow className="align-items-center border-bottom border-bottom-light border-2">
          <CCol xs="auto" className="me-auto">
            <CBreadcrumb className="m-0 ms-2">
              {/* <CBreadcrumbItem href="/">Home</CBreadcrumbItem>
              {breadcrumbs.map((breadcrumb, index) => {
                return (
                  <CBreadcrumbItem
                    {...(breadcrumb.active ? { active: true } : { href: breadcrumb.pathname })}
                    key={index}
                  >
                    {breadcrumb.name}
                  </CBreadcrumbItem>
                )
              })} */}

              <CHeaderNav className="d-md-flex me-auto main-menu">
                <CNavItem className="px-1">
                  <CNavLink href="#/project/add?mode=view&tab=project_add" className={isSelctedTab("project_add")}>
                    {'Website'}
                  </CNavLink>
                </CNavItem>
                {activeDomainName.length > 0 && (
                  <>
                    <CNavItem className="px-1">
                      <CNavLink className={isSelctedTab("project_keyword")} href={'#/project/keyword/?tab=project_keyword&domainId=' + activeDomainId + '&domainName=' + activeDomainName}>
                        Keyword
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink className={isSelctedTab("cloudflare_dns")} href={'#/cloudflare/dns/?tab=cloudflare_dns&zoneId=' + activeZoneId + '&domainName=' + activeDomainName}>
                        DNS Status
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink className={isSelctedTab("theme_article")} href={'#/theme/article/?tab=theme_article&domainId=' + activeDomainId + '&domainName=' + activeDomainName + '&domainIp=' + activeDomainIp}>
                        Theme
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink className={isSelctedTab("schedule_view")} href={'#/schedule/view/?tab=schedule_view&domainId=' + activeDomainId + '&isOnAFScrapping=' + isOnAFScrapping + '&scrappingMode=' +scrappingMode+ '&isOnPublish=' + isOnPublish}>
                        Schedule
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink
                        className={isSelctedTab("article_approval")}
                        href={'#/article/approval/?tab=article_approval&domainId=' + activeDomainId + '&domainName=' + activeDomainName + '&domainIp=' + activeDomainIp}
                      >
                        Approval Article
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink
                        className={isSelctedTab("article_list")}
                        href={'#/article/list/?tab=article_list&domainId=' + activeDomainId + '&domainName=' + activeDomainName + '&domainIp=' + activeDomainIp}
                      >
                        Article Pages
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink className={isSelctedTab("sync_view")} href={'#/sync/view?tab=sync_view&domainId=' + activeDomainId + '&domain=' + activeDomainName + '&domainIp=' + activeDomainIp}>
                        Sync
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink className={isSelctedTab("log_view")} href={'#/log/view?tab=log_view&domainId=' + activeDomainId + '&domain=' + activeDomainName + '&domainIp=' + activeDomainIp}>
                        Log
                      </CNavLink>
                    </CNavItem>
                    {/* <CNavItem className="px-1">
                      <CNavLink className="btn btn-light" href="#" disabled>
                        Article Theme
                      </CNavLink>
                    </CNavItem> */}
                  </>
                )}
              </CHeaderNav>
            </CBreadcrumb>
          </CCol>
        </CRow>
      </CContainer>
    </>
  )
}

export default React.memo(AppBreadcrumb)
