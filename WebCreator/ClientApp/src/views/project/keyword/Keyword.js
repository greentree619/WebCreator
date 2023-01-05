import PropTypes from 'prop-types'
import React, { useEffect, useState, createRef } from 'react'
import classNames from 'classnames'
import {
  CRow,
  CCol,
  CCard,
  CButton,
  CCardHeader,
  CCardBody,
  CForm,
  CFormInput,
  CFormLabel,
  CAlert,
  CFormTextarea,
  CDropdown,
  CDropdownItem,
  CDropdownToggle,
  CDropdownMenu,
  CContainer,
  CSpinner,
  CTabs,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { Outlet, Link } from 'react-router-dom'

const Keyword = (props) => {
  const location = useLocation()

  const [searchKeyword, setSearchKeyword] = useState('')
  const [questionsCount, setQuestionsCount] = useState(50)
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [activeKey, setActiveKey] = useState(1)

  return (
    <>
      <CContainer className="px-4">
        <CRow xs={{ gutterX: 5 }}>
          <CCol>
            <CCard className="mb-4">
              <CCardHeader>Keyword Management</CCardHeader>
              <CCardBody>
                <CAlert
                  color={alertColor}
                  dismissible
                  visible={alarmVisible}
                  onClose={() => setAlarmVisible(false)}
                >
                  {alertMsg}
                </CAlert>
                <CNav variant="tabs" role="tablist">
                  <CNavItem>
                    <CNavLink
                      href="javascript:void(0);"
                      active={activeKey === 1}
                      onClick={() => setActiveKey(1)}
                    >
                      Scrap
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      href="javascript:void(0);"
                      active={activeKey === 2}
                      onClick={() => setActiveKey(2)}
                    >
                      Manual
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      href="javascript:void(0);"
                      active={activeKey === 3}
                      onClick={() => setActiveKey(3)}
                    >
                      From File
                    </CNavLink>
                  </CNavItem>
                </CNav>
                <CTabContent>
                  <CTabPane role="tabpanel" aria-labelledby="home-tab" visible={activeKey === 1}>
                    <div className={'mb-3'}>
                      <CFormLabel htmlFor="exampleFormControlInput1">
                        Search Keyword(can use multiple keywords using &apos;;&apos;)
                      </CFormLabel>
                      <CRow>
                        <CCol className='col-9'>
                          <CFormInput
                            type="text"
                            id="searchKeywordFormControlInput"
                            placeholder="Search Keyword"
                            aria-label="Search Keyword"
                          // onChange={(e) => inputChangeHandler(setSearchKeyword, e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                          // value={searchKeyword}
                          />
                        </CCol>
                        <CCol>
                          <CFormInput type="file"
                            id="formFile"
                          // onChange={(e) => readKeyFile(e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                          />
                        </CCol>
                      </CRow>
                    </div>
                    <div className={'mb-3'}>
                      <CFormLabel htmlFor="exampleFormControlInput1">Questions Count</CFormLabel>
                      <CFormInput
                        type="text"
                        id="questionsCountFormControlInput"
                        placeholder="50"
                      //onChange={(e) => inputChangeHandler(setQuestionsCount, e)}
                      //required={simpleMode ? false : true}
                      //disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                      //value={questionsCount}
                      />
                    </div>
                    <div className={'mb-12 d-grid gap-2 col-6 mx-auto'}>
                      <CButton
                        type="button"
                      // onClick={() =>
                      //   scrapQuery(
                      //     location.state.project.id,
                      //     location.state.project.keyword,
                      //     location.state.project.quesionsCount,
                      //   )
                      // }
                      >
                        Scrap
                      </CButton>
                      &nbsp;
                      <CButton type="button">Update</CButton>
                    </div>
                  </CTabPane>
                  <CTabPane role="tabpanel" aria-labelledby="profile-tab" visible={activeKey === 2}>
                  <div className={'mb-3'}>
                      <CFormLabel htmlFor="exampleFormControlInput1">
                        Search Keyword(can use multiple keywords using &apos;;&apos;)
                      </CFormLabel>
                      <CRow>
                        <CCol className='col-12'>
                          <CFormInput
                            type="text"
                            id="searchKeywordFormControlInput"
                            placeholder="Search Keyword"
                            aria-label="Search Keyword"
                          // onChange={(e) => inputChangeHandler(setSearchKeyword, e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                          // value={searchKeyword}
                          />
                        </CCol>
                      </CRow>
                    </div>
                    <div className={'mb-12 d-grid gap-2 col-6 mx-auto'}>
                      <CButton type="button">Add</CButton>
                    </div>
                  </CTabPane>
                  <CTabPane role="tabpanel" aria-labelledby="contact-tab" visible={activeKey === 3}>
                  <div className={'mb-3'}>
                      <CFormLabel htmlFor="exampleFormControlInput1">
                        Search Keyword(can use multiple keywords using &apos;;&apos;)
                      </CFormLabel>
                      <CRow>
                        <CCol className='col-9'>
                          <CFormInput
                            type="text"
                            id="searchKeywordFormControlInput"
                            placeholder="Search Keyword"
                            aria-label="Search Keyword"
                          // onChange={(e) => inputChangeHandler(setSearchKeyword, e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                          // value={searchKeyword}
                          />
                        </CCol>
                        <CCol>
                          <CFormInput type="file"
                            id="formFile"
                          // onChange={(e) => readKeyFile(e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                          />
                        </CCol>
                      </CRow>
                    </div>
                    <div className={'mb-12 d-grid gap-2 col-6 mx-auto'}>
                      <CButton type="button">Add</CButton>
                    </div>
                  </CTabPane>
                </CTabContent>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      </CContainer>
    </>
  )
}

export default Keyword
