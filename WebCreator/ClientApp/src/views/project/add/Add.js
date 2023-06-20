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
  CContainer,
  CSpinner,
  CFormSelect,
  CFormCheck
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { Outlet, Link } from 'react-router-dom'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import {saveToLocalStorage, globalRegionMap, loadFromLocalStorage, clearLocalStorage, alertConfirmOption } from 'src/utility/common'
import {languageMap} from 'src/utility/LanguageCode'
import {countryMap} from 'src/utility/CountryCode'
import { ReactSession }  from 'react-client-session'

const Add = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  const navigate = useNavigate()

  const [simpleMode, setSimpleMode] = useState(
    location.state != null && location.state.simple_mode != null
      ? location.state.simple_mode
      : false,
  )

  const activeProject = useSelector((state) => state.activeProject)
  if (location.search.length > 0) {
    //console.log()
    const linkMode = new URLSearchParams(location.search).get('mode')
    if (linkMode == 'view') {
      location.state = { project: activeProject, mode: 'VIEW' }
    }
  }

  const [validated, setValidated] = useState(false)
  const [useHttps, setUseHttps] = useState(
    location.state != null && !simpleMode ? location.state.project.useHttps : false,
  )
  const [projectName, setProjectName] = useState(
    location.state != null && !simpleMode ? location.state.project.name : '',
  )
  const [ipAddress, setIpAddress] = useState(
    location.state != null && !simpleMode ? location.state.project.ip : '0.0.0.0',
  )
  const [s3BucketName, setS3BucketName] = useState(
    location.state != null && !simpleMode ? location.state.project.s3BucketName : '',
  )
  const [s3BucketRegion, setS3BucketRegion] = useState(
    location.state != null && !simpleMode ? location.state.project.s3BucketRegion : '',
  )
  const [searchKeyword, setSearchKeyword] = useState(
    location.state != null && !simpleMode ? location.state.project.keyword : '',
  )
  const [questionsCount, setQuestionsCount] = useState(
    location.state != null && !simpleMode ? location.state.project.quesionsCount : 50,
  )
  const [brandName, setBrandName] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.brandname : '',
  )
  const [useTitleByBrandname, setUseTitleByBrandname] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.useTitleByBrandname : false,
  )
  const [streetAddress, setStreetAddress] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.streetAddress : '',
  )
  const [adrdressLocality, setAddressLocality] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.adrdressLocality : '',
  )
  const [addressRegion, setAddressRegion] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.addressRegion : '',
  )
  const [postalCode, setPostalCode] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.postalCode : '',
  )
  const [country, setCountry] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.country : '',
  )
  const [phone, setPhone] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.phone : '',
  )
  const [website, setWebsite] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.website : '',
  )
  const [descriptionOfCompany, setDescriptionOfCompany] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.descriptionOfCompany : '',
  )
  const [openingHours, setOpeningHours] = useState(
    location.state != null && !simpleMode ? location.state.project.contactInfo.openingHours : '',
  )

  const [scrappingFrom, setScrappingFrom] = useState(
    location.state != null && !simpleMode ? location.state.project.imageAutoGenInfo.scrappingFrom : 0,
  )

  const [imageNumber, setImageNumber] = useState(
    location.state != null && !simpleMode ? location.state.project.imageAutoGenInfo.imageNumber : 0,
  )

  const [insteadOfTitle, setInsteadOfTitle] = useState(
    location.state != null && !simpleMode ? location.state.project.imageAutoGenInfo.insteadOfTitle : '',
  )

  const [alarmVisible, setAlarmVisible] = useState(false)
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [mode, setMode] = useState('ADD')
  const [s3BucketList, setS3BucketList] = useState([])
  const [language, setLanguage] = useState(
    location.state && !simpleMode ? location.state.project.languageString : 'Engllish',
  )
  const [languageValue, setLanguageValue] = useState(
    location.state && !simpleMode ? location.state.project.language : 'en',
  )
  const [isOnScrapping, setIsOnScrapping] = useState(false)
  const [isOnAFScrapping, setIsOnAFScrapping] = useState(false)
  const [isOnPublish, setIsOnPublish] = useState(false)
  const [disabledUpdate, setDisabledUpdate] = useState(false)
  const [ipAddressMap, setIpAddressMap] = useState([
    { ip: 'AWS S3 Bucket', value: '0.0.0.0' },
    ])
  // let ipAddressMap = [
  //   { ip: 'AWS S3 Bucket', value: '0.0.0.0' },
  //   { ip: '3.14.14.86', value: '3.14.14.86' },
  //   { ip: '3.131.110.136', value: '3.131.110.136' },
  //   { ip: '3.142.69.221', value: '3.142.69.221' },
  // ]

  const handleSubmit = (event) => {
    const form = event.currentTarget
    event.preventDefault()

    if (location.state != null && !simpleMode && location.state.mode == 'EDIT') {
      postAddProject()
    } else if (location.state != null && !simpleMode && location.state.mode == 'VIEW') {
      navigate(-1)
    } else {
      if (form.checkValidity() === false) {
        event.stopPropagation()
      } else {
        postAddProject()
      }
      setValidated(true)
    }
  }

  const inputChangeHandler = (setFunction, event) => {
    setFunction(event.target.value)
  }

  async function postAddProject() {
    var projInfo = {
      id: location.state && !simpleMode ? location.state.project.id : '-1',
      name: projectName,
      ip: ipAddress,
      useHttps: useHttps,
      s3BucketName: s3BucketName,
      s3BucketRegion: s3BucketRegion,
      keyword: searchKeyword,
      quesionscount: questionsCount,
      language: languageValue,
      languageString: language,
      contactInfo:{        
        brandname: brandName,
        useTitleByBrandname: useTitleByBrandname,
        streetAddress: streetAddress,
        adrdressLocality: adrdressLocality,
        addressRegion: addressRegion,
        postalCode: postalCode,
        country: country,
        phone: phone,
        website: website,
        descriptionOfCompany: descriptionOfCompany,
        openingHours: openingHours,
      },
      imageAutoGenInfo:{
        scrappingFrom: scrappingFrom,
        imageNumber: imageNumber,
        insteadOfTitle: insteadOfTitle,
      },
    }

    const requestOptions = {
      method: location.state && !simpleMode ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(projInfo),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project`, requestOptions)
    // setAlertColor('danger')
    // setAlertMsg('Faild to create/update new domain unfortunatley.')
    let ret = await response.json()
    if (response.status === 200 && ret.result) {
      // setAlertMsg('Created/Updated new domain successfully.')
      // setAlertColor('success')
      toast.success('Created/Updated new domain successfully.', alertConfirmOption);

      console.log("ret.error", ret.error)
      if (ret.error.length > 0) {
        dispatch({ type: 'set', notification: [ret.error] })
      }

      location.state.project = projInfo
      dispatch({ type: 'set', activeDomainName: location.state.project.name })
      dispatch({ type: 'set', activeDomainIp: location.state.project.ip })
      dispatch({ type: 'set', activeProject: location.state.project })
      saveToLocalStorage({name: location.state.project.s3BucketName, region: location.state.project.s3BucketRegion}, 's3host')

      if (location.state && !simpleMode)
      {//Because update, update redux project array.
        var allProjects = loadFromLocalStorage('allProjects')
        if(allProjects != null && allProjects != undefined)
        {
          let tmpProjects = [...allProjects]
          let idx = tmpProjects.findIndex((pro) => pro.id === projInfo.id)
          tmpProjects[idx] = projInfo//tmpProjects.splice(idx, 1)
          saveToLocalStorage(tmpProjects, 'allProjects')
        }
      }

      if (simpleMode)
      {
        ReactSession.set("allProjects", "0")
        clearLocalStorage('allProjects')
        navigate('/dashboard')
      }
    }
    else {
      toast.error('Faild to create/update new domain unfortunatley.', alertConfirmOption);
    }
    //setAlarmVisible(true)
  }

  const handleClick = (lang, value) => {
    setLanguageValue(value)
    setLanguage(lang)
    //console.log('clicked ' + i + ', state.value = ' + languageValue)
  }

  const renderItem = (lang, value) => {
    return (
      <CDropdownItem key={value} onClick={() => handleClick(lang, value)}>
        {lang}
      </CDropdownItem>
    )
  }

  const handleIpAddrClick = (ipAddr) => {
    setIpAddress(ipAddr)
    if(ipAddr == "0.0.0.0") setUseHttps( true )
    //console.log('clicked ' + i + ', state.value = ' + languageValue)
  }

  const renderIpAddrItem = (ipaddr) => {
    return (
      <CDropdownItem key={ipaddr.value} onClick={() => handleIpAddrClick(ipaddr.value)}>
        {ipaddr.ip}
      </CDropdownItem>
    )
  }

  let ActionMode = 'Create'
  if (location.state != null && !simpleMode && location.state.mode == 'EDIT') {
    ActionMode = 'Update'
  } else if (location.state != null && !simpleMode && location.state.mode == 'VIEW') {
    ActionMode = 'Back'
  }

  async function scrapQuery(_id, keyword, count) {
    keyword = keyword.replaceAll(';', '&')
    keyword = keyword.replaceAll('?', ';')
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}project/serpapi/` + _id + '/' + keyword + '/' + count,
    )
    // setAlarmVisible(false)
    // setAlertMsg('Unfortunately, scrapping faild.')
    // setAlertColor('danger')
    if (response.status === 200) {
      //console.log('add success')
      // setAlertMsg('Completed to scrapping questions from google successfully.')
      // setAlertColor('success')
      toast.success('Completed to scrapping questions from google successfully.', alertConfirmOption);
    }
    else {
      toast.error('Unfortunately, scrapping faild.', alertConfirmOption);
    }
    // setAlarmVisible(true)
  }

  const renderLanguageItem = () => {
    //if(location.state != null )
    return 'English'
  }

  async function getZoneInformation() {
    try {
      if (location.state != null && location.state.project != null
        && (location.state.mode == 'VIEW' || location.state.mode == 'EDIT')) {
        const response = await fetch(`${process.env.REACT_APP_SERVER_URL}dns/byname/${location.state.project.name}`)
        const data = await response.json()
        console.log("getZoneInformation", data.result);
        if (data.result.length > 0) {
          //console.log(data.result[0].name);
          //console.log(data.result[0].id);
          dispatch({ type: 'set', activeZoneName: data.result[0].name })
          dispatch({ type: 'set', activeZoneId: data.result[0].id })
          dispatch({ type: 'set', activeZoneStatus: data.result[0].status })
        }
        else
        {
          dispatch({ type: 'set', activeZoneName: "" })
          dispatch({ type: 'set', activeZoneId: "" })
          dispatch({ type: 'set', activeZoneStatus: (location.state.project.ip === "0.0.0.0" ? "active": "") })
        }
      }
    } catch (e) {
      console.log(e);
    }
  }

  useEffect(() => {
    //   async function loadScrappingStatus()  {
    //     try {
    //      if(location.state != null && location.state.project != null 
    //        && (location.state.mode == 'VIEW' || location.state.mode == 'EDIT')){
    //        const requestOptions = {
    //          method: 'GET',
    //          headers: { 'Content-Type': 'application/json' },
    //        }

    //        const response = await fetch(`${process.env.REACT_APP_SERVER_URL}project/isscrapping/${location.state.project.id}`, requestOptions)
    //        let ret = await response.json()
    //        if (response.status === 200 && ret) {
    //          //console.log(ret);
    //          setIsOnScrapping(ret.serpapi);
    //          setIsOnAFScrapping(ret.afapi);
    //        }
    //      }
    //    } catch (e) {
    //        //console.log(e);
    //        setIsOnScrapping(false);
    //        setIsOnAFScrapping(false);
    //    }
    //  }
    //  var refreshIntervalId = setInterval(loadScrappingStatus, 1000);
    
    populateEC2IPList()
    populateBucketNameList()

    //Omitted console.log("location.search.length = " + location.search.length, location.state.project)
    dispatch({ type: 'set', activeTab: "project_add" })
    if (location.search.length == 0
      && (location.state != null && (location.state.mode == 'VIEW' || location.state.mode == 'EDIT'))) {
      //normal link
      if (location.state != null && !simpleMode) {
        dispatch({ type: 'set', activeDomainName: location.state.project.name })
        dispatch({ type: 'set', activeDomainId: location.state.project.id })
        dispatch({ type: 'set', activeProject: location.state.project })
        dispatch({ type: 'set', activeDomainIp: location.state.project.ip })
        saveToLocalStorage({name: location.state.project.s3BucketName, region: location.state.project.s3BucketRegion}, 's3host')
        // var s3Host = loadFromLocalStorage('s3host')
        // console.log('s3Host=>', s3Host)
      } else {
        dispatch({ type: 'set', activeDomainName: '', activeProject: {}, activeDomainId: '' })
      }
    }

    getZoneInformation();
    return () => {
      //unmount
      // clearInterval(refreshIntervalId);
      console.log('project scrapping status interval cleared!!!');
    }
  }, [])

  const populateBucketNameList = async () => {
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}s3Bucket/nameList`)    
    const data = await response.json()
    if (response.status === 200) {
      setS3BucketList(data.result);
    }
  }

  const populateEC2IPList = async () => {
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}EC2IPAddress/`,
    )
    const data = await response.json()
    var tmpMap = [{ ip: 'AWS S3 Bucket', value: '0.0.0.0' },]
    await data.data.map((item, index) => {
      var tmp2Map = [...tmpMap, { ip: item.ipAddress, value: item.ipAddress }]
      tmpMap = tmp2Map
      //console.log(ids, "<--", articleDocumentIds);
    });
    setIpAddressMap(tmpMap)
  }

  const readKeyFile = async (e) => {
    e.preventDefault()
    const reader = new FileReader()
    reader.onload = async (e) => {
      const text = (e.target.result)
      //console.log(text)
      //alert(text.replaceAll('\r\n', ';'))
      var tmpKeyword = text.replaceAll('\r\n', ';')
      if (tmpKeyword[tmpKeyword.length - 1] == ';') tmpKeyword = tmpKeyword.substring(0, tmpKeyword.length - 1)
      setSearchKeyword(tmpKeyword);
    };
    reader.readAsText(e.target.files[0])
  }

  const onChangedBucketName = async ( name ) => {
    setS3BucketName(name)
    setDisabledUpdate(true)
    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}s3Bucket/getRegion/${name}`)
    const data = await response.json()
    if (response.status === 200) {
      setS3BucketRegion(data.result)
    }
    setDisabledUpdate(false)
  }

  return (
    <>
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
      <CContainer className="px-4">
        <CRow xs={{ gutterX: 5 }}>
          <CCol>
            <CCard className="mb-4">
              <CCardHeader>New/Update Domain</CCardHeader>
              <CCardBody>
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
                  validated={validated}
                  onSubmit={handleSubmit}
                >
                  <CRow>
                    <CCol>
                      <div className="mb-3">
                        <CRow>
                          <CCol>
                            <CRow>
                              <CCol>
                                <CFormLabel htmlFor="projectNameFormControlInput">
                                  Domain
                                </CFormLabel>
                              </CCol>
                              <CCol className={simpleMode ? 'd-none' : 'mb-3'}>
                                <CFormCheck id="useHttps" 
                                  checked={useHttps} 
                                  onChange={() => setUseHttps(!useHttps)}
                                  label="Enable https"
                                  disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                                />
                              </CCol>
                            </CRow>
                            <CFormInput
                              type="text"
                              id="projectNameFormControlInput"
                              placeholder="www.domain.com"
                              aria-label="Domain"
                              required
                              onChange={(e) => inputChangeHandler(setProjectName, e)}
                              disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                              value={projectName}
                            />
                          </CCol>
                        </CRow>
                      </div>
                      <div className={simpleMode ? 'd-none' : 'mb-3'}>
                        <CRow>
                          <CFormLabel htmlFor="ipSelect" className="col-sm-4 col-form-label">IP Address</CFormLabel>
                          <CCol sm={8} >
                            <CFormSelect id="ipSelect" value={ipAddress} 
                              onChange={(obj) => handleIpAddrClick(obj.target.value)} size="sm" 
                              className="mb-3" aria-label="Small select example"
                              disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}>
                              {
                                ipAddressMap.map((ipAddr, index) => {
                                    return (<option key={index} value={ipAddr.value}>{ipAddr.ip}</option>)
                                  })
                              }
                            </CFormSelect>
                          </CCol>
                        </CRow>
                      </div>
                      {ipAddress === "0.0.0.0" && (
                        <div className={simpleMode ? 'd-none' : 'mb-3'}>
                          <CRow>
                            <CCol className='mb-8'>
                              <CFormSelect id="bucketSelect" value={s3BucketName} 
                                onChange={(obj) => onChangedBucketName(obj.target.value)} 
                                size="sm" className="mb-3" aria-label="Select Bucket"
                                disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}>
                                  <option value="-">No selected</option>
                                {
                                  s3BucketList.map((bucketItem, index) => {
                                      return (<option key={index} value={bucketItem.name}>{bucketItem.name}</option>)
                                    })
                                }
                              </CFormSelect>
                            </CCol>
                            <CCol className='mb-4'>
                              <CFormSelect id="regionSelect" value={s3BucketRegion} 
                                onChange={(obj) => setS3BucketRegion(obj.target.value)} 
                                size="sm" className="mb-3" aria-label="Select Region"
                                disabled={true}>
                                {
                                  globalRegionMap.map((regionItem, index) => {
                                      return (<option key={index} value={regionItem.value}>{regionItem.region}</option>)
                                    })
                                }
                              </CFormSelect>
                            </CCol>
                          </CRow>
                        </div>
                      )}
                      <div className={simpleMode ? 'd-none' : 'd-none'}>
                        <CFormLabel htmlFor="exampleFormControlInput1">
                          Search Keyword(can use multiple keywords using &apos;;&apos;)
                        </CFormLabel>
                        <CRow>
                          <CCol className='col-9'>
                            <CFormInput
                              type="text"
                              id="searchKeywordFormControlInput"
                              placeholder="Search Keyword"
                              aria-label="Search Keyword"
                              onChange={(e) => inputChangeHandler(setSearchKeyword, e)}
                              disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                              value={searchKeyword}
                            />
                          </CCol>
                          <CCol>
                            <CFormInput type="file"
                              id="formFile"
                              onChange={(e) => readKeyFile(e)}
                              disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'} />
                          </CCol>
                        </CRow>
                      </div>
                      <div className={simpleMode ? 'd-none' : 'd-none'}>
                        <CFormLabel htmlFor="exampleFormControlInput1">Questions Count</CFormLabel>
                        <CFormInput
                          type="text"
                          id="questionsCountFormControlInput"
                          placeholder="50"
                          onChange={(e) => inputChangeHandler(setQuestionsCount, e)}
                          required={simpleMode ? false : true}
                          disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                          value={questionsCount}
                        />
                      </div>
                      <div className={simpleMode ? 'd-none' : 'mb-3'}>
                        <CRow>
                          <CFormLabel htmlFor="exampleFormControlInput1" className="col-sm-4 col-form-label">Language({languageValue})</CFormLabel>
                          <CCol sm={8}>
                            <CDropdown
                              id="axes-dd"
                              className="float-right mr-0"
                              size="sm"
                              disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                            >
                              <CDropdownToggle
                                id="axes-ddt"
                                color="secondary"
                                size="sm"
                                disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                              >
                                {language}
                              </CDropdownToggle>
                              <CDropdownMenu>
                                {languageMap.map((langInfo, index) => {
                                  return renderItem(langInfo.lang, langInfo.value)
                                })}
                              </CDropdownMenu>
                            </CDropdown>
                          </CCol>
                          {/* <CCol>
                            <CFormSelect id="countrySelect"
                              //onChange={(obj) => setS3BucketRegion(obj.target.value)} 
                              size="sm" className="mb-3" aria-label="Country">
                              {
                                countryMap.map((countryItem, index) => {
                                    return (<option key={index} value={countryItem.value}>{countryItem.country}</option>)
                                  })
                              }
                            </CFormSelect>
                          </CCol> */}
                        </CRow>
                      </div>
                      <CRow className={simpleMode ? 'd-none' : 'mb-3 py-0'}>
                        <CFormLabel htmlFor="Brandname" className="col-sm-4 col-form-label">Brand Name</CFormLabel>
                        <CCol sm={8} className={"d-flex align-items-center"}>
                          <CFormInput type="text" id="Brandname" value={brandName} onChange={(e) => inputChangeHandler(setBrandName, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                        <CCol className={"d-flex justify-content-end"}>
                            <CFormCheck id="useMetaTitle" 
                              checked={useTitleByBrandname} 
                              onChange={() => setUseTitleByBrandname(!useTitleByBrandname)}
                              label="Use Title Format with 'Title-Brand Name'"
                              disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}
                            />
                        </CCol>
                      </CRow>
                      <CRow className={simpleMode ? 'd-none' : 'mb-3 py-0'}>
                        <CFormLabel htmlFor="StreetAddress" className="col-sm-4 col-form-label">Street Address</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="StreetAddress" value={streetAddress} onChange={(e) => inputChangeHandler(setStreetAddress, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                      <CRow className={simpleMode ? 'd-none' : 'mb-3 py-0'}>
                        <CFormLabel htmlFor="AdrdressLocality" className="col-sm-4 col-form-label">Adrdress Locality</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="AdrdressLocality" value={adrdressLocality} onChange={(e) => inputChangeHandler(setAddressLocality, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                      <CRow className={simpleMode ? 'd-none' : 'mb-3 py-0'}>
                        <CFormLabel htmlFor="AddressRegion" className="col-sm-4 col-form-label">Address Region</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="AddressRegion" value={addressRegion} onChange={(e) => inputChangeHandler(setAddressRegion, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                    </CCol>
                    <CCol className={simpleMode ? 'd-none' : ''}>
                      <CRow className="mb-3 pt-4">
                        <CFormLabel htmlFor="PostalCode" className="col-sm-4 col-form-label">Postal Code</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="PostalCode" value={postalCode} onChange={(e) => inputChangeHandler(setPostalCode, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                      <CRow className="mb-3 py-0">
                        <CFormLabel htmlFor="Country" className="col-sm-4 col-form-label">Country</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="Country" value={country} onChange={(e) => inputChangeHandler(setCountry, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                      <CRow className="mb-3 py-0">
                        <CFormLabel htmlFor="Phone" className="col-sm-4 col-form-label">Phone</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="Phone" value={phone} onChange={(e) => inputChangeHandler(setPhone, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                      <CRow className="mb-3 py-0">
                        <CFormLabel htmlFor="Website" className="col-sm-4 col-form-label">Website</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="Website" value={website} onChange={(e) => inputChangeHandler(setWebsite, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                      <CRow className="mb-3 py-0">
                        <CFormLabel htmlFor="DescriptionofCompany" className="col-sm-4 col-form-label">Description of Company</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="DescriptionofCompany" value={descriptionOfCompany} onChange={(e) => inputChangeHandler(setDescriptionOfCompany, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                      <CRow className="mb-3 py-0">
                        <CFormLabel htmlFor="OpeningHours" className="col-sm-4 col-form-label">Opening Hours</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="OpeningHours" value={openingHours} onChange={(e) => inputChangeHandler(setOpeningHours, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                      <CRow className="mb-3 py-0">
                        <CFormLabel htmlFor="ArticleImageNumber" className="col-sm-4 col-form-label">Article Image Number</CFormLabel>
                        <CCol sm={4}>
                          <CFormInput type="number" id="ArticleImageNumber" value={imageNumber} onChange={(e) => inputChangeHandler(setImageNumber, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                        <CCol sm={4}>
                          <CFormSelect id="sourceSelect" value={scrappingFrom} 
                            onChange={(obj) => setScrappingFrom(Number(obj.target.value))} size="sm" 
                            className="mb-3" aria-label="Small select example"
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}>
                            <option value='0'>Pixabay</option>
                            <option value='1'>OpenAI</option>
                            <option value='2'>Pixabay & OpenAI</option>
                          </CFormSelect>
                        </CCol>
                      </CRow>
                      <CRow className="mb-3 py-0">
                        <CFormLabel htmlFor="insteadOfTitle" className="col-sm-4 col-form-label">Article Image Keyword Instead of Title</CFormLabel>
                        <CCol sm={8}>
                          <CFormInput type="text" id="insteadOfTitle" value={insteadOfTitle} placeholder='Split each keyword by ";"'
                            onChange={(e) => inputChangeHandler(setInsteadOfTitle, e)}
                            disabled={location.state != null && !simpleMode && location.state.mode == 'VIEW'}/>
                        </CCol>
                      </CRow>
                    </CCol>
                  </CRow>

                  <div className="mb-3">
                    {location.state != null && !simpleMode && (
                      <CButton type="button" onClick={() => navigate('/project/add')}>
                        New Domain
                      </CButton>
                    )}
                    &nbsp;
                    {location.state != null && !simpleMode && location.state.mode == 'VIEW' && (
                      <>
                        <CButton
                          type="button"
                          onClick={() =>
                            scrapQuery(
                              location.state.project.id,
                              location.state.project.keyword,
                              location.state.project.quesionsCount,
                            )
                          }
                        >
                          Scrap
                        </CButton>
                        &nbsp;
                        <Link
                          to={`/project/add`}
                          state={{ mode: 'EDIT', project: location.state.project }}
                        >
                          <CButton type="button">Edit</CButton>
                        </Link>
                      </>
                    )}
                    &nbsp;
                    <CButton type="submit" disabled={disabledUpdate}>{ActionMode}</CButton>
                  </div>
                </CForm>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      </CContainer>
    </>
  )
}

export default Add
