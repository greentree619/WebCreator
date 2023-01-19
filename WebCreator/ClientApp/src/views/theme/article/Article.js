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
      `${process.env.REACT_APP_SERVER_URL}project/themeUpload/${location.state.projectid}/${location.state.domainName}/${location.state.domainIp}`,
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
          <br/>
          <CRow>
            <CCol>
            <CCard>
              <CCardBody>
                <CCardTitle>HowTo Upload Theme</CCardTitle>
                <CCardSubtitle className="mb-2 text-medium-emphasis">(This theme is only for this domain.)</CCardSubtitle>
                <CCardText>
                  1. Make sure to ready &ldquo;index.html&ldquo;, &ldquo;articlepage.html&ldquo; file and &ldquo;assets&ldquo; folder in same folder.
                </CCardText>
                <CCardText>
                  2. Update &ldquo;articlepage.html&ldquo; file like following:<br/>
                  &emsp;TODO&#41;. Put below code where should be put article content.
                  <CCallout color="primary">
                  &#123;&#123;TITLE&#125;&#125;<br/>
                  &#123;&#123;META_DESC&#125;&#125;<br/>
                  &#123;&#123;META_KEYWORD&#125;&#125;<br/>
                  &#123;&#123;META_AUTHOR&#125;&#125;<br/>
                  &#123;&#123;CONTENT&#125;&#125;<br/>
                  &#123;&#123;FOOTER&#125;&#125;
                  </CCallout>
                </CCardText>                
                <CCardText>
                  3. Compress as zip &ldquo;index.html&ldquo;, &ldquo;articlepage.html&ldquo; files and &ldquo;assets&ldquo; Folder without sub folder in zip file.
                </CCardText>
                <CCardText>
                  4. Upload above zip file.
                </CCardText>
              </CCardBody>
            </CCard>              
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>
    </>
  )
}

export default Article
