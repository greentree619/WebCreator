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
import { CKEditor } from '@ckeditor/ckeditor5-react'
import ClassicEditor from '@ckeditor/ckeditor5-build-classic'

const View = (props) => {
  const location = useLocation()
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [projectId, setProjectId] = useState('')
  const [countForNow, setCountForNow] = useState(1)
  const [eachCount, setEachCount] = useState(1)
  const [betweenNumber, setBetweenNumber] = useState(1)
  const [betweenUnit, setBetweenUnit] = useState(60)//1 min as default
  const [betweenUnitLabel, setBetweenUnitLabel] = useState('Minute(s)')
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

  if (location.state == null && location.search.length > 0) {
    location.state = { projectid: new URLSearchParams(location.search).get('domainId') }
  }

  if ( location.search.length > 0) {
    
    new URLSearchParams(location.search).get('domainId')
  }

  useEffect(() => {
    if(location.state.projectid != null)
    {
      console.log("useEffect--->" + location.state.projectid);
      setProjectId( location.state.projectid );
      console.log(projectId);
      if(location.state.projectid.length > 0) getScheduleInfo(location.state.projectid);
    }
      
  }, [])

  const handleBetweenUnitClick = (betweenUnit) => {
    setBetweenUnitLabel(betweenUnit.unit)
    setBetweenUnit(betweenUnit.value)
    console.log('clicked ' + betweenUnit)
  }

  const startScrapping = async (domainId) => {
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/startaf/${domainId}/${location.state.scheduleId}`,
    )
    setAlarmVisible(false)
    setAlertMsg('Unfortunately, scrapping faild.')
    setAlertColor('danger')
    if (response.status === 200) {
      //console.log('add success')
      setAlertMsg('Completed to scrapping questions from Article Forge successfully.')
      setAlertColor('success')
    }
    setAlarmVisible(true)

    console.log('clicked ' + betweenUnit)
  }

  const renderItem = (unit, index) => {
    return (
      <CDropdownItem key={index} onClick={() => handleBetweenUnitClick(unit)}>
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
    setAlertColor('danger')
    setAlertMsg('Faild to update schedule unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      setAlertMsg('Updated schedule successfully.')
      setAlertColor('success')
    }
    setAlarmVisible(true)
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>Article Forge Schedule Management</CCardHeader>
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
            onSubmit={handleSubmit}
          >
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">Scrapping Count for just now</CFormLabel>
              <CFormInput
                type="number"
                id="justNowCountFormControlInput"
                aria-label="countForNow"
                value={countForNow}
                onChange={(e) => setCountForNow(e.target.value)}
              />
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
                        return renderItem(unit, index)
                      })}
                    </CDropdownMenu>
                  </CDropdown>
                </CCol>
              </CRow>
            </div>
            <div className="mb-3">
              <CButton type="submit">Update</CButton>
              &nbsp;
              <CButton type="button" onClick={() => startScrapping(projectId)}>Start Scrapping</CButton>
            </div>
          </CForm>
        </CCardBody>
      </CCard>
    </>
  )
}

export default View
