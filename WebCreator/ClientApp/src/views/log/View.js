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
  CTabs,
  CNav,
  CNavItem,
  CNavLink,
  CTabContent,
  CTabPane,
} from '@coreui/react'
import { rgbToHex } from '@coreui/utils'
import { DocsLink } from 'src/components'
import { useLocation, useNavigate } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { Outlet, Link } from 'react-router-dom'
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import {saveToLocalStorage, globalRegionMap, loadFromLocalStorage, clearLocalStorage, alertConfirmOption } from 'src/utility/common'
import { confirmAlert } from 'react-confirm-alert';
import 'react-confirm-alert/src/react-confirm-alert.css'; 

const View = (props) => {
  const location = useLocation()
  const dispatch = useDispatch()
  const activeProject = useSelector((state) => state.activeProject)

  if (location.search.length > 0) {
    //console.log()
    location.state = { project: activeProject }
    //console.log(location)
  }
  
  const [alertColor, setAlertColor] = useState('success')
  const [alertMsg, setAlertMsg] = useState('')
  const [activeKey, setActiveKey] = useState(1)
  const [lineCount, setLineCount] = useState(300)
  const [isRefresh, setIsRefresh] = useState(false)
  const [logContent, setLogContent] = useState([])

  useEffect(() => {
    dispatch({ type: 'set', activeTab: "log_view" })
    //console.log(activeProject.id)
    populateArticleData()
  }, [isRefresh, lineCount, activeKey])

  const getCategory = () => {
    var cate = "scrap"
    switch(activeKey){
      case 1:
        {
          cate = "question"
          break
        }
      case 2:
        {
          cate = "scrap"
        break
        }
      case 3:
        {
          cate = "publish"
          break
        }
    }
    return cate
  }

  async function populateArticleData() {
    //console.log("populateArticleData", activeProject.id)
    var cate = getCategory()
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}log/Read/${activeProject.id}/${cate}/${lineCount}`,
    )
    const data = await response.json()
    //console.log(data)
    setLogContent(data.result)
    // let ids = "";
    // await _data.map((item, index) => {
    //   if (ids.length > 0) ids += ",";
    //   ids += item.id;

    //   setTimeout(() => {
    //     this.loadSyncStatus(item.id);
    //   }, 100 * index);
    //   console.log("sync view<--", index);
    // });
    //Omitted this.loadSyncStatus(ids);
  }

  const deleteLogConfirm = () => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete this log.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => clearLog()
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  };

  const clearLog = async () => {
    var cate = getCategory()
    const response = await fetch(
      `${process.env.REACT_APP_SERVER_URL}log/Delete/${activeProject.id}/${cate}`,
    )
  }

  return (
    <>
      <CContainer className="px-4">
        <CRow xs={{ gutterX: 5 }}>
          <CCol>
            <CCard className="mb-4">
              <CCardHeader>Log View</CCardHeader>
              <CCardBody>
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
                <CNav className='d-flex' variant="tabs" role="tablist">
                  <CNavItem>
                    <CNavLink
                      href={void(0)}
                      active={activeKey === 1}
                      onClick={() => setActiveKey(1)}
                    >
                      Scrap Questions
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      href={void(0)}
                      active={activeKey === 2}
                      onClick={() => setActiveKey(2)}
                    >
                      Scrap Articles
                    </CNavLink>
                  </CNavItem>
                  <CNavItem>
                    <CNavLink
                      href={void(0)}
                      active={activeKey === 3}
                      onClick={() => setActiveKey(3)}
                    >
                      Publish
                    </CNavLink>
                  </CNavItem>
                  <CNavItem className='flex-grow-1'></CNavItem>
                  <CNavItem className='ms-1'>
                    <CFormInput style={{width:'70px'}} value={lineCount} type='number'
                    onChange={(e) => setLineCount(e.target.value)}/>
                  </CNavItem>
                  <CNavItem className=''>
                    &nbsp;
                    <Link>
                      <CButton type="button" color="success" onClick={() => setIsRefresh(!isRefresh)}>Refresh</CButton>
                    </Link>
                  </CNavItem>
                  <CNavItem className=''>
                    &nbsp;
                    <Link>
                      <CButton type="button" color="danger" onClick={() => deleteLogConfirm()}>Clear</CButton>
                    </Link>
                  </CNavItem>
                </CNav>
                <CTabContent>
                  <CTabPane role="tabpanel" aria-labelledby="home-tab" visible={true}>
                    <div className={'mb-12 d-grid gap-2 col-12 mx-auto'} style={logContent.length == 0 ? {minHeight:'300px'} : {minHeight:'0px'}}>
                      {
                        logContent.map((item, index) =>
                          {
                            return (<p key={index} className="lh-sm">{item}</p>)
                          }
                        )
                      }
                      {/* <p className="lh-sm">This is a long paragraph written to show how the line-height of an element is affected by our utilities. Classes are applied to the element itself or sometimes the parent element. These classes can be customized as needed with our utility API.</p>
                      <p className="lh-sm">This is a long paragraph written to show how the line-height of an element is affected by our utilities. Classes are applied to the element itself or sometimes the parent element. These classes can be customized as needed with our utility API.</p> */}
                    </div>
                  </CTabPane>
                </CTabContent>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      </CContainer>
    </>
  )
}

export default View
