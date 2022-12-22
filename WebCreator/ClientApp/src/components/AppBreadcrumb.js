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
  const currentLocation = useLocation().pathname
  const activeDomainName = useSelector((state) => state.activeDomainName)
  const activeDomainId = useSelector((state) => state.activeDomainId)
  const activeZoneId = useSelector((state) => state.activeZoneId)
  const activeZoneName = useSelector((state) => state.activeZoneName)
  const activeZoneStatus = useSelector((state) => state.activeZoneStatus)
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

  useEffect(() => {
    async function loadScrappingStatus()  {
      try {
       if( activeDomainId.length > 0 ){
         const requestOptions = {
           method: 'GET',
           headers: { 'Content-Type': 'application/json' },
         }
     
         const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/isscrapping/${activeDomainId}`, requestOptions)
         let ret = await response.json()
         if (response.status === 200 && ret) {
           //console.log(ret);
           setIsOnScrapping(ret.serpapi);
           setIsOnAFScrapping(ret.afapi);
         }
       }
       else
       {
          setIsOnScrapping(false);
          setIsOnAFScrapping(false);
       }
     } catch (e) {
         //console.log(e);
         setIsOnScrapping(false);
         setIsOnAFScrapping(false);
     }
   }

    var refreshIntervalId = setInterval(loadScrappingStatus, 1000);
    return ()=>{
      //unmount
      clearInterval(refreshIntervalId);
      console.log('Bread Crumb project scrapping status interval cleared!!!');
    }    
  }, [])

  return (
    <>
      <CContainer>
        <CRow className="align-items-center">
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

              <CHeaderNav className="d-md-flex me-auto">
                <CNavItem className="px-1">
                  <CNavLink href="#/project/add?mode=view" className="btn btn-light">
                    {'DOMAIN : ' + activeDomainName}
                  </CNavLink>
                </CNavItem>
                {activeDomainName.length > 0 && (
                  <>
                    <CNavItem className="px-1">
                      <CNavLink href={'#/cloudflare/dns/?domainId=' + activeZoneId + '&domainName=' + activeZoneName} className="btn btn-light">
                        DNS Status
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink href={'#/schedule/view/?domainId=' + activeDomainId} className="btn btn-light">
                        Article Scrap Schedule
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink
                        className="btn btn-light"
                        href={'#/article/list/?domainId=' + activeDomainId}
                      >
                        Article Pages
                      </CNavLink>
                    </CNavItem>
                    <CNavItem className="px-1">
                      <CNavLink className="btn btn-light" href={'#/sync/view?domainId=' + activeDomainId + '&domain='+activeDomainName}>
                        Sync
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
          <CCol xs="auto">
            <CBadge color={activeZoneStatus == 'active' ? "success" : "dark"} shape="rounded-pill">{activeDomainName}</CBadge>
            &nbsp;
            <CBadge color={isOnScrapping ? "success" : "dark"} shape="rounded-pill">Query Scrapping</CBadge>
            &nbsp;
            <CBadge color={isOnAFScrapping ? "success" : "dark"} shape="rounded-pill">Article Forge Scheduleing</CBadge>
          </CCol>
        </CRow>
      </CContainer>
    </>
  )
}

export default React.memo(AppBreadcrumb)
