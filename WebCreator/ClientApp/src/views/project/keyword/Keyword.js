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
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import {saveToLocalStorage, globalRegionMap, loadFromLocalStorage, clearLocalStorage, alertConfirmOption } from 'src/utility/common'

const Keyword = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  const activeProject = useSelector((state) => state.activeProject)

  if (location.search.length > 0) {
    //console.log()
    location.state = { project: activeProject }
    //console.log(location)
  }

  const [searchKeyword, setSearchKeyword] = useState(
    location.state != null ? location.state.project.keyword : '',
  )
  const [questionsCount, setQuestionsCount] = useState(
    location.state != null ? location.state.project.quesionsCount : 50,
  )
  
  const [manualSearchKeyword, setManualSearchKeyword] = useState('')
  const [fileSearchKeyword, setFileSearchKeyword] = useState('')

  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [activeKey, setActiveKey] = useState(1)

  dispatch({ type: 'set', activeTab: "project_keyword" })

  const readKeyFile = async (e) => {
    e.preventDefault()
    const reader = new FileReader()
    reader.onload = async (e) => { 
      const text = (e.target.result)
      var tmpKeyword = text.replaceAll('\r\n', ';')
      if(tmpKeyword[tmpKeyword.length-1] == ';') tmpKeyword = tmpKeyword.substring(0, tmpKeyword.length-1)
      
      const requestOptions = {
        method: 'POST',
        body: tmpKeyword,
      }
      const response = await fetch(
        `${process.env.REACT_APP_SERVER_URL}project/keywordTranslate?lang=` + location.state.project.language,
        requestOptions,
      )
      let ret = await response.json()
      if (response.status === 200 && ret) {
        setSearchKeyword(ret.data);
      }
      else toast.error('Keyword Translate Error!', alertConfirmOption);
    };
    reader.readAsText(e.target.files[0])
  }

  const readManualKeyFile = async (e) => {
    e.preventDefault()
    const reader = new FileReader()
    reader.onload = async (e) => { 
      const text = (e.target.result)
      var tmpKeyword = text.replaceAll('\r\n', ';')
      if(tmpKeyword[tmpKeyword.length-1] == ';') tmpKeyword = tmpKeyword.substring(0, tmpKeyword.length-1)
      
      const requestOptions = {
        method: 'POST',
        body: tmpKeyword,
      }
      const response = await fetch(
        `${process.env.REACT_APP_SERVER_URL}project/keywordTranslate?lang=` + location.state.project.language,
        requestOptions,
      )
      let ret = await response.json()
      if (response.status === 200 && ret) {
        setFileSearchKeyword(ret.data);
      }
      else toast.error('Keyword Translate Error!', alertConfirmOption);
    };
    reader.readAsText(e.target.files[0])
  }

  const inputChangeHandler = (setFunction, event) => {
    setFunction(event.target.value)
  }

  async function updateProject() {
    var s3Host = loadFromLocalStorage('s3host')
    var s3Name = (s3Host.name == null || s3Host.name.length == 0) ? this.state.projectInfo.projectDomain : s3Host.name;
    var s3Region = s3Host.region == null ? "us-east-2" : s3Host.region;

    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: location.state ? location.state.project.id : '-1',
        name: location.state.project.name,
        ip: location.state.project.ip,
        s3BucketName: s3Name,
        s3BucketRegion: s3Region,
        keyword: searchKeyword,
        quesionscount: questionsCount,
        contactInfo: location.state.project.contactInfo,
        language: location.state.project.language,
        languageString: location.state.project.languageString,
      }),
    }

    console.log( requestOptions );
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project`, requestOptions)
    // setAlertColor('danger')
    // setAlertMsg('Faild to update unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      // setAlertMsg('Updated scrap information successfully.')
      // setAlertColor('success')

      location.state.project.keyword = searchKeyword
      location.state.project.quesionsCount = questionsCount
      dispatch({ type: 'set', activeProject: location.state.project })
      console.log(location.state.project, "after update")

      toast.success('Updated scrap information successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Faild to update unfortunatley.', alertConfirmOption);
    }
    // setAlarmVisible(true)
  }

  async function scrapQuery(_id, keyword, count) {
    keyword = keyword.replaceAll(';', '&')
    keyword = keyword.replaceAll('?', ';')
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/serpapi/` + _id + '/' + keyword + '/' + count,
    )
    // setAlarmVisible(false)
    // setAlertMsg('Unfortunately, scrapping faild.')
    // setAlertColor('danger')
    if (response.status === 200) {
      //console.log('add success')
      // setAlertMsg('Completed to scrapping questions from google successfully.')
      // setAlertColor('success')
      toast.success('Completed to scrapping questions from google successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Unfortunately, scrapping faild.', alertConfirmOption);
    }
    // setAlarmVisible(true)
  }

  async function addManualArticleByTitle()
  {
    var kwd = manualSearchKeyword
    kwd = kwd.replaceAll(';', '&')
    kwd = kwd.replaceAll('?', ';')
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}article/addArticlesByTitle/` + location.state.project.id + '/' + kwd,
    )
    // setAlarmVisible(false)
    // setAlertMsg('Unfortunately, To adding manual article faild.')
    // setAlertColor('danger')
    if (response.status === 200) {
      //console.log('add success')
      // setAlertMsg('Completed to add manual article successfully.')
      // setAlertColor('success')
      toast.success('Completed to add manual article successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Unfortunately, To adding manual article faild.', alertConfirmOption);
    }
    // setAlarmVisible(true)
  }

  async function addManualArticleByFileTitle()
  {
    var kwd = fileSearchKeyword
    kwd = kwd.replaceAll(';', '&')
    kwd = kwd.replaceAll('?', ';')
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}article/addArticlesByTitle/` + location.state.project.id + '/' + kwd,
    )
    // setAlarmVisible(false)
    // setAlertMsg('Unfortunately, To adding manual article faild.')
    // setAlertColor('danger')
    if (response.status === 200) {
      //console.log('add success')
      // setAlertMsg('Completed to add manual article from file successfully.')
      // setAlertColor('success')
      toast.success('Completed to add manual article from file successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Unfortunately, To adding manual article faild.', alertConfirmOption);
    }
    // setAlarmVisible(true)
  }

  return (
    <>
      <CContainer className="px-4">
        <CRow xs={{ gutterX: 5 }}>
          <CCol>
            <CCard className="mb-4">
              <CCardHeader>Keyword Management</CCardHeader>
              <CCardBody>
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
                      href={void(0)}
                      active={activeKey === 1}
                      onClick={() => setActiveKey(1)}
                    >
                      Scrap
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      href={void(0)}
                      active={activeKey === 2}
                      onClick={() => setActiveKey(2)}
                    >
                      Manual
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      href={void(0)}
                      active={activeKey === 3}
                      onClick={() => setActiveKey(3)}
                    >
                      From File
                    </CNavLink>
                  </CNavItem>                  
                  <CNavItem>
                    &nbsp;
                    <Link to={`/history/view/?category=Keyword&projectId=${location.state.project.id}`}>
                      <CButton type="button">History View</CButton>
                    </Link>
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
                            onChange={(e) => inputChangeHandler(setSearchKeyword, e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                            value={searchKeyword}
                          />
                        </CCol>
                        <CCol>
                          <CFormInput type="file"
                            id="formFile"
                            onChange={(e) => readKeyFile(e)}
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
                        onChange={(e) => inputChangeHandler(setQuestionsCount, e)}
                      //required={simpleMode ? false : true}
                      //disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                        value={questionsCount}
                      />
                    </div>
                    <div className={'mb-12 d-grid gap-2 col-6 mx-auto'}>
                      <CButton
                        type="button"
                        onClick={() =>
                          scrapQuery(
                            location.state.project.id,
                            location.state.project.keyword,
                            location.state.project.quesionsCount,
                          )
                        }
                      >
                        Scrap
                      </CButton>
                      &nbsp;
                      <CButton type="button" onClick={() =>
                            updateProject()
                          }>Update</CButton>
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
                            onChange={(e) => inputChangeHandler(setManualSearchKeyword, e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                            value={manualSearchKeyword}
                          />
                        </CCol>
                      </CRow>
                    </div>
                    <div className={'mb-12 d-grid gap-2 col-6 mx-auto'}>
                      <CButton type="button" onClick={() =>
                            addManualArticleByTitle()
                          }>Add</CButton>
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
                            onChange={(e) => inputChangeHandler(setFileSearchKeyword, e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                            value={fileSearchKeyword}
                          />
                        </CCol>
                        <CCol>
                          <CFormInput type="file"
                            id="fromManualFile"
                            onChange={(e) => readManualKeyFile(e)}
                          // disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                          />
                        </CCol>
                      </CRow>
                    </div>
                    <div className={'mb-12 d-grid gap-2 col-6 mx-auto'}>
                      <CButton type="button" onClick={() =>
                            addManualArticleByFileTitle()
                          }>Add</CButton>
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
