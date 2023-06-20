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
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { alertConfirmOption } from 'src/utility/common'

const Setting = (props) => {
  const location = useLocation()
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [sentence, setSntence] = useState(1)
  const [paragraphSel, setParagraphSel] = useState(1)
  const [shuffle, setShuffle] = useState(false)
  const [video, setVideo] = useState(0.00)
  const [length, setLength] = useState(2)
  const [quality, setQuality] = useState(4)
  const [title, setTitle] = useState(false)
  const [image, setImage] = useState(0.00)
  const navigate = useNavigate()

  const getAFSettingInfo = async () => {
    const requestOptions = {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    }
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}setting/afsetting`, requestOptions)
    let ret = await response.json()
    if (response.status === 200 && ret) {
      console.log(ret, ret.data, Number(ret.data.video.toFixed(2)))
      setSntence(ret.data.sentenceVariation)//const [sentence, setSntence] = useState(1)
      setParagraphSel(ret.data.paragraphVariation)//const [paragraph, setParagraphSel] = useState(1)
      setShuffle(ret.data.shuffleParagraphs ? true : false)//const [shuffle, setShuffle] = useState(false)
      setVideo(Number(ret.data.video.toFixed(2)))//const [video, setVideo] = useState(0.00)
      setLength(ret.data.length)//const [length, setLength] = useState(2)
      setQuality(ret.data.quality)//const [quality, setQuality] = useState(4)
      setTitle(ret.data.title ? true : false)//const [title, setTitle] = useState(false)
      setImage(Number(ret.data.image.toFixed(2)))//const [image, setImage] = useState(0.00)
    }    
  }

  useEffect(() => {
    getAFSettingInfo()
  }, [])

  const handleSubmit = (event) => {
    const form = event.currentTarget
    event.preventDefault()

    if (form.checkValidity() === false) {
      event.stopPropagation()
    } else {
      updateAFSetting()
    }
    //setValidated(true)
  }

  async function updateAFSetting() {
    console.log(sentence, paragraphSel, shuffle)
    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        sentenceVariation: Number(sentence),
        paragraphVariation: Number(paragraphSel),
        shuffleParagraphs: Number(shuffle ? 1 : 0),
        length: Number(length),
        title: Number(title ? 1 : 0),
        image: Number(image),
        video: Number(video),
        quality: Number(quality),
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}setting/afsetting`, requestOptions)
    // setAlertColor('danger')
    // setAlertMsg('Faild to update Article Forge Setting unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      // setAlertMsg('Updated Article Forge Setting successfully.')
      // setAlertColor('success')
      toast.success('Updated Article Forge Setting successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Faild to update Article Forge Setting unfortunatley.', alertConfirmOption);
    }
    // setAlarmVisible(true)
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>Article Forge Setting</CCardHeader>
        <CCardBody>
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
          <CForm 
            className="row g-3 needs-validation"
            noValidate
            onSubmit={handleSubmit}
          >
            <div className="mb-3">
              <CRow>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="sentenceVariation">Sentence Variation</CFormLabel>
                  <CFormSelect id="sentenceVariation" value={sentence} onChange={(obj) => setSntence(obj.target.value)} size="sm" className="mb-3" aria-label="Small select example">
                      <option value="1">1</option>
                      <option value="2">2</option>
                      <option value="3">3</option>
                  </CFormSelect>
                  (number of sentence variations. It can be either 1, 2, or 3. The default value is 1.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="paragraphVariation">Paragraph Variation</CFormLabel>
                  <CFormSelect id="paragraphVariation" value={paragraphSel} onChange={(obj) => setParagraphSel(obj.target.value)} size="sm" className="mb-3" aria-label="Small select example">
                      <option value="1">1</option>
                      <option value="2">2</option>
                      <option value="3">3</option>
                  </CFormSelect>
                  (number of paragraph variations. It can be either 1, 2, or 3. The default value is 1.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="shuffleParagraphs">Shuffle Paragraphs</CFormLabel>
                  <CFormSwitch size="lg" checked={shuffle} onChange={() => setShuffle(!shuffle)} label="Shuffle Paragraphs Enable/Disable" id="shuffleParagraphs"/>
                  <br/>
                  (enable shuffle paragraphs or not. It can be either 0(disabled) or 1(enabled). The default value is 0.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormRange min={0.00} max={1.00} step={0.01} value={video} onChange={(obj)=>{setVideo(obj.target.value)}} id="videoRange" label={"video Range (" + video + ")"} />
                  <br/>
                  (the probability of adding a video into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.)
                </CCol>
              </CRow>
            </div>
            <div className='mb-3'>
              <CRow>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="lengthInput">Length</CFormLabel>
                  <CFormSelect id="lengthInput" value={length} onChange={(obj) => setLength(obj.target.value)} size="sm" className="mb-3" aria-label="Small select example">
                      <option value="1">very_short</option>
                      <option value="2">short</option>
                      <option value="3">medium</option>
                      <option value="4">long</option>
                  </CFormSelect>
                  (the length of the article. It can be either ‘very_short’(approximately 50 words), ‘short’(approximately 200 words), ‘medium’(approximately 500 words), or ‘long’(approximately 750 words). The default value is ‘short’.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="qualityInput">Quality</CFormLabel>
                  <CFormSelect id="qualityInput" value={quality} onChange={(obj) => setQuality(obj.target.value)} size="sm" className="mb-3" aria-label="quality">
                      <option value="1">Regular</option>
                      <option value="2">Unique</option>
                      <option value="3">Very Unique</option>
                      <option value="4">Readable</option>
                      <option value="5">Very Readable</option>
                  </CFormSelect>
                  (the quality of article. It can be either 1(Regular), 2(Unique), 3(Very Unique), 4(Readable), or 5(Very Readable). The default value is 4.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormLabel htmlFor="titleSwitch">Title</CFormLabel>
                  <CFormSwitch checked={title} onChange={() => setTitle(!title)} size="lg" label="Title Switch" id="titleSwitch"/>
                  <br/>
                  (It can be either 0 or 1. If it is set to be 0, the article generated is without titles and headings. The default value is 0.)
                </CCol>
                <CCol className='col-3' xs>
                  <CFormRange min={0.00} max={1.00} step={0.01} value={image} onChange={(obj)=>{setImage(obj.target.value)}} id="imageRange" label={"Image Range (" + image  + ")"} />
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
