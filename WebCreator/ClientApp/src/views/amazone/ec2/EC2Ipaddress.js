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

const EC2Ipaddress = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [allIPAddr, setAllIPAddr] = useState([])
  const [ipAddrMap, setIPAddrMap] = useState({})
  const [ipAddr, setIPAddr] = useState("")

  useEffect(() => {
    populateData()
  }, [])

  const populateData = async () => {
    setLoading(true);
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}EC2IPAddress/`,
    )
    const data = await response.json()
    var tmpMap = {}
    await data.data.map((item, index) => {
      tmpMap[item.id] = item.ipAddress
      //console.log(ids, "<--", articleDocumentIds);
    });
    setIPAddrMap(tmpMap)
    setLoading(false)
    setAllIPAddr(data.data)
    console.log(data.data)
    console.log("ipAddrMap=>", ipAddrMap)
  }

  const refreshAllIPAddr = async() => {
    populateData()
  }

  const deleteIPAddr = async (id) => {
    const requestOptions = {
      method: 'DELETE',
      headers: { 'Content-Type': 'application/json' }
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}EC2IPAddress?docId=${id}`, requestOptions)
    let ret = await response.json()
    console.log(ret)
    if (response.status === 200 && ret) {
      toast.success(`Deleted IP Address successfully.`, alertConfirmOption);
    }
    else
    {
      toast.error(`Can\'t delete IP Address, unfortunatley.`, alertConfirmOption);
    }
  }

  const changeIPAddr = async (id, ipaddr) => {
    console.log("changeIPAddr=>", id, ipaddr)
    var ret = {...ipAddrMap}
    ret[id] = ipaddr
    setIPAddrMap(ret)
  }

  const updateIpaddress = async (id) => {
    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: id,
        ipAddress: ipAddrMap[id],
        projectId: "",
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}EC2IPAddress`, requestOptions)
    let ret = await response.json()
    console.log(ret)
    if (response.status === 200 && ret) {
      toast.success(`Updated IP Address successfully.`, alertConfirmOption);
    }
    else
    {
      toast.error(`Can\'t update IP Address, unfortunatley.`, alertConfirmOption);
    }
  }

  const addIPAddress = async () => {
    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ipAddress: ipAddr,
        projectId: "",
      }),
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}EC2IPAddress`, requestOptions)
    let ret = await response.json()
    if (response.status === 200 && ret) {
      toast.success('Added new IP Address successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Can\'t add new IP Address, unfortunatley. Maybe IP Address is duplicated.', alertConfirmOption);
    }
  }

  const deleteIPAddressConfirm = (id) => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete this IP Address.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => deleteIPAddr(id)
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
      <CRow>
        <CCol>
          <CCard className="mb-4">
            <CCardHeader>EC2 IP Address Managment</CCardHeader>
            <CCardBody>
              <CForm
                className="row g-3 needs-validation"
                noValidate
              >
                <CRow className='mt-2'>
                  <CCol>
                  <CFormInput
                      type="text"
                      id="ipAddress"
                      placeholder="Please Input IP Address."
                      value={ipAddr}
                      onChange={(e) => setIPAddr(e.target.value)}
                    />
                  </CCol>
                  <CCol>
                    <CButton color="primary" onClick={() => addIPAddress()}>Add New IP Address</CButton>
                    &nbsp;<CButton color="success" onClick={() => refreshAllIPAddr()}>Refresh</CButton>
                  </CCol>
                </CRow>
                <CRow className='mt-2'>
                  <CCol>
                  <table className="table">
                    <thead>
                      <tr>
                        <th className='text-center'>No</th>
                        <th className='text-center'>IP Address</th>
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
                          allIPAddr.map((ipaddrItem) => {
                            //if (article.content != null && article.content.length > 0)
                            {
                              return (<tr key={ipaddrItem.id}>
                                <td className='text-center'>
                                  <CFormCheck id={ipaddrItem.id+""} label={ipaddrItem.id}/>
                                </td>
                                <td>
                                  <CFormInput
                                    key={ipaddrItem.id}
                                    type="text"
                                    id="ipAddress"
                                    placeholder="Please Input IP Address."
                                    value={ipAddrMap[ipaddrItem.id]}
                                    onChange={(e) => changeIPAddr(ipaddrItem.id, e.target.value)}
                                  />
                                  
                                </td>
                                <td className='text-center'>{ipaddrItem.createdTime}</td>
                                <td className='text-center'>
                                  <CButton
                                    type="button"
                                    onClick={() => updateIpaddress(ipaddrItem.id)}
                                  >
                                    Update
                                  </CButton>
                                  &nbsp;
                                  <CButton
                                    type="button"
                                    onClick={() => deleteIPAddressConfirm(ipaddrItem.id)}
                                  >
                                    Delete
                                  </CButton>
                                </td>
                                <td className='text-center'>
                                </td>
                              </tr>)
                            }
                          })
                        )
                      }
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

export default EC2Ipaddress
