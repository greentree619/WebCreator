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
  CFormSelect,
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate, Link } from 'react-router-dom'
import { CKEditor } from '@ckeditor/ckeditor5-react'
import ClassicEditor from '@ckeditor/ckeditor5-build-classic'
import { Col } from 'reactstrap'
import { useDispatch, useSelector } from 'react-redux'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { alertConfirmOption } from 'src/utility/common'

const View = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  
  if (location.state == null && location.search.length > 0) {
    location.state = { projectid: new URLSearchParams(location.search).get('domainId'),
                      isOnAFScrapping: new URLSearchParams(location.search).get('isOnAFScrapping'),
                      isOnPublish: new URLSearchParams(location.search).get('isOnPublish'),
                      scrappingMode: new URLSearchParams(location.search).get('scrappingMode'), }
  }

  //console.log(location.state);

  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [projectId, setProjectId] = useState('')
  const [countForNow, setCountForNow] = useState(1)
  const [eachCount, setEachCount] = useState(1)
  const [betweenNumber, setBetweenNumber] = useState(1)
  const [betweenUnit, setBetweenUnit] = useState(86400)//1 min as default
  const [betweenUnitLabel, setBetweenUnitLabel] = useState('Days(s)')

  const [publishCountForNow, setPublishCountForNow] = useState(1)
  const [publishEachCount, setPublishEachCount] = useState(1)
  const [publishBetweenNumber, setPublishBetweenNumber] = useState(1)
  const [publishBetweenUnit, setPublishBetweenUnit] = useState(86400)//1 min as default
  const [publishBetweenUnitLabel, setPublishBetweenUnitLabel] = useState('Days(s)')
  const [scrappingScheduleMode, setScrappingScheduleMode] = useState(0)

  const [scrapCommand, setScrapCommand] = useState((location.state.isOnAFScrapping == 'true' ? 'Stop Scrapping' : 'Start Scrapping'))
  const [publishCommand, setPublishCommand] = useState((location.state.isOnPublish == 'true' ? 'Stop Publish' : 'Start Publish'))
  const navigate = useNavigate()

  let unitMap = [
    { unit: 'Minute(s)', value: 60 },
    { unit: 'Hour(s)', value: 3600 },
    { unit: 'Days(s)', value: 86400 },
  ]

  let unitLabelMap = {
    60: 'Minute(s)',
    3600: 'Hour(s)',
    86400: 'Days(s)',
  };

  const getScheduleInfo = async (domainId) => {
    const requestOptions = {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/schedule/${domainId}`, requestOptions)
    let ret = await response.json()
    if (response.status === 200 && ret) {
      console.log(ret, ret.data.justNowCount);
      console.log(unitLabelMap[ret.data.spanUnit], ret.data.spanUnit);
      location.state.scheduleId = ret.data.id;
      setCountForNow(ret.data.justNowCount);
      setEachCount(ret.data.eachCount);
      setBetweenNumber(ret.data.spanTime);
      setBetweenUnit(ret.data.spanUnit);
      setBetweenUnitLabel(unitLabelMap[ret.data.spanUnit]);
    }    
  }

  const getPublishScheduleInfo = async (domainId) => {
    const requestOptions = {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/publishSchedule/${domainId}`, requestOptions)
    let ret = await response.json()
    if (response.status === 200 && ret && ret.data) {
      //console.log(ret, ret.data.publishJustNowCount);
      //console.log(unitLabelMap[ret.data.publishSpanUnit], ret.data.publishSpanUnit);
      console.log("schedule ret.data=>", ret.data);
      location.state.publishScheduleId = ret.data.id;
      setPublishCountForNow(ret.data.justNowCount);
      setPublishEachCount(ret.data.eachCount);
      setPublishBetweenNumber(ret.data.spanTime);
      setPublishBetweenUnit(ret.data.spanUnit);
      setPublishBetweenUnitLabel(unitLabelMap[ret.data.spanUnit]);
    }    
  }

  useEffect(() => {
    dispatch({ type: 'set', activeTab: 'schedule_view' })

    if(location.state.projectid != null)
    {
      console.log("useEffect--->" + location.state.projectid);
      setProjectId( location.state.projectid );
      setScrappingScheduleMode( location.state.scrappingMode );
      console.log(projectId);
      if(location.state.projectid.length > 0){
        getScheduleInfo(location.state.projectid);
        getPublishScheduleInfo(location.state.projectid);
      } 
    }
      
  }, [])

  const handleBetweenUnitClick = (betweenUnit) => {
    setBetweenUnitLabel(betweenUnit.unit)
    setBetweenUnit(betweenUnit.value)
    console.log('clicked ' + betweenUnit)
  }

  const handlePublishBetweenUnitClick = (betweenUnit) => {
    setPublishBetweenUnitLabel(betweenUnit.unit)
    setPublishBetweenUnit(betweenUnit.value)
    console.log('clicked ' + betweenUnit)
  }

  const startScrapping = async (domainId) => {
    location.state.isOnAFScrapping = (location.state.isOnAFScrapping == 'true' ? 'false' : 'true')
    const response = await fetch(
      scrappingScheduleMode == 0 ? `${process.env.REACT_APP_SERVER_URL}project/startaf/${domainId}/${location.state.scheduleId}`
                                 : `${process.env.REACT_APP_SERVER_URL}project/startOpenAI/${domainId}/${location.state.scheduleId}`,
    )
    // setAlarmVisible(false)
    // setAlertMsg('Unfortunately, scrapping faild.')
    // setAlertColor('danger')
    if (response.status === 200) {
      //console.log('add success')
      let ret = await response.json()
      if(ret)
      {
        setScrapCommand('Stop Scrapping')
        // setAlertMsg('Article Forge Scrapping Schedule started successfully.')
        // setAlertColor('success')
        toast.success('Article Forge Scrapping Schedule started successfully.', alertConfirmOption);
      }
      else
      {
        setScrapCommand('Start Scrapping')
        // setAlertMsg('Article Forge Scrapping Schedule stopped.')
        // setAlertColor('success')
        toast.success('Article Forge Scrapping Schedule stopped.', alertConfirmOption);
      }
    }
    else
    {
      toast.error('Unfortunately, scrapping faild.', alertConfirmOption);
    }
    // setAlarmVisible(true)

    console.log('clicked ' + betweenUnit)
  }

  const startPublish = async (domainId) => {
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/startPublish/${domainId}/${location.state.publishScheduleId}`,
    )
    // setAlarmVisible(false)
    // setAlertMsg('Unfortunately, Publish Schedule faild.')
    // setAlertColor('danger')
    if (response.status === 200) {
      //console.log('add success')
      let ret = await response.json()
      if(ret)
      {
        setPublishCommand('Stop Publish')
        // setAlertMsg('Publish Schedule started successfully.')
        // setAlertColor('success')
        toast.success('Publish Schedule started successfully.', alertConfirmOption);
      }
      else
      {
        setPublishCommand('Start Publish')
        // setAlertMsg('Publish Schedule stopped.')
        // setAlertColor('success')
        toast.success('Publish Schedule stopped.', alertConfirmOption);
      }
    }
    else
    {
      toast.error('Unfortunately, Publish Schedule faild.', alertConfirmOption);
    }
    // setAlarmVisible(true)

    console.log('clicked ' + betweenUnit)
  }

  const renderItem = (unit, index, callback) => {
    return (
      <CDropdownItem key={index} onClick={() => callback(unit)}>
        {unit.unit}
      </CDropdownItem>
    )
  }

  const handleSubmit = (event) => {
    const form = event.currentTarget
    event.preventDefault()

    if (form.checkValidity() === false) {
      event.stopPropagation()
    } else {
      updateSchedule()
    }
    //setValidated(true)
  }

  async function updateSchedule() {
    if(projectId.length <= 0) return;

    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: location.state.scheduleId,
        projectId: projectId,
        justNowCount: Number(countForNow),
        eachCount: Number(eachCount),
        spanTime: Number(betweenNumber),
        spanUnit: Number(betweenUnit),
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/updateSchedule`, requestOptions)
    // setAlertColor('danger')
    // setAlertMsg('Faild to update AF schedule unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      // setAlertMsg('Updated AF schedule successfully.')
      // setAlertColor('success')
      toast.success('Updated AF schedule successfully. Please restart schedule.', alertConfirmOption);
    }
    else
    {
      toast.error('Faild to update AF schedule unfortunatley.', alertConfirmOption);
    }
    // setAlarmVisible(true)
  }

  const handlePublishSubmit = (event) => {
    const form = event.currentTarget
    event.preventDefault()

    if (form.checkValidity() === false) {
      event.stopPropagation()
    } else {
      updatePublishSchedule()
    }
    //setValidated(true)
  }

  async function updatePublishSchedule() {
    if(projectId.length <= 0) return;

    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: location.state.publishScheduleId,
        projectId: projectId,
        justNowCount: Number(publishCountForNow),
        eachCount: Number(publishEachCount),
        spanTime: Number(publishBetweenNumber),
        spanUnit: Number(publishBetweenUnit),
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/updatePublishSchedule`, requestOptions)
    // setAlertColor('danger')
    // setAlertMsg('Faild to update publish schedule unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      // setAlertMsg('Updated publish schedule successfully.')
      // setAlertColor('success')
      toast.success('Updated publish schedule successfully. Please restart schedule.', alertConfirmOption);
    }
    else
    {
      toast.error('Faild to update publish schedule unfortunatley.', alertConfirmOption);
    }
    // setAlarmVisible(true)
  }

  return (
    <>
      {/* <ToastContainer
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
      /> */}
      <CAlert
        color={alertColor}
        dismissible
        visible={alarmVisible}
        onClose={() => setAlarmVisible(false)}
      >
        {alertMsg}
      </CAlert>
      <CRow>
        <CCol>
          <CCard className="mb-4">
            <CCardHeader>Article Forge Schedule</CCardHeader>
            <CCardBody>
              <CForm
                className="row g-3 needs-validation"
                noValidate
                onSubmit={handleSubmit}
              >
                <div className="mb-3">
                  <CRow>
                    <CCol>
                      <CFormLabel htmlFor="scrappingEngine">Scrapping Schedule Mode</CFormLabel>
                      <CFormSelect id="scrappingEngine" value={scrappingScheduleMode} disabled={location.state.isOnAFScrapping == 'true'} 
                      onChange={(obj) => setScrappingScheduleMode(obj.target.value)} size="sm" className="mb-3" aria-label="Small select example">
                        <option value="0">Article Forege</option>
                        <option value="1">OpenAI</option>
                      </CFormSelect>
                    </CCol>
                    <CCol>
                      <CFormLabel htmlFor="exampleFormControlInput1">Scrapping Count for just now</CFormLabel>
                      <CFormInput
                        type="number"
                        id="justNowCountFormControlInput"
                        aria-label="countForNow"
                        value={countForNow}
                        onChange={(e) => setCountForNow(e.target.value)}
                      />
                    </CCol>
                  </CRow>
                </div>
                <div className="mb-3">
                  <CFormLabel>Scrapping Count</CFormLabel>
                  <CFormInput
                    type="number"
                    id="countFormControlInput"
                    aria-label="eachCount"
                    value={eachCount}
                    onChange={(e) => setEachCount(e.target.value)}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="exampleFormControlInput1">Scrapping between</CFormLabel>
                  <CRow>
                    <CCol xs>
                      <CFormInput
                        type="number"
                        id="betweenNumberFormControlInput"
                        placeholder="1"
                        value={betweenNumber}
                        onChange={(e) => setBetweenNumber(e.target.value)}
                      />
                    </CCol>
                    <CCol xs>
                      <CDropdown
                        id="axes-dd"
                        className="float-right mr-0"
                        size="sm"
                      >
                        <CDropdownToggle
                          id="axes-ddt"
                          color="secondary"
                          size="sm"
                        >
                          {betweenUnitLabel}
                        </CDropdownToggle>
                        <CDropdownMenu>
                          {unitMap.map((unit, index) => {
                            return renderItem(unit, index, handleBetweenUnitClick)
                          })}
                        </CDropdownMenu>
                      </CDropdown>
                    </CCol>
                  </CRow>
                </div>
                <div className="mb-3">
                  <CButton type="submit">Update</CButton>
                  &nbsp;
                  <CButton type="button" onClick={() => startScrapping(projectId)}>{scrapCommand}</CButton>
                  &nbsp;
                  <Link to={`/history/view/?category=ArticleScrap&projectId=${projectId}`}>
                    <CButton type="button">History View</CButton>
                  </Link>
                </div>
              </CForm>
            </CCardBody>
          </CCard>
        </CCol>
        <CCol>
        <CCard className="mb-4">
            <CCardHeader>Publish Schedule</CCardHeader>
            <CCardBody>
              <CForm
                className="row g-3 needs-validation"
                noValidate
                onSubmit={handlePublishSubmit}
              >
                <div className="mb-3">
                  <CFormLabel htmlFor="exampleFormControlInput1">Publish Count for just now</CFormLabel>
                  <CFormInput
                    type="number"
                    id="justNowCountFormControlInput"
                    aria-label="countForNow"
                    value={publishCountForNow}
                    onChange={(e) => setPublishCountForNow(e.target.value)}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel>Publish Count</CFormLabel>
                  <CFormInput
                    type="number"
                    id="countFormControlInput"
                    aria-label="eachCount"
                    value={publishEachCount}
                    onChange={(e) => setPublishEachCount(e.target.value)}
                  />
                </div>
                <div className="mb-3">
                  <CFormLabel htmlFor="exampleFormControlInput1">Publish between</CFormLabel>
                  <CRow>
                    <CCol xs>
                      <CFormInput
                        type="number"
                        id="betweenNumberFormControlInput"
                        placeholder="1"
                        value={publishBetweenNumber}
                        onChange={(e) => setPublishBetweenNumber(e.target.value)}
                      />
                    </CCol>
                    <CCol xs>
                      <CDropdown
                        id="axes-dd"
                        className="float-right mr-0"
                        size="sm"
                      >
                        <CDropdownToggle
                          id="axes-ddt"
                          color="secondary"
                          size="sm"
                        >
                          {publishBetweenUnitLabel}
                        </CDropdownToggle>
                        <CDropdownMenu>
                          {unitMap.map((unit, index) => {
                            return renderItem(unit, index, handlePublishBetweenUnitClick)
                          })}
                        </CDropdownMenu>
                      </CDropdown>
                    </CCol>
                  </CRow>
                </div>
                <div className="mb-3">
                  <CButton type="submit">Update</CButton>
                  &nbsp;
                  <CButton type="button" onClick={() => startPublish(projectId)}>{publishCommand}</CButton>
                  &nbsp;
                  <Link to={`/history/view/?category=Publish&projectId=${projectId}`}>
                    <CButton type="button">History View</CButton>
                  </Link>
                </div>
              </CForm>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  )
}

export default View
