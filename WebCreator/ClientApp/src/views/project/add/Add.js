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

const Add = (props) => {
  let languageMap = [
    { lang: 'Abkhazian', value: 'ab' },
    { lang: 'Afrikaans', value: 'af' },
    { lang: 'Aragonese', value: 'an' },
    { lang: 'Arabic', value: 'ar' },
    { lang: 'Assamese', value: 'as' },
    { lang: 'Azerbaijani', value: 'az' },
    { lang: 'Belarusian', value: 'be' },
    { lang: 'Bulgarian', value: 'bg' },
    { lang: 'Bengali', value: 'bn' },
    { lang: 'Tibetan', value: 'bo' },
    { lang: 'Breton', value: 'br' },
    { lang: 'Bosnian', value: 'bs' },
    { lang: 'Catalan / Valencian', value: 'ca' },
    { lang: 'Chechen', value: 'ce' },
    { lang: 'Corsican', value: 'co' },
    { lang: 'Czech', value: 'cs' },
    { lang: 'Church Slavic', value: 'cu' },
    { lang: 'Welsh', value: 'cy' },
    { lang: 'Danish', value: 'da' },
    { lang: 'German', value: 'de' },
    { lang: 'Greek', value: 'el' },
    { lang: 'English', value: 'en' },
    { lang: 'Esperanto', value: 'eo' },
    { lang: 'Spanish / Castilian', value: 'es' },
    { lang: 'Estonian', value: 'et' },
    { lang: 'Basque', value: 'eu' },
    { lang: 'Persian', value: 'fa' },
    { lang: 'Finnish', value: 'fi' },
    { lang: 'Fijian', value: 'fj' },
    { lang: 'Faroese', value: 'fo' },
    { lang: 'French', value: 'fr' },
    { lang: 'Western Frisian', value: 'fy' },
    { lang: 'Irish', value: 'ga' },
    { lang: 'Gaelic / Scottish Gaelic', value: 'gd' },
    { lang: 'Galician', value: 'gl' },
    { lang: 'Manx', value: 'gv' },
    { lang: 'Hebrew', value: 'he' },
    { lang: 'Hindi', value: 'hi' },
    { lang: 'Croatian', value: 'hr' },
    { lang: 'Haitian; Haitian Creole', value: 'ht' },
    { lang: 'Hungarian', value: 'hu' },
    { lang: 'Armenian', value: 'hy' },
    { lang: 'Indonesian', value: 'id' },
    { lang: 'Icelandic', value: 'is' },
    { lang: 'Italian', value: 'it' },
    { lang: 'Japanese', value: 'ja' },
    { lang: 'Javanese', value: 'jv' },
    { lang: 'Georgian', value: 'ka' },
    { lang: 'Kongo', value: 'kg' },
    { lang: 'Korean', value: 'ko' },
    { lang: 'Kurdish', value: 'ku' },
    { lang: 'Cornish', value: 'kw' },
    { lang: 'Kirghiz', value: 'ky' },
    { lang: 'Latin', value: 'la' },
    { lang: 'Luxembourgish; Letzeburgesch', value: 'lb' },
    { lang: 'Limburgan; Limburger; Limburgish', value: 'li' },
    { lang: 'Lingala', value: 'ln' },
    { lang: 'Lithuanian', value: 'lt' },
    { lang: 'Latvian', value: 'lv' },
    { lang: 'Malagasy', value: 'mg' },
    { lang: 'Macedonian', value: 'mk' },
    { lang: 'Mongolian', value: 'mn' },
    { lang: 'Moldavian', value: 'mo' },
    { lang: 'Malay', value: 'ms' },
    { lang: 'Maltese', value: 'mt' },
    { lang: 'Burmese', value: 'my' },
    { lang: 'Norwegian (Bokmål)', value: 'nb' },
    { lang: 'Nepali', value: 'ne' },
    { lang: 'Dutch', value: 'nl' },
    { lang: 'Norwegian (Nynorsk)', value: 'nn' },
    { lang: 'Norwegian', value: 'no' },
    { lang: 'Occitan (post 1500); Provençal', value: 'oc' },
    { lang: 'Polish', value: 'pl' },
    { lang: 'Portuguese', value: 'pt' },
    { lang: 'Raeto-Romance', value: 'rm' },
    { lang: 'Romanian', value: 'ro' },
    { lang: 'Russian', value: 'ru' },
    { lang: 'Sardinian', value: 'sc' },
    { lang: 'Northern Sami', value: 'se' },
    { lang: 'Slovak', value: 'sk' },
    { lang: 'Slovenian', value: 'sl' },
    { lang: 'Somali', value: 'so' },
    { lang: 'Albanian', value: 'sq' },
    { lang: 'Serbian', value: 'sr' },
    { lang: 'Swedish', value: 'sv' },
    { lang: 'Swahili', value: 'sw' },
    { lang: 'Turkmen', value: 'tk' },
    { lang: 'Turkish', value: 'tr' },
    { lang: 'Tahitian', value: 'ty' },
    { lang: 'Ukrainian', value: 'uk' },
    { lang: 'Urdu', value: 'ur' },
    { lang: 'Uzbek', value: 'uz' },
    { lang: 'Vietnamese', value: 'vi' },
    { lang: 'Volapük', value: 'vo' },
    { lang: 'Yiddish', value: 'yi' },
    { lang: 'Chinese (simplified)', value: 'zh-hans' },
    { lang: 'Thai', value: 'th' },
  ]

  const location = useLocation()
  const dispatch = useDispatch()
  const navigate = useNavigate()

  const [simpleMode, setSimpleMode] = useState(
    location.state != null && location.state.simple_mode != null
      ? location.state.simple_mode
      : false,
  )

  console.log("location.search.length = " + location.search.length)
  if (location.search.length == 0 
      && (location.state != null && (location.state.mode == 'VIEW' || location.state.mode == 'EDIT'))) {
    //normal link
    if (location.state != null && !simpleMode) {
      dispatch({ type: 'set', activeDomainName: location.state.project.name })
      dispatch({ type: 'set', activeDomainId: location.state.project.id })
      dispatch({ type: 'set', activeProject: location.state.project })
    } else {
      dispatch({ type: 'set', activeDomainName: '', activeProject: {}, activeDomainId: '' })
    }
  }

  const activeProject = useSelector((state) => state.activeProject)
  if (location.search.length > 0) {
    //console.log()
    const linkMode = new URLSearchParams(location.search).get('mode')
    if (linkMode == 'view') {
      location.state = { project: activeProject, mode: 'VIEW' }
    }
  }

  const [validated, setValidated] = useState(false)
  const [projectName, setProjectName] = useState(
    location.state != null && !simpleMode ? location.state.project.name : '',
  )
  const [ipAddress, setIpAddress] = useState(
    location.state != null && !simpleMode ? location.state.project.ip : '127.0.0.1',
  )
  const [searchKeyword, setSearchKeyword] = useState(
    location.state != null && !simpleMode ? location.state.project.keyword : '',
  )
  const [questionsCount, setQuestionsCount] = useState(
    location.state != null && !simpleMode ? location.state.project.quesionsCount : 50,
  )
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [mode, setMode] = useState('ADD')
  const [language, setLanguage] = useState(
    location.state && !simpleMode ? location.state.project.languageString : 'Engllish',
  )
  const [languageValue, setLanguageValue] = useState('en')
  const [isOnScrapping, setIsOnScrapping] = useState(false)
  const [isOnAFScrapping, setIsOnAFScrapping] = useState(false)

  let ipAddressMap = [
    { ip: '3.14.14.86', value: '3.14.14.86' },
    { ip: '3.131.110.136', value: '3.131.110.136' },
    { ip: '3.142.69.221', value: '3.142.69.221' },
  ]

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
    setAlertMsg('Faild to create new domain unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      setAlertMsg('Created new domain successfully.')
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
    async function loadScrappingStatus()  {
      try {
       if(location.state != null && location.state.project != null 
         && (location.state.mode == 'VIEW' || location.state.mode == 'EDIT')){
         const requestOptions = {
           method: 'GET',
           headers: { 'Content-Type': 'application/json' },
         }
     
         const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/isscrapping/${location.state.project.id}`, requestOptions)
         let ret = await response.json()
         if (response.status === 200 && ret) {
           //console.log(ret);
           setIsOnScrapping(ret.serpapi);
           setIsOnAFScrapping(ret.afapi);
         }
       }
     } catch (e) {
         //console.log(e);
         setIsOnScrapping(false);
         setIsOnAFScrapping(false);
     }
   }

   getZoneInformation();
    var refreshIntervalId = setInterval(loadScrappingStatus, 1000);
    return ()=>{
      //unmount
      clearInterval(refreshIntervalId);
      console.log('project scrapping status interval cleared!!!');
    }    
  }, [])

  return (
    <>
      <CContainer className="px-4">
        <CRow xs={{ gutterX: 5 }}>
          <CCol>
          <CContainer>
              <CRow xs={{ cols: 2 }}>
                <CCol className="border border-secondary text-center">
                  SerpAPI Scrapping
                </CCol>
                <CCol className="border border-secondary text-center">
                  Article Forge Scrapping
                </CCol>
                <CCol className="border border-secondary text-center">
                  {isOnScrapping ? (
                    <>
                      <CSpinner component="span" size="sm" variant="grow" aria-hidden="true" />
                      Scrapping...
                    </>
                  ) : 'Stopped'
                }
                  
                </CCol>
                <CCol className="border border-secondary text-center">
                  {isOnAFScrapping ? (
                    <>
                      <CSpinner component="span" size="sm" variant="grow" aria-hidden="true" />
                      Scrapping...
                    </>
                  ) : 'Stopped'
                  }
                </CCol>
              </CRow>
            </CContainer>
            <br/>
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
                    <CFormInput
                      type="text"
                      id="searchKeywordFormControlInput"
                      placeholder="Search Keyword"
                      aria-label="Search Keyword"
                      onChange={(e) => inputChangeHandler(setSearchKeyword, e)}
                      disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                      value={searchKeyword}
                    />
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
