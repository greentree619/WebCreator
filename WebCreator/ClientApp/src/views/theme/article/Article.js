import PropTypes from 'prop-types'
import React, { useEffect, useState, createRef, useRef } from 'react'
import classNames from 'classnames'
import fileDownload from 'js-file-download'
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
  CCardTitle,
  CCardText,
  CCardSubtitle,
  CCallout,
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'
import SunEditor,{buttonList} from 'suneditor-react';
import 'suneditor/dist/css/suneditor.min.css'; // Import Sun Editor's CSS File
import plugins from 'suneditor/src/plugins'
import katex from 'katex'
import 'katex/dist/katex.min.css'
import pixabayImageGallery  from 'src/plugins/PixabayImageGallery'

const Article = (props) => {
  const location = useLocation()
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [selectedFile, setSelectedFile] = useState(null)
  const navigate = useNavigate()

  if (location.state == null && location.search.length > 0) {
    location.state = { projectid: new URLSearchParams(location.search).get('domainId'), 
    domainName: new URLSearchParams(location.search).get('domainName'), 
    domainIp: new URLSearchParams(location.search).get('domainIp') }
  }

  useEffect(() => {
  }, [])

  async function uploadHandler() {
    if(selectedFile == null){
      alert('Please select theme zip file.');
      return;
    }

    const data = new FormData();
    data.append(`theme`, selectedFile, selectedFile.name);

    const requestOptions = {
      method: 'POST',
      body: data,
    }

    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/themeUpload/${location.state.domainName}`,
      requestOptions,
    )

    setAlertColor('danger');
    setAlertMsg('Theme upload is failed, unfortunatley.');
    let ret = await response.json()
    if (response.status === 200 && ret) {
      setAlertColor('success');
      setAlertMsg('Zip file was created successfully.');
    }
    setAlarmVisible(true);
  }

  const handleFileReader = (event) => {
    setSelectedFile(event.target.files[0]);
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>Theme Upload</CCardHeader>
        <CCardBody>
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
              <CFormInput type="file" 
                    id="formFile" onChange={handleFileReader}/>
            </CCol>
            <CCol>
              <CButton color="primary" onClick={uploadHandler}>Upload Theme Zip</CButton>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>
    </>
  )
}

export default Article
