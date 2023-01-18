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

  const handleDownload = () => {
    fileDownload(articlejs, 'article.js')
  }

  const articlejs = "var getUrlParameter = function getUrlParameter(sParam) {\
    var sPageURL = window.location.search.substring(1),\
        sURLVariables = sPageURL.split('&'),\
        sParameterName,\
        i;\
\
    for (i = 0; i < sURLVariables.length; i++) {\
        sParameterName = sURLVariables[i].split('=');\
\
        if (sParameterName[0] === sParam) {\
            return sParameterName[1] === undefined ? true : decodeURIComponent(sParameterName[1]);\
        }\
    }\
    return false;\
};\
\
$(document).ready(function () {\
    var title = getUrlParameter('article');\
    $.get(\"articles/\" + title + \"-meta\", function (data) {\
        $(\"meta[name='description']\").remove();\
        $(\"meta[name='keywords']\").remove();\
        $(\"meta[name='author']\").remove();\
        $(\"title\").before(data);\
\
    });\
    $.get(\"articles/\" + title + \"-title\", function (data) {\
        $(\"title\").html(data);\
    });\
    $.get(\"articles/\" + title + \"-body\", function (data) {\
        $(\"div#article-content\").html(data);\
    });\
});"

  async function uploadHandler() {
    if(selectedFile == null){
      alert('Please select theme zip file.');
      return;
    }

    const data = new FormData();
    data.append(
      "theme",
      selectedFile,
      location.state.domainName + ".zip"
    );

    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'multipart/form-data' },
      body: data,
    }

    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/themeUpload`,
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
                  1. Make sure to ready &ldquo;index.html&ldquo; file and &ldquo;assets&ldquo; folder in same folder.
                </CCardText>
                <CCardText>
                  2. Update &ldquo;index.html&ldquo; file like following:<br/>
                  &emsp;a&#41;. Add below tags before head close tag - &lt;/head&gt;.
                  <CCallout color="primary">
                  &lt;script src=&quot;https://ajax.googleapis.com/ajax/libs/jquery/3.6.1/jquery.min.js&quot;&gt;&lt;/script&gt;<br/>
                  &lt;script src=&quot;article.js&quot;&gt;&lt;/script&gt;
                  </CCallout>
                  &emsp;b&#41;. Wrap part where should be put article content adding below tag and comment mark.
                  <CCallout color="primary">
                  &lt;!--CONTENT BEGIN--&gt;<br/>
                  &lt;div id=&quot;article-content&quot;&gt;<br/>
                  Article Content Tags<br/>
                  &lt;/div&gt;<br/>
                  &lt;!--CONTENT END--&gt;
                  </CCallout>
                </CCardText>
                <CCardText>
                  3. <CButton color="link" onClick={() => handleDownload()}>Download</CButton> &ldquo;article.js&ldquo; file and copy in same folder that is &ldquo;index.html&ldquo; file.
                </CCardText>
                
                <CCardText>
                  4. Compress as zip &ldquo;index.html&ldquo;, &ldquo;article.js&ldquo; files and &ldquo;assets&ldquo; Folder without sub folder in zip file.
                </CCardText>
                <CCardText>
                  5. Upload above zip file.
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
