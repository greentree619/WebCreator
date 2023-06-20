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
  CImage,
  CCardTitle,
  CCardText,
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
import { alertConfirmOption, saveToLocalStorage, loadFromLocalStorage } from 'src/utility/common'
import { useDispatch, useSelector } from 'react-redux'
import AddImage from 'src/assets/images/AddImage.png'
import { render } from '@testing-library/react'
import { confirmAlert } from 'react-confirm-alert'; // Import
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css
import AddImagesComponent from '../common/AddImagesComponent'

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
  const [pixabayURL, setPixabayURL] = useState(`https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=&image_type=photo&min_width=${process.env.REACT_APP_PIXABAY_MIN_WIDTH}&min_height=${process.env.REACT_APP_PIXABAY_MIN_HEIGHT}&per_page=100&page=1`)
  const navigate = useNavigate()
  const dispatch = useDispatch()
  const curProjectArticleList = useSelector((state) => state.curProjectArticleList)
  const [addImgVisible, setAddImgVisible] = useState(false)
  const [imageGallery, setImageGallery] = useState([])
  const [imageArray, setImageArray] = useState([])
  const [thumbImageArray, setThumbImageArray] = useState([])
  const addImagesComponent = useRef()

  function onChange( content )
  {
    console.log('onChange', content);
  }

  useEffect(() => {
    let q = title.replaceAll(' ', '+').replaceAll('?', '')
    console.log(q, title)
    setPixabayURL()
    footEditor.current.core.options.imageGalleryLoadURL = 'https://pixabay.com/api/?key=27944002-ca9bbda02c769f32ad5769e81&q=' + q + `&image_type=photo&min_width=${process.env.REACT_APP_PIXABAY_MIN_WIDTH}&min_height=${process.env.REACT_APP_PIXABAY_MIN_HEIGHT}&per_page=100&page=1`
    bodyEditor.current.core.options.imageGalleryLoadURL = 'https://pixabay.com/api/?key=27944002-ca9bbda02c769f32ad5769e81&q=' + q + `&image_type=photo&min_width=${process.env.REACT_APP_PIXABAY_MIN_WIDTH}&min_height=${process.env.REACT_APP_PIXABAY_MIN_HEIGHT}&per_page=100&page=1`
    console.log(footEditor.current.core.options.imageGalleryLoadURL)

  }, [title])

  const addNewArticle = async () => {
    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        projectId: projectId,
        title: title,
        metaDescription: metaDescription,
        metaKeywords: metaKeywords,
        metaAuthor: metaAuthor,
        content: content,
        footer: footer,
        imageArray: imageArray,
        thumbImageArray: thumbImageArray,
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/add`, requestOptions)
    // setAlertColor('danger')
    // setAlertMsg('Faild to add article unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret.data != null) {
      // setAlertMsg('Article content is added successfully.')
      // setAlertColor('success')
      toast.success('Article content is added successfully.', alertConfirmOption);
      var articlesAry = [ret.data, ...curProjectArticleList];
      dispatch({ type: 'set', curProjectArticleList: articlesAry })
    }
    else
    {
      toast.error('Faild to add article unfortunatley.', alertConfirmOption);
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

  const deleteImageConfirm = (idx) => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete this.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => deleteImage(idx)
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  }

  const deleteImage = async (idx) => {
    console.log("deleteImage=>", idx)
    var tmpimgAry = [...imageArray]
    var tmpThumImgAry = [...thumbImageArray]
    tmpimgAry.splice(idx, 1)
    tmpThumImgAry.splice(idx, 1)
    setImageArray(tmpimgAry)
    setThumbImageArray(tmpThumImgAry)
    console.log(thumbImageArray)
  }

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
                  imageGalleryLoadURL: `https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=&image_type=photo&min_width=${process.env.REACT_APP_PIXABAY_MIN_WIDTH}&min_height=${process.env.REACT_APP_PIXABAY_MIN_HEIGHT}&per_page=100&page=1`,
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
            <CCard style={{ width: '100%' }}>
              <CCardBody>
                <CCardTitle>Article Images</CCardTitle>
                <CCardText>
                  <div className="clearfix">
                  {thumbImageArray.map((img, idx) => {
                    //console.log(img.thumb)
                    return (
                      <>
                        &nbsp;<CImage onClick={() => deleteImageConfirm(idx)} key={"thumb"+idx} align="start" rounded src={img} width={80} height={80} />
                      </>)
                  })}
                    &nbsp;<CImage onClick={() => addImagesComponent.current.showAddImageModal()} align="start" rounded src={AddImage} width={80} height={80} />
                  </div>                  
                </CCardText>
              </CCardBody>
            </CCard>
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
      <AddImagesComponent 
        ref = {addImagesComponent}
        title = {title}
        addImgVisible = {addImgVisible}
        setAddImgVisible = {setAddImgVisible}
        imageArray = {imageArray}
        setImageArray = {setImageArray}
        thumbImageArray = {thumbImageArray}
        setThumbImageArray = {setThumbImageArray}
      />
    </>
  )
}

export default Add
