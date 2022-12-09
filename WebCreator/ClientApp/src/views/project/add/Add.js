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
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'

const Add = (props) => {
  const location = useLocation()
  const [validated, setValidated] = useState(false)
  const [projectName, setProjectName] = useState(
    location.state != null ? location.state.project.name : '',
  )
  const [ipAddress, setIpAddress] = useState(
    location.state != null ? location.state.project.ip : '',
  )
  const [searchKeyword, setSearchKeyword] = useState(
    location.state != null ? location.state.project.keyword : '',
  )
  const [questionsCount, setQuestionsCount] = useState(
    location.state != null ? location.state.project.quesionsCount : 50,
  )
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [mode, setMode] = useState('ADD')
  const [language, setLanguage] = useState(
    location.state ? location.state.project.languageString : 'Engllish',
  )
  const [languageValue, setLanguageValue] = useState('en')
  const navigate = useNavigate()

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

  const handleSubmit = (event) => {
    const form = event.currentTarget

    if (location.state != null && location.state.mode == 'EDIT') {
      postAddProject()
    } else if (location.state != null && location.state.mode == 'VIEW') {
      navigate(-1)
    } else {
      event.preventDefault()
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
      method: location.state ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: location.state ? location.state.project.id : '-1',
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

  let ActionMode = 'Add'
  if (location.state != null && location.state.mode == 'EDIT') {
    ActionMode = 'Edit'
  } else if (location.state != null && location.state.mode == 'VIEW') {
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

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>Create Website</CCardHeader>
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
                disabled={location.state != null && location.state.mode == 'VIEW'}
                value={projectName}
              />
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">IP Address</CFormLabel>
              <CFormInput
                type="text"
                id="ipAddressFormControlInput"
                placeholder="127.0.0.1"
                aria-label="IP Address"
                onChange={(e) => inputChangeHandler(setIpAddress, e)}
                disabled={location.state != null && location.state.mode == 'VIEW'}
                value={ipAddress}
              />
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">
                Search Keyword(can use multiple keywords using &apos;;&apos;)
              </CFormLabel>
              <CFormInput
                type="text"
                id="searchKeywordFormControlInput"
                placeholder="Search Keyword"
                aria-label="Search Keyword"
                onChange={(e) => inputChangeHandler(setSearchKeyword, e)}
                disabled={location.state != null && location.state.mode == 'VIEW'}
                value={searchKeyword}
              />
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">Questions Count</CFormLabel>
              <CFormInput
                type="text"
                id="questionsCountFormControlInput"
                placeholder="50"
                onChange={(e) => inputChangeHandler(setQuestionsCount, e)}
                required
                disabled={location.state != null && location.state.mode == 'VIEW'}
                value={questionsCount}
              />
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">Language</CFormLabel>
              &nbsp;
              <CDropdown
                id="axes-dd"
                className="float-right mr-0"
                size="sm"
                disabled={location.state != null && location.state.mode == 'VIEW'}
              >
                <CDropdownToggle
                  id="axes-ddt"
                  color="secondary"
                  size="sm"
                  disabled={location.state != null && location.state.mode == 'VIEW'}
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
              {location.state != null && location.state.mode == 'VIEW' && (
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
              )}
              &nbsp;
              {location.state != null && (
                <CButton type="button" onClick={() => navigate('/project/add')}>
                  Add
                </CButton>
              )}
              &nbsp;
              <CButton type="submit">{ActionMode}</CButton>
            </div>
          </CForm>
        </CCardBody>
      </CCard>
    </>
  )
}

export default Add
