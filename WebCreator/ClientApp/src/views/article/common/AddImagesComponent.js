import PropTypes from 'prop-types'
import React, { useEffect, useState, useImperativeHandle, forwardRef, createRef, useRef } from 'react'
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
  CSpinner,
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

const AddImagesComponent = forwardRef((props, ref) => {
  const [imageGallery, setImageGallery] = useState([])
  const [searchPixabay, setSearchPixabay] = useState(true)
  const [searchKeyword, setSearchKeyword] = useState('')
  const [searchOn, setSearchOn] = useState(false)

  useEffect(() => {
  }, [])

  const attachImageGallery = async ( isPixabay, keyword ) => {
    console.log("attachImageGallery=>", isPixabay, keyword)
    
    setSearchOn(true)
    setImageGallery([])
    //if(keyword.length == 0) return
    if(isPixabay)
    {
      const requestOptions = {
        method: 'GET',
      }
      const response = await fetch(`https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=${keyword}&image_type=photo&min_width=480&min_height=600&per_page=100&page=1`, requestOptions)
      let ret = await response.json()
      if (response.status === 200 && ret) {
        //console.log(ret.hits[0])
        var tmpGallery = []
        ret.hits.map((img, idx) => {
          tmpGallery.push({url:img.largeImageURL
            , thumb:img.previewURL})          
        })
        setImageGallery(tmpGallery)
        //console.log(imageGallery)
      }
      setSearchOn(false)
    }
    else
    {
      const requestOptions = {
        method: 'GET',
      }
      const response = await fetch(`${process.env.REACT_APP_SERVER_URL}openAI/image/10?prompt=${keyword}`, requestOptions)
      let ret = await response.json()
      if (response.status === 200 && ret) {
        //console.log(ret)
        var tmpGallery = []
        ret.data.map((img, idx) => {
          tmpGallery.push({url:img.url
            , thumb:"data:image/jpeg;base64," + img.thumb})
        })
        setImageGallery(tmpGallery)
        //console.log(imageGallery)
        setSearchOn(false)
      }
    }
  }

  useImperativeHandle(ref, () => ({
    showAddImageModal()
    {
      props.setAddImgVisible(true)
      //setSearchPixabay(true)
      //setSearchKeyword("")
      //attachImageGallery(searchPixabay, searchKeyword)
    }
  }));

  const generateImagesFromTitle = async () => {
    setSearchOn(true)
    var query = props.title.replace("?", "").replace(" ", "+")
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/translateKeyword?keyword=${query}`,
    )
    const data = await response.json()
    if (response.status === 200 && data) {
      query = data.data
    }
    setSearchKeyword(query)
    console.log(searchKeyword)
    attachImageGallery(searchPixabay, query)
  }

  const addImageArray = (url, thumb) => {
    var tmpimgAry = props.imageArray
    var tmpThumImgAry = props.thumbImageArray
    tmpimgAry.push(url)
    tmpThumImgAry.push(thumb)
    props.setImageArray(tmpimgAry)
    props.setThumbImageArray(tmpThumImgAry)
    props.setAddImgVisible(false)
  }

  return (
    <>
      <CModal scrollable size="xl" visible={props.addImgVisible} 
        onClose={() => props.setAddImgVisible(false)}>
        <CModalHeader onClose={() => props.setAddImgVisible(false)}>
          <CRow className='col-12'>
            <CCol xs={1} className="d-flex justify-content-center">
              {searchOn ? <CSpinner size={"md"}/> : ""}
            </CCol>
            <CCol xs={3} className="d-flex justify-content-center">
              <CFormSwitch value={searchPixabay} onChange={(e)=>setSearchPixabay(!e.target.checked)} label="From Pixabay/OpenAI images" id="pixabayOrOpenAI"/>
            </CCol>
            <CCol xs={4} className="d-flex justify-content-center">
              <CFormInput type="text" value={searchKeyword} 
              onChange={(e) => setSearchKeyword(e.target.value)}
              placeholder="Search Keyword" aria-label="Search Keyword"/>
              <CButton color="primary" onClick={()=>attachImageGallery(searchPixabay, searchKeyword)}>Search</CButton>
            </CCol>
            <CCol xs={4} className="d-flex justify-content-center">
              <CButton color="dark" onClick={()=>generateImagesFromTitle()}>Generate Images From Title</CButton>
            </CCol>
          </CRow>
        </CModalHeader>
        <CModalBody>
          <div className="clearfix">
            {imageGallery.map((img, idx) => {
              //console.log(img.thumb)
              return <div key={idx}>
                <CImage onClick={()=>addImageArray(img.url, img.thumb)}  align="start" className='p-1' rounded src={img.thumb} width={150} height={150} />
              </div>
            })}
          </div>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => props.setAddImgVisible(false)}>
            Close
          </CButton>
          {/* <CButton color="primary">Select</CButton> */}
        </CModalFooter>
      </CModal>
    </>
  )
})

AddImagesComponent.displayName = 'AddImagesComponent';

AddImagesComponent.propTypes = {
  title : PropTypes.string,
  addImgVisible : PropTypes.bool,
  setAddImgVisible : PropTypes.func,
  imageArray : PropTypes.array,
  setImageArray : PropTypes.func,
  thumbImageArray : PropTypes.array,
  setThumbImageArray : PropTypes.func,
}

export default AddImagesComponent
