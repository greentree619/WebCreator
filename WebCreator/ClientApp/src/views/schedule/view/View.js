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
  const [countForNow, setCountForNow] = useState(1)
  const [betweenNumber, setBetweenNumber] = useState(1)
  const [betweenUnit, setBetweenUnit] = useState(60)//1 min as default
  const [betweenUnitLabel, setBetweenUnitLabel] = useState('Minute(s)')
  const navigate = useNavigate()

  let unitMap = [
    { unit: 'Minute(s)', value: 60 },
    { unit: 'Hour(s)', value: 3600 },
    { unit: 'Days(s)', value: 86400 },
  ]

  const handleBetweenUnitClick = (betweenUnit) => {
    setBetweenUnitLabel(betweenUnit.unit)
    setBetweenUnit(betweenUnit.value)
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
      //postAddProject()
    }
    //setValidated(true)
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
            </div>
          </CForm>
        </CCardBody>
      </CCard>
    </>
  )
}

export default View
