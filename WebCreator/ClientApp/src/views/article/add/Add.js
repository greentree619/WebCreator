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

const API_URL = "https://77em4-8080.sse.codesandbox.io";
const UPLOAD_ENDPOINT = "upload_files";

const Add = (props) => {
  const location = useLocation()
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [title, setTitle] = useState('')
  const [projectId, setProjectId] = useState(location.state.projectId)
  const [content, setContent] = useState('')
  const navigate = useNavigate()

  //console.log(projectId)
  useEffect(() => {
  }, [])

  function uploadAdapter(loader) {
    return {
      upload: () => {
        return new Promise((resolve, reject) => {
          const body = new FormData();
          loader.file.then((file) => {
            body.append("files", file);
            // let headers = new Headers();
            // headers.append("Origin", "http://localhost:3000");
            fetch(`${API_URL}/${UPLOAD_ENDPOINT}`, {
              method: "post",
              body: body
              // mode: "no-cors"
            })
              .then((res) => res.json())
              .then((res) => {
                resolve({
                  default: `${API_URL}/${res.filename}`
                });
              })
              .catch((err) => {
                reject(err);
              });
          });
        });
      }
    };
  }

  function uploadPlugin(editor) {
    editor.plugins.get("FileRepository").createUploadAdapter = (loader) => {
      return uploadAdapter(loader);
    };
  }

  const addNewArticle = async () => {
    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        projectId: projectId,
        title: title,
        content: content,
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/add`, requestOptions)
    setAlertColor('danger')
    setAlertMsg('Faild to add article unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      setAlertMsg('Article content is added successfully.')
      setAlertColor('success')
    }
    setAlarmVisible(true)
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>Article View</CCardHeader>
        <CCardBody>
          <CAlert
            color={alertColor}
            dismissible
            visible={alarmVisible}
            onClose={() => setAlarmVisible(false)}
          >
            {alertMsg}
          </CAlert>
          <CForm className="row g-3 needs-validation">
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">Title</CFormLabel>
              <CFormInput
                type="text"
                id="titleFormControlInput"
                aria-label="title"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
              />
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">Content</CFormLabel>
              <CKEditor
                config={{
                  extraPlugins: [uploadPlugin]
                }}
                editor={ClassicEditor}
                data={content}
                onReady={(editor) => {
                  console.log('Editor is ready to use!', editor)
                }}
                onChange={(event, editor) => {
                  const data = editor.getData()
                  setContent(data)
                  //console.log({ event, editor, data })
                }}
                onBlur={(event, editor) => {
                  console.log('Blur.', editor)
                }}
                onFocus={(event, editor) => {
                  console.log('Focus.', editor)
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
