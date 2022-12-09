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
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const navigate = useNavigate()

  useEffect(() => {
    const getFetch = async () => {
      const response = await fetch(
        `${process.env.REACT_APP_SERVER_URL}article/fromid/` + location.state.article.id,
      )
      const data = await response.json()
      //console.log(data)
      setTitle(data.data.title)
      if (data.data.content != null) setContent(data.data.content)
    }
    getFetch()
  }, [])

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
                disabled
                value={title}
                onChange={(e) => setTitle(e.target.value)}
              />
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput1">Content</CFormLabel>
              <CKEditor
                editor={ClassicEditor}
                data={content}
                onReady={(editor) => {
                  console.log('Editor is ready to use!', editor)
                }}
                onChange={(event, editor) => {
                  const data = editor.getData()
                  console.log({ event, editor, data })
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
            </div>
          </CForm>
        </CCardBody>
      </CCard>
    </>
  )
}

export default View
