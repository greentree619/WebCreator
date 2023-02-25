import React, { useEffect } from 'react'
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

} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilBell, cilEnvelopeOpen, cilList, cilMenu } from '@coreui/icons'

import { AppBreadcrumb } from './index'
import { AppHeaderDropdown } from './header/index'
import { logo } from 'src/assets/brand/logo'

const AppHeader = () => {
  const dispatch = useDispatch()
  const sidebarShow = useSelector((state) => state.sidebarShow)
  const activeDomainName = useSelector((state) => state.activeDomainName)
  const activeDomainIp = useSelector((state) => state.activeDomainIp)
  const activeDomainId = useSelector((state) => state.activeDomainId)
  const activeZoneId = useSelector((state) => state.activeZoneId)
  const activeZoneName = useSelector((state) => state.activeZoneName)
  const activeZoneStatus = useSelector((state) => state.activeZoneStatus)
  const isOnScrapping= useSelector((state) => state.isOnScrapping)
  const isOnAFScrapping= useSelector((state) => state.isOnAFScrapping)
  const isOnPublish= useSelector((state) => state.isOnPublish)

  useEffect(() => {
    console.log("AppHeader ->", isOnScrapping, isOnAFScrapping, isOnPublish, activeDomainName)
  }, [isOnScrapping, isOnAFScrapping, isOnPublish, activeDomainName])

  return (
    <CHeader position="sticky" className="mb-4">
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
        <CHeaderNav className="d-none d-md-flex me-auto">
          <CNavItem>
            <CNavLink to="/dashboard" component={NavLink}>
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
          <CNavLink href={'#/article/setting/?domainId=' + activeDomainId}>
            AF Setting
          </CNavLink>
        </CNavItem>
        <CNavItem>
          <CNavLink href={'#/openai/setting/?domainId=' + activeDomainId}>
            OpenAI Setting
          </CNavLink>
        </CNavItem>
        </CHeaderNav>
        <CHeaderNav>
        {activeDomainName.length > 0 && (
          <CCol xs="auto">
            <CBadge color={activeZoneStatus == 'active' ? "success" : "dark"} shape="rounded-pill">{activeDomainName}</CBadge>
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
        <CHeaderNav className="ms-3">
          <AppHeaderDropdown />
        </CHeaderNav>
      </CContainer>
      <CHeaderDivider />
      <CContainer fluid>
        <AppBreadcrumb />
      </CContainer>
    </CHeader>
  )
}

export default AppHeader
