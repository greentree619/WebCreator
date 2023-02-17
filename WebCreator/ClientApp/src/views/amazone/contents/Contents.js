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
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { alertConfirmOption } from 'src/utility/common'

const Contents = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [allFiles, setAllFiles] = useState([])
  const [selectedFile, setSelectedFile] = useState(null)
  //console.log(location.state)
  if (location.state == null && location.search.length > 0) {
    location.state = { 
      domain: new URLSearchParams(location.search).get('domain'),
      domainId: new URLSearchParams(location.search).get('domainId'),
      region: new URLSearchParams(location.search).get('region'),
   }
  }

  console.log(location.state)

  useEffect(() => {
    populateData()
  }, [])

  const populateData = async () => {
    setLoading(true);
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}s3Bucket/contents/${location.state.domain}/${location.state.region}`,
    )
    const data = await response.json()
    //console.log("<--", data);
    if (response.status === 200 && data) {
      setAllFiles(data.result)
    }
    setLoading(false)
  }

  const refreshBucket = async() => {
    populateData()
  }

  async function uploadHandler() {
    if(selectedFile == null){
      alert('Please select theme zip file.');
      return;
    }

    const data = new FormData();
    data.append(`theme`, selectedFile, selectedFile.name);

    const requestOptions = {
      method: 'POST',
      body: data,
    }

    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}s3Bucket/upload/${location.state.domainId}/${location.state.domain}`,
      requestOptions,
    )

    // setAlertColor('danger');
    // setAlertMsg('Theme upload is failed, unfortunatley.');
    let ret = await response.json()
    if (response.status === 200 && ret) {
      // setAlertColor('success');
      // setAlertMsg('Zip file was created successfully.');
      toast.success('Zip file was Upload successfully.', alertConfirmOption);
    }
    else
    {
      toast.error('Zip file upload is failed, unfortunatley.', alertConfirmOption);
    }
    // setAlarmVisible(true);
  }

  const handleFileReader = (event) => {
    setSelectedFile(event.target.files[0]);
  }

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
            <CCardHeader>Bucket-{location.state.domain}({location.state.region}) Contents</CCardHeader>
            <CCardBody>
              <CForm
                className="row g-3 needs-validation"
                noValidate
              >
                <CRow className='mt-2'>
                  <CCol>
                    <CFormInput type="file" 
                          id="formFile" onChange={handleFileReader}/>
                  </CCol>
                  <CCol>
                    <CButton color="primary" onClick={uploadHandler}>Upload Zip File</CButton>
                  </CCol>
                  <CCol className='d-flex justify-content-right'>
                    <CButton color="success" onClick={() => refreshBucket()}>Refresh</CButton>
                  </CCol>
                  <CCol>
                    <CButton color="primary" onClick={() => navigate(-1)}>Back</CButton>
                  </CCol>
                </CRow>
                <CRow className='mt-2'>
                  <CCol>
                  <table className="table">
                    <thead>
                      <tr>
                        <th>FileName</th>
                        <th>Modified Date</th>
                        <th>Size</th>
                      </tr>
                    </thead>
                    <tbody>
                    {
                        loading ? (
                          <p>
                            <em>Loading...</em>
                          </p>
                        ) : (
                          allFiles.map((file) => {
                            //if (article.content != null && article.content.length > 0)
                            {
                              return (<tr key={file.key}>
                                <td>
                                  {file.key}
                                </td>
                                <td>{file.date}</td>
                                <td>{file.size}</td>
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

export default Contents