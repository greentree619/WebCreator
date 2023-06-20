import PropTypes from 'prop-types'
import React, { useEffect, useState, createRef, useRef } from 'react'
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
  CFormCheck,
  CImage,
  CModal,
  CModalBody,
  CModalTitle,
  CModalFooter,
  CModalHeader,
  CFormSwitch,
  CContainer,
  COffcanvas,
  COffcanvasHeader,
  CCloseButton,
  CCardTitle,
  CCardText,
  CNavLink,
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
import openAIImageGallery  from 'src/plugins/OpenAIImageGallery'
import openAIVideoLibrary  from 'src/plugins/OpenAIVideoLibrary'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { useDispatch, useSelector } from 'react-redux'
import {saveToLocalStorage, loadFromLocalStorage, clearLocalStorage, alertConfirmOption } from 'src/utility/common.js'
import AddImage from 'src/assets/images/AddImage.png'
import { render } from '@testing-library/react'
import { confirmAlert } from 'react-confirm-alert'; // Import
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css
import AddImagesComponent from '../common/AddImagesComponent'

const View = (props) => {
  const location = useLocation()
  const navigate = useNavigate()
  const activeProject = useSelector((state) => state.activeProject)
  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [title, setTitle] = useState('')
  const [metaTitle, setMetaTitle] = useState('')
  const [article, setArticle] = useState({})
  const [content, setContent] = useState('')
  const [footer, setFooter] = useState('')
  const [metaDescription, setMetaDescription] = useState('')
  const [metaKeywords, setMetaKeywords] = useState('')
  const [metaAuthor, setMetaAuthor] = useState('')
  const [pixabayURL, setPixabayURL] = useState(`https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=&image_type=photo&min_width=${process.env.REACT_APP_PIXABAY_MIN_WIDTH}&min_height=${process.env.REACT_APP_PIXABAY_MIN_HEIGHT}&per_page=100&page=1`)
  const [useTitleByBrandname, setUseTitleByBrandname] = useState(activeProject.contactInfo.useTitleByBrandname)
  const [addImgVisible, setAddImgVisible] = useState(false)
  const [imageGallery, setImageGallery] = useState([])
  const [imageArray, setImageArray] = useState([])
  const [thumbImageArray, setThumbImageArray] = useState([])
  const addImagesComponent = useRef()
  
  console.log(activeProject)

  useEffect(() => {
    const getFetch = async () => {
      const response = await fetch(
        `${process.env.REACT_APP_SERVER_URL}article/fromid/` + location.state.article.id,
      )
      const data = await response.json()
      //console.log(data)
      setTitle(data.data.title)
      if (data.data.metaTitle != null) setMetaTitle(data.data.metaTitle)
      if (data.data.metaDescription != null) setMetaDescription(data.data.metaDescription)
      if (data.data.metaKeywords != null) setMetaKeywords(data.data.metaKeywords)
      if (data.data.metaAuthor != null) setMetaAuthor(data.data.metaAuthor)
      if (data.data.content != null) setContent(data.data.content)
      if (data.data.footer != null) setFooter(data.data.footer)
      if (data.data.imageArray != null) setImageArray(data.data.imageArray)
      if (data.data.thumbImageArray != null) setThumbImageArray(data.data.thumbImageArray)

      if( useTitleByBrandname )
      {
        setMetaTitle(data.data.title + "-" + activeProject.contactInfo.brandname);
      }
      setArticle(data.data)

      let q = data.data.title.replaceAll(' ', '+').replaceAll('?', '')
      console.log(data.data.title, q)
      let openAIKeyword = data.data.title.replaceAll('?', '')
      console.log(data.data.title, openAIKeyword)
      setPixabayURL()
      footEditor.current.core.options.imageGalleryLoadURL = 'https://pixabay.com/api/?key=27944002-ca9bbda02c769f32ad5769e81&q=' + q + `&image_type=photo&min_width=${process.env.REACT_APP_PIXABAY_MIN_WIDTH}&min_height=${process.env.REACT_APP_PIXABAY_MIN_HEIGHT}&per_page=100&page=1`
      bodyEditor.current.core.options.imageGalleryLoadURL = 'https://pixabay.com/api/?key=27944002-ca9bbda02c769f32ad5769e81&q=' + q + `&image_type=photo&min_width=${process.env.REACT_APP_PIXABAY_MIN_WIDTH}&min_height=${process.env.REACT_APP_PIXABAY_MIN_HEIGHT}&per_page=100&page=1`
      footEditor.current.core.options.openAIImageLoadURL = `${process.env.REACT_APP_SERVER_URL}openAI/image/10?prompt=${openAIKeyword}`;
      bodyEditor.current.core.options.openAIImageLoadURL = `${process.env.REACT_APP_SERVER_URL}openAI/image/10?prompt=${openAIKeyword}`;
      footEditor.current.core.options.openAIVideoLoadURL = `${process.env.REACT_APP_SERVER_URL}openAI/video/10?prompt=${openAIKeyword}`;
      bodyEditor.current.core.options.openAIVideoLoadURL = `${process.env.REACT_APP_SERVER_URL}openAI/video/10?prompt=${openAIKeyword}`;
      //console.log(footEditor.current.core.options.imageGalleryLoadURL)
    }
    getFetch()
  }, [])

  const updateContent = async () => {
    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: location.state.article.id,
        metaDescription: metaDescription,
        metaKeywords: metaKeywords,
        metaAuthor: metaAuthor,
        content: content,
        metaTitle: (metaTitle.length == 0 ? title : metaTitle),
        footer: footer,
        state: article.state,
        imageArray: imageArray,
        thumbImageArray: thumbImageArray,
      }),
    }

    console.log(location.state.projectInfo);

    var s3Host = loadFromLocalStorage('s3host')
    var s3Name = s3Host.name == null ? "" : s3Host.name;
    var s3Region = s3Host.region == null ? "" : s3Host.region;
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}article/update_content/${location.state.projectInfo.projectid}/${location.state.projectInfo.domainName}/${location.state.projectInfo.domainIp}?s3Name=${s3Name}&region=${s3Region}`, requestOptions)
    // setAlertColor('danger')
    // setAlertMsg('Faild to update content unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret) {
      // setAlertMsg('Article content is updated successfully.')
      // setAlertColor('success')
      toast.success('Article content is updated successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Faild to update content unfortunatley.', alertConfirmOption);
    }
    // setAlarmVisible(true)
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
        <CCardHeader>Article View</CCardHeader>
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
              <CRow>
                <CCol>
                  <CFormLabel htmlFor="metaTitle">Meta Title</CFormLabel>
                  <CFormInput
                    type="text"
                    id="metaTitle"
                    aria-label="metaTitle"
                    value={metaTitle}
                    onChange={(e) => setMetaTitle(e.target.value)}
                  />
                </CCol>
                <CCol>
                  <CFormCheck id="useMetaTitle" 
                    checked={useTitleByBrandname}
                    className="pt-5" 
                    label="Use Mete Title Format with <Title>-<Brand Name>"
                    onChange={(e) => {
                      setUseTitleByBrandname(e.target.checked)
                      if( e.target.checked )
                      {
                        setMetaTitle(title + "-" + activeProject.contactInfo.brandname);
                      }
                      else
                      {
                        setMetaTitle('');
                      }
                    }}
                  />
                </CCol>
              </CRow>
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
                    plugins: {...plugins, pixabayImageGallery, openAIImageGallery, openAIVideoLibrary},
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
                        ['pixabayImageGallery', 'openAIImageGallery'/*, 'openAIVideoLibrary'*/], // You must add the "imageGalleryUrl".
                        ['fullScreen', 'showBlocks', 'codeView'],
                        ['preview', 'print'],
                        ['save', 'template'],
                        /** ['dir', 'dir_ltr', 'dir_rtl'] */ // "dir": Toggle text direction, "dir_ltr": Right to Left, "dir_rtl": Left to Right
                    ]
              }}/>
            </div>
            <div className="mb-3">
              <CFormLabel htmlFor="exampleFormControlInput2">Footer</CFormLabel>
              <SunEditor 
                getSunEditorInstance={getFooterSunEditorInstance}
                setContents={footer} 
                onChange={(contents, core) => {
                  setFooter(contents)
                  //console.log({ event, editor, data })
                }}
                setOptions={{
                    height: 200,
                    plugins: {...plugins, pixabayImageGallery, openAIImageGallery, openAIVideoLibrary},
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
                        ['pixabayImageGallery', 'openAIImageGallery'/*, 'openAIVideoLibrary'*/], // You must add the "imageGalleryUrl".
                        ['fullScreen', 'showBlocks', 'codeView'],
                        ['preview', 'print'],
                        ['save', 'template'],
                        /** ['dir', 'dir_ltr', 'dir_rtl'] */ // "dir": Toggle text direction, "dir_ltr": Right to Left, "dir_rtl": Left to Right
                    ]
              }}/>
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
                        &nbsp;<CImage key={"thumb"+idx} onClick={() => deleteImageConfirm(idx)} align="start" rounded src={((img.substr(0, 10) == "data:image" || img.substr(0, 6) == "https:") ? img : ("data:image/jpeg;base64," + img))} width={80} height={80} />
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
              <CButton type="button" onClick={() => updateContent()}>
                Update
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

export default View
