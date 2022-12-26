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
  CFormSwitch,
  CFormRange,
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'
import { CKEditor } from '@ckeditor/ckeditor5-react'
import ClassicEditor from '@ckeditor/ckeditor5-build-classic'

const Setting = (props) => {
  const location = useLocation()
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [projectId, setProjectId] = useState('')
  const navigate = useNavigate()

  const handleSubmit = (event) => {
    const form = event.currentTarget
    event.preventDefault()

    if (form.checkValidity() === false) {
      event.stopPropagation()
    } else {
      //updateSchedule()
    }
    //setValidated(true)
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>Article Forge Setting</CCardHeader>
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
              <CRow>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="sentenceVariation">Sentence Variation</CFormLabel>
                  <CFormSelect id="sentenceVariation" size="sm" className="mb-3" aria-label="Small select example">
                      <option value="1">1</option>
                      <option value="2">2</option>
                      <option value="3">3</option>
                  </CFormSelect>
                  (number of sentence variations. It can be either 1, 2, or 3. The default value is 1.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="paragraphVariation">Paragraph Variation</CFormLabel>
                  <CFormSelect id="paragraphVariation" size="sm" className="mb-3" aria-label="Small select example">
                      <option value="1">1</option>
                      <option value="2">2</option>
                      <option value="3">3</option>
                  </CFormSelect>
                  (number of paragraph variations. It can be either 1, 2, or 3. The default value is 1.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="shuffleParagraphs">Shuffle Paragraphs</CFormLabel>
                  <CFormSwitch size="lg" label="Shuffle Paragraphs Enable/Disable" id="shuffleParagraphs"/>
                  <br/>
                  (enable shuffle paragraphs or not. It can be either 0(disabled) or 1(enabled). The default value is 0.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormRange min={0.00} max={1.00} step={0.01} defaultValue="0.00" id="videoRange" label="video Range" />
                  <br/>
                  (the probability of adding a video into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.)
                </CCol>
              </CRow>
            </div>
            <div className='mb-3'>
              <CRow>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="lengthInput">Length</CFormLabel>
                  <CFormSelect id="lengthInput" size="sm" className="mb-3" aria-label="Small select example">
                      <option value="short">short</option>
                      <option value="medium">medium</option>
                      <option value="long">long</option>
                  </CFormSelect>
                  (the length of the article. It can be either ‘very_short’(approximately 50 words), ‘short’(approximately 200 words), ‘medium’(approximately 500 words), or ‘long’(approximately 750 words). The default value is ‘short’.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="qualityInput">Quality</CFormLabel>
                  <CFormSelect id="qualityInput" size="sm" className="mb-3" aria-label="quality">
                      <option value="1">Regular</option>
                      <option value="2">Unique</option>
                      <option value="3">Very Unique</option>
                      <option value="4" selected>Readable</option>
                      <option value="5">Very Readable</option>
                  </CFormSelect>
                  (the quality of article. It can be either 1(Regular), 2(Unique), 3(Very Unique), 4(Readable), or 5(Very Readable). The default value is 4.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="titleSwitch">Title</CFormLabel>
                  <CFormSwitch size="lg" label="Title Switch" id="titleSwitch"/>
                  <br/>
                  (It can be either 0 or 1. If it is set to be 0, the article generated is without titles and headings. The default value is 0.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormRange min={0.00} max={1.00} step={0.01} defaultValue="0.00" id="imageRange" label="Image Range" />
                  <br/>
                  (the probability of adding an image into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.)
                </CCol>
                
              </CRow>
            </div>
            <div>
              <CRow>
                <CCol xs="auto" className="me-auto">
                </CCol>
                <CCol xs="auto">
                  <CButton type="submit">Save</CButton>
                </CCol>
              </CRow>
            </div>
          </CForm>
        </CCardBody>
      </CCard>
    </>
  )
}

export default Setting
