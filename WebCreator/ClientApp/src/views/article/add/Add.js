import PropTypes from 'prop-types'
import React, { useEffect, useState, createRef, useRef, useMemo } from 'react'
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
import SunEditor,{buttonList} from 'suneditor-react';
import 'suneditor/dist/css/suneditor.min.css'; // Import Sun Editor's CSS File
import plugins from 'suneditor/src/plugins'
import katex from 'katex'
import 'katex/dist/katex.min.css'
import pixabayImageGallery  from 'src/plugins/PixabayImageGallery'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const Add = (props) => {
  const location = useLocation()
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [title, setTitle] = useState('')
  const [projectId, setProjectId] = useState(location.state.projectId)
  const [content, setContent] = useState('')
  const [footer, setFooter] = useState('')
  const [metaDescription, setMetaDescription] = useState('')
  const [metaKeywords, setMetaKeywords] = useState('')
  const [metaAuthor, setMetaAuthor] = useState('')
  const [pixabayURL, setPixabayURL] = useState('https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=&image_type=photo&min_width=480&min_height=600&per_page=100&page=1')
  const navigate = useNavigate()

  function onChange(content)
  {
    console.log('onChange', content);
  }

  useEffect(() => {
    let q = title.replaceAll(' ', '+').replaceAll('?', '')
    console.log(q, title)
    setPixabayURL()
    footEditor.current.core.options.imageGalleryLoadURL = 'https://pixabay.com/api/?key=27944002-ca9bbda02c769f32ad5769e81&q=' + q + '&image_type=photo&min_width=480&min_height=600&per_page=100&page=1'
    bodyEditor.current.core.options.imageGalleryLoadURL = 'https://pixabay.com/api/?key=27944002-ca9bbda02c769f32ad5769e81&q=' + q + '&image_type=photo&min_width=480&min_height=600&per_page=100&page=1'
    console.log(footEditor.current.core.options.imageGalleryLoadURL)

  }, [title])

  const addNewArticle = async () => {
    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        projectId: projectId,
        title: title,
        metaDescription: metaDescription,
        metaKeywords: metaKeywords,
        metaAuthor: metaAuthor,
        content: content,
        footer: footer,
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/add`, requestOptions)
    // setAlertColor('danger')
    // setAlertMsg('Faild to add article unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      // setAlertMsg('Article content is added successfully.')
      // setAlertColor('success')
      toast.success('Article content is added successfully.', {
        position: "top-right",
        autoClose: 5000,
        hideProgressBar: true,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
        theme: "colored",
        });
    }
    else
    {
      toast.error('Faild to add article unfortunatley.', {
        position: "top-right",
        autoClose: 5000,
        hideProgressBar: true,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
        theme: "colored",
        });
    }
    // setAlarmVisible(true)
  }

  const updatePixabayURL = async (val) => {
    setTitle(val)
  }

  const footEditor = useRef(null)
  const bodyEditor = useRef(null)

  const getFooterSunEditorInstance = (sunEditor) => {
    footEditor.current = sunEditor;
  };

  const getBodySunEditorInstance = (sunEditor) => {
    bodyEditor.current = sunEditor;
  };

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>New Article</CCardHeader>
        <CCardBody>
          <CAlert
            color={alertColor}
            dismissible
            visible={alarmVisible}
            onClose={() => setAlarmVisible(false)}
          >
            {alertMsg}
          </CAlert>
          <ToastContainer
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
          />
          <CForm className="row g-3 needs-validation">
          <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">Title</CFormLabel>
              <CFormInput
                type="text"
                id="titleFormControlInput"
                aria-label="title"
                value={title}
                onChange={(e) => updatePixabayURL(e.target.value)}
              />
            </div>
            <div className="mb-3">
            <CRow>
                <CCol>
                  <CFormLabel htmlFor="metaDescription">Meta Description</CFormLabel>
                  <CFormInput
                    type="text"
                    id="metaDescription"
                    aria-label="metaDescription"
                    value={metaDescription}
                    onChange={(e) => setMetaDescription(e.target.value)}
                  />
                </CCol>
                <CCol>
                  <CFormLabel htmlFor="metaKeywords">Meta Keywords</CFormLabel>
                  <CFormInput
                    type="text"
                    id="metaKeywords"
                    aria-label="metaKeywords"
                    value={metaKeywords}
                    onChange={(e) => setMetaKeywords(e.target.value)}
                  />
                </CCol>
                <CCol>
                  <CFormLabel htmlFor="metaAuthor">Meta Author</CFormLabel>
                  <CFormInput
                    type="text"
                    id="metaAuthor"
                    aria-label="metaAuthor"
                    value={metaAuthor}
                    onChange={(e) => setMetaAuthor(e.target.value)}
                  />
                </CCol>
              </CRow>
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">Body</CFormLabel>
              <SunEditor
                getSunEditorInstance={getBodySunEditorInstance}
                setContents={content} 
                onChange={(contents, core) => {
                  setContent(contents)
                  //console.log({ event, editor, data })
                }}
                setOptions={{
                    height: 200,
                    plugins: {...plugins, pixabayImageGallery},
                    katex: katex,
                    //imageGalleryUrl: "",
                    buttonList: [
                        ['undo', 'redo'],
                        ['font', 'fontSize', 'formatBlock'],
                        ['paragraphStyle', 'blockquote'],
                        ['bold', 'underline', 'italic', 'strike', 'subscript', 'superscript'],
                        ['fontColor', 'hiliteColor', 'textStyle'],
                        ['removeFormat'],
                        '/', // Line break
                        ['outdent', 'indent'],
                        ['align', 'horizontalRule', 'list', 'lineHeight'],
                        ['table', 'link', 'image', 'video', 'audio' ,'math'], // You must add the 'katex' library at options to use the 'math' plugin.
                        ['pixabayImageGallery'], // You must add the "imageGalleryUrl".
                        ['fullScreen', 'showBlocks', 'codeView'],
                        ['preview', 'print'],
                        ['save', 'template'],
                        /** ['dir', 'dir_ltr', 'dir_rtl'] */ // "dir": Toggle text direction, "dir_ltr": Right to Left, "dir_rtl": Left to Right
                    ]
              }}/>
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput2"
                
              >Footer</CFormLabel>
              <SunEditor 
                getSunEditorInstance={getFooterSunEditorInstance}
                className='footerEditor'
                setContents={footer} 
                onChange={(contents, core) => {
                  setFooter(contents)
                  //console.log({ event, editor, data })
                }}
                setOptions={{
                  height: 200,
                  plugins: {...plugins, pixabayImageGallery},
                  katex: katex,
                  imageGalleryLoadURL: "https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=&image_type=photo&min_width=480&min_height=600&per_page=100&page=1",
                  buttonList: [
                      ['undo', 'redo'],
                      ['font', 'fontSize', 'formatBlock'],
                      ['paragraphStyle', 'blockquote'],
                      ['bold', 'underline', 'italic', 'strike', 'subscript', 'superscript'],
                      ['fontColor', 'hiliteColor', 'textStyle'],
                      ['removeFormat'],
                      '/', // Line break
                      ['outdent', 'indent'],
                      ['align', 'horizontalRule', 'list', 'lineHeight'],
                      ['table', 'link', 'image', 'video', 'audio' ,'math'], // You must add the 'katex' library at options to use the 'math' plugin.
                      ['pixabayImageGallery'], // You must add the "imageGalleryUrl".
                      ['fullScreen', 'showBlocks', 'codeView'],
                      ['preview', 'print'],
                      ['save', 'template'],
                      /** ['dir', 'dir_ltr', 'dir_rtl'] */ // "dir": Toggle text direction, "dir_ltr": Right to Left, "dir_rtl": Left to Right
                  ]
              }}
              />
            </div>
            <div className="mb-3">
              <CButton type="button" onClick={() => navigate(-1)}>
                Back
              </CButton>
              &nbsp;
              <CButton type="button" onClick={() => addNewArticle()}>
                Add
              </CButton>
            </div>
          </CForm>
        </CCardBody>
      </CCard>
    </>
  )
}

export default Add
