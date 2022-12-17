import React, { useEffect } from 'react'
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
} from '@coreui/react'
import { NavLink } from 'react-router-dom'
import { Outlet, Link } from 'react-router-dom'

const AppBreadcrumb = () => {
  const currentLocation = useLocation().pathname
  const activeDomainName = useSelector((state) => state.activeDomainName)
  const activeDomainId = useSelector((state) => state.activeDomainId)
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

  return (
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
              <CNavLink href="#" className="btn btn-light">
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
              <CNavLink className="btn btn-light" href="#">
                Article Theme
              </CNavLink>
            </CNavItem>
            <CNavItem className="px-1">
              <CNavLink className="btn btn-light" href="#">
                Sync
              </CNavLink>
            </CNavItem>
          </>
        )}
      </CHeaderNav>
    </CBreadcrumb>
  )
}

export default React.memo(AppBreadcrumb)
