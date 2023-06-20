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
  CFormCheck,
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate, Link } from 'react-router-dom'
import { CKEditor } from '@ckeditor/ckeditor5-react'
import ClassicEditor from '@ckeditor/ckeditor5-build-classic'
import { Col } from 'reactstrap'
import { useDispatch, useSelector } from 'react-redux'
import { ToastContainer, toast } from 'react-toastify'
import 'react-toastify/dist/ReactToastify.css'
import {globalRegionMap, alertConfirmOption} from 'src/utility/common.js'
import {saveToLocalStorage, loadFromLocalStorage, clearLocalStorage } from 'src/utility/common.js'
import { confirmAlert } from 'react-confirm-alert';
import 'react-confirm-alert/src/react-confirm-alert.css'; 

const Bucket = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [checkedItem, setCheckedItem] = useState({})
  const [allBuckets, setAllBuckets] = useState([])
  const [bucketName, setBucketName] = useState("")
  const [region, setRegion] = useState("")
  
  let regionMap = globalRegionMap

  useEffect(() => {
    populateData()
  }, [])

  const populateData = async () => {
    var store = loadFromLocalStorage('s3Buckets');
    if(store != null && store != undefined)
    {
      console.log(store)
      setAllBuckets(store.allBuckets)
      clearLocalStorage('s3Buckets')
      return
    }
    setLoading(true);
    setCheckedItem({});
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}s3Bucket/`,
    )
    const data = await response.json()
    //console.log("<--", data);
    await data.result.map((item, index) => {
      var ret = checkedItem
      ret[String(item.name)] = {checked: false, index: index}
      setCheckedItem(ret)
      //console.log(ids, "<--", articleDocumentIds);
    });

    setLoading(false)
    setAllBuckets(data.result)
  }

  const refreshBucket = async() => {
    populateData()
  }

  const deleteBucket = async (bucketName, reg) => {
    const requestOptions = {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' }
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}s3Bucket/${bucketName}/${reg}`, requestOptions)
    let ret = await response.json()
    if (response.status === 200 && ret.result) {
      toast.success(`Deleted bucket-${bucketName} successfully.`, alertConfirmOption);
    }
    else
    {
      toast.error(`Can\'t delete bucket -${bucketName}, unfortunatley. Maybe bucket can conaints some files.`, alertConfirmOption);
    }
  }

  const emptyBucket = async (bucketName, reg) => {
    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}s3Bucket/empty/${bucketName}/${reg}`, requestOptions)
    let ret = await response.json()
    if (response.status === 200 && ret.result) {
      toast.success(`was empty bucket-${bucketName} successfully.`, alertConfirmOption);
    }
    else
    {
      toast.error(`Can\'t empty bucket -${bucketName}, unfortunatley. Maybe bucket can conaints some files.`, alertConfirmOption);
    }
  }

  const createBucket = async () => {
    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}s3Bucket/${bucketName}/${region}`, requestOptions)
    let ret = await response.json()
    if (response.status === 200 && ret.result) {
      toast.success('Created new bucket successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Can\'t create new bucket, unfortunatley. Maybe bucket name is duplicated.', alertConfirmOption);
    }
  }

  const onContent = () => {
    saveToLocalStorage({allBuckets: allBuckets}, 's3Buckets')
  }

  const deleteBucketConfirm = (bucketName, reg) => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete this bucket.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => deleteBucket(bucketName, reg)
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  };

  const emptyBucketConfirm = (bucketName, reg) => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to empty this bucket.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => emptyBucket(bucketName, reg)
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  };

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
      <CRow>
        <CCol>
          <CCard className="mb-4">
            <CCardHeader>S3 Bucket Managment</CCardHeader>
            <CCardBody>
              <CForm
                className="row g-3 needs-validation"
                noValidate
              >
                <CRow className='mt-2'>
                  <CCol>
                  <CFormInput
                      type="text"
                      id="bucketName"
                      placeholder="Please Input Bucket Name."
                      value={bucketName}
                      onChange={(e) => setBucketName(e.target.value)}
                    />
                  </CCol>
                  <CCol>
                    <CFormSelect id="regionSelect" value={region} onChange={(obj) => setRegion(obj.target.value)} size="sm" className="mb-3" aria-label="Small select example">
                      {
                        regionMap.map((regionItem, index) => {
                            return (<option key={index} value={regionItem.value}>{regionItem.region}</option>)
                          })
                      }
                    </CFormSelect>
                  </CCol>
                  <CCol>
                    <CButton color="primary" onClick={() => createBucket()}>Add New Hosting Bucket</CButton>
                    &nbsp;<CButton color="success" onClick={() => refreshBucket()}>Refresh</CButton>
                  </CCol>
                </CRow>
                <CRow className='mt-2'>
                  <CCol>
                  <table className="table">
                    <thead>
                      <tr>
                        <th className='text-center'>No</th>
                        <th className='text-center'>Bucket Name</th>
                        <th className='text-center'>Region</th>
                        <th className='text-center'>Create Date</th>
                        <th className='text-center'>Action</th>
                        <th className='text-center'>Status</th>
                      </tr>
                    </thead>
                    <tbody>
                      {
                        loading ? (
                          <p>
                            <em>Loading...</em>
                          </p>
                        ) : (
                          allBuckets.map((bucket) => {
                            //if (article.content != null && article.content.length > 0)
                            {
                              return (<tr key={bucket.index}>
                                <td className='text-center'><CFormCheck id={bucket.index+""} label={bucket.index}
                                    // checked={checkedItem[String(bucket.name)].checked}
                                    onChange={(e) => {
                                      var ret = checkedItem
                                      ret[String(bucket.name)].checked = e.target.checked
                                      setCheckedItem(ret)
                                      console.log(checkedItem)
                                      console.log(e.target.checked, checkedItem[String(bucket.name)].checked)
                                    }}/>
                                </td>
                                <td>{bucket.name}</td>
                                <td className='text-center'>{bucket.region}</td>
                                <td className='text-center'>{bucket.createDate}</td>
                                <td className='text-center'>
                                  <Link to={`/amazone/contents`} state={{ domainId:'1234567890', domain:bucket.name, region: bucket.region }}>
                                    <CButton type="button" onClick={() => onContent()}>Content</CButton>
                                  </Link>
                                  &nbsp;
                                  <CButton
                                    type="button"
                                    onClick={() => emptyBucketConfirm(bucket.name, bucket.region)}
                                  >
                                    Empty
                                  </CButton>
                                  &nbsp;
                                  <CButton
                                    type="button"
                                    onClick={() => deleteBucketConfirm(bucket.name, bucket.region)}
                                  >
                                    Delete
                                  </CButton>
                                </td>
                                <td className='text-center'>
                                  {"Public"}
                                </td>
                              </tr>)
                            }
                          })
                        )
                      }
                      {/* {allBuckets.length > 0 && (
                      <tr>
                        <td colSpan={4}>
                          <table>
                            <tr>
                              <td>
                                <CFormCheck id="checkAll" label="Chceck All | Selected" 
                                  onChange={(e) => {
                                    var checkedItem = checkedItem
                                    Object.keys(checkedItem).map((item)=>{
                                      checkedItem[item].checked = e.target.checked
                                      //console.log(item)
                                    })
                                    setCheckedItem(checkedItem)
                                    //console.log(e.target.checked, this.state.checkedItem[article.id])
                                  }}
                                />
                              </td>
                              <td className='px-2'>
                                <CButton onClick={() => this.setArticleState(2)}>Delete</CButton>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                      )} */}
                    </tbody>
                  </table>
                  </CCol>
                </CRow>
              </CForm>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  )
}

export default Bucket
