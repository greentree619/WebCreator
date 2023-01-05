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
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { Outlet, Link } from 'react-router-dom'

const Keyword = (props) => {
  const location = useLocation()

  const [searchKeyword, setSearchKeyword] = useState(
    location.state != null && !simpleMode ? location.state.project.keyword : '',
  )
  const [questionsCount, setQuestionsCount] = useState(
    location.state != null && !simpleMode ? location.state.project.quesionsCount : 50,
  )
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')

  const handleSubmit = (event) => {
    const form = event.currentTarget
    event.preventDefault()

    if (location.state != null && !simpleMode && location.state.mode == 'EDIT') {
      postAddProject()
    } else if (location.state != null && !simpleMode && location.state.mode == 'VIEW') {
      navigate(-1)
    } else {
      if (form.checkValidity() === false) {
        event.stopPropagation()
      } else {
        postAddProject()
      }
      setValidated(true)
    }
  }

  const inputChangeHandler = (setFunction, event) => {
    setFunction(event.target.value)
  }

  async function postAddProject() {
    const requestOptions = {
      method: location.state && !simpleMode ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: location.state && !simpleMode ? location.state.project.id : '-1',
        name: projectName,
        ip: ipAddress,
        keyword: searchKeyword,
        quesionscount: questionsCount,
        language: languageValue,
        languageString: language,
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project`, requestOptions)
    setAlertColor('danger')
    setAlertMsg('Faild to create/update new domain unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      setAlertMsg('Created/Updated new domain successfully.')
      setAlertColor('success')

      if (simpleMode) navigate('/dashboard')
    }
    setAlarmVisible(true)
  }

  const handleClick = (lang, value) => {
    setLanguageValue(value)
    setLanguage(lang)
    //console.log('clicked ' + i + ', state.value = ' + languageValue)
  }

  const renderItem = (lang, value) => {
    return (
      <CDropdownItem key={value} onClick={() => handleClick(lang, value)}>
        {lang}
      </CDropdownItem>
    )
  }

  const handleIpAddrClick = (ipAddr) => {
    setIpAddress(ipAddr)
    //console.log('clicked ' + i + ', state.value = ' + languageValue)
  }

  const renderIpAddrItem = (ipaddr) => {
    return (
      <CDropdownItem key={ipaddr} onClick={() => handleIpAddrClick(ipaddr)}>
        {ipaddr}
      </CDropdownItem>
    )
  }

  let ActionMode = 'Create'
  if (location.state != null && !simpleMode && location.state.mode == 'EDIT') {
    ActionMode = 'Update'
  } else if (location.state != null && !simpleMode && location.state.mode == 'VIEW') {
    ActionMode = 'Back'
  }

  async function scrapQuery(_id, keyword, count) {
    keyword = keyword.replaceAll(';', '&')
    keyword = keyword.replaceAll('?', ';')
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/serpapi/` + _id + '/' + keyword + '/' + count,
    )
    setAlarmVisible(false)
    setAlertMsg('Unfortunately, scrapping faild.')
    setAlertColor('danger')
    if (response.status === 200) {
      //console.log('add success')
      setAlertMsg('Completed to scrapping questions from google successfully.')
      setAlertColor('success')
    }
    setAlarmVisible(true)
  }

  const renderLanguageItem = () => {
    //if(location.state != null )
    return 'English'
  }

  async function getZoneInformation()  {
    try {
     if(location.state != null && location.state.project != null 
       && (location.state.mode == 'VIEW' || location.state.mode == 'EDIT')){
      const response = await fetch(`${process.env.REACT_APP_SERVER_URL}dns/byname/${location.state.project.name}`)
      const data = await response.json()
      //console.log(data.result);
      if(data.result.length > 0){
        //console.log(data.result[0].name);
        //console.log(data.result[0].id);
        dispatch({ type: 'set', activeZoneName: data.result[0].name })
        dispatch({ type: 'set', activeZoneId: data.result[0].id })
        dispatch({ type: 'set', activeZoneStatus: data.result[0].status })
      }
     }
   } catch (e) {
       console.log(e);
   }
  }

  useEffect(() => {
    console.log("location.search.length = " + location.search.length)
    if (location.search.length == 0
      && (location.state != null && (location.state.mode == 'VIEW' || location.state.mode == 'EDIT'))) {
      //normal link
      if (location.state != null && !simpleMode) {
        dispatch({ type: 'set', activeDomainName: location.state.project.name })
        dispatch({ type: 'set', activeDomainId: location.state.project.id })
        dispatch({ type: 'set', activeProject: location.state.project })
        dispatch({ type: 'set', activeDomainIp: location.state.project.ip })
      } else {
        dispatch({ type: 'set', activeDomainName: '', activeProject: {}, activeDomainId: '' })
      }
    }

    getZoneInformation();
    return () => {
      //unmount
      // clearInterval(refreshIntervalId);
      console.log('project scrapping status interval cleared!!!');
    }
  }, [])

  const readKeyFile = async (e) => {
    e.preventDefault()
    const reader = new FileReader()
    reader.onload = async (e) => { 
      const text = (e.target.result)
      //console.log(text)
      //alert(text.replaceAll('\r\n', ';'))
      var tmpKeyword = text.replaceAll('\r\n', ';')
      if(tmpKeyword[tmpKeyword.length-1] == ';') tmpKeyword = tmpKeyword.substring(0, tmpKeyword.length-1)
      setSearchKeyword(tmpKeyword);
    };
    reader.readAsText(e.target.files[0])
  }

  return (
    <>
      <CContainer className="px-4">
        <CRow xs={{ gutterX: 5 }}>
          <CCol>
            <CCard className="mb-4">
              <CCardHeader>New/Update Domain</CCardHeader>
              <CCardBody>
                <CAlert
                  color={alertColor}
                  dismissible
                  visible={alarmVisible}
                  onClose={() => setAlarmVisible(false)}
                >
                  {alertMsg}
                </CAlert>
                <CForm
                  className="row g-3 needs-validation"
                  noValidate
                  validated={validated}
                  onSubmit={handleSubmit}
                >
                  <div className="mb-3">
                    <CFormLabel htmlFor="exampleFormControlInput1">Domain</CFormLabel>
                    <CFormInput
                      type="text"
                      id="projectNameFormControlInput"
                      placeholder="www.domain.com"
                      aria-label="Domain"
                      required
                      onChange={(e) => inputChangeHandler(setProjectName, e)}
                      disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                      value={projectName}
                    />
                  </div>
                  <div className={simpleMode ? 'd-none' : 'mb-3'}>
                    <CFormLabel htmlFor="exampleFormControlInput1">IP Address</CFormLabel>
                    &nbsp;
                    <CDropdown
                      id="axes-dd"
                      className="float-right mr-0"
                      size="sm"
                      disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                    >
                      <CDropdownToggle
                        id="axes-ddt"
                        color="secondary"
                        size="sm"
                        disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                      >
                        {ipAddress}
                      </CDropdownToggle>
                      <CDropdownMenu>
                        {ipAddressMap.map((ipAddr, index) => {
                          return renderIpAddrItem(ipAddr.ip)
                        })}
                      </CDropdownMenu>
                    </CDropdown>
                  </div>
                  <div className={simpleMode ? 'd-none' : 'mb-3'}>
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
                          disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                          value={searchKeyword}
                        />
                      </CCol>
                      <CCol>
                        <CFormInput type="file" 
                        id="formFile" 
                        onChange={(e) => readKeyFile(e)}
                        disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                      </CCol>
                    </CRow>
                  </div>
                  <div className={simpleMode ? 'd-none' : 'mb-3'}>
                    <CFormLabel htmlFor="exampleFormControlInput1">Questions Count</CFormLabel>
                    <CFormInput
                      type="text"
                      id="questionsCountFormControlInput"
                      placeholder="50"
                      onChange={(e) => inputChangeHandler(setQuestionsCount, e)}
                      required={simpleMode ? false : true}
                      disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                      value={questionsCount}
                    />
                  </div>
                  <div className={simpleMode ? 'd-none' : 'mb-3'}>
                    <CFormLabel htmlFor="exampleFormControlInput1">Language</CFormLabel>
                    &nbsp;
                    <CDropdown
                      id="axes-dd"
                      className="float-right mr-0"
                      size="sm"
                      disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                    >
                      <CDropdownToggle
                        id="axes-ddt"
                        color="secondary"
                        size="sm"
                        disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                      >
                        {language}
                      </CDropdownToggle>
                      <CDropdownMenu>
                        {languageMap.map((langInfo, index) => {
                          return renderItem(langInfo.lang, langInfo.value)
                        })}
                      </CDropdownMenu>
                    </CDropdown>
                  </div>
                  <div className="mb-3">
                    {location.state != null && !simpleMode && (
                      <CButton type="button" onClick={() => navigate('/project/add')}>
                        New Domain
                      </CButton>
                    )}
                    &nbsp;
                    {location.state != null && !simpleMode && location.state.mode == 'VIEW' && (
                      <>
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
                        <Link
                          to={`/project/add`}
                          state={{ mode: 'EDIT', project: location.state.project }}
                        >
                          <CButton type="button">Edit</CButton>
                        </Link>
                      </>
                    )}
                    &nbsp;
                    <CButton type="submit">{ActionMode}</CButton>
                  </div>
                </CForm>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      </CContainer>
    </>
  )
}

export default Add
