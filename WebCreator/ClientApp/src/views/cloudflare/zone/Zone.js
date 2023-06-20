import React, { Component } from 'react'
import {
  CCard,
  CCardHeader,
  CCardBody,
  CButton,
  CAlert,
  CPagination,
  CPaginationItem,
  CRow,
  CCol,
  CContainer,
} from '@coreui/react'
import { DocsLink } from 'src/components'
import { Outlet, Link, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import {saveToLocalStorage, loadFromLocalStorage, clearLocalStorage } from 'src/utility/common.js'
import { ToastContainer, toast } from 'react-toastify'
import 'react-toastify/dist/ReactToastify.css'
import { alertConfirmOption } from 'src/utility/common'
import { confirmAlert } from 'react-confirm-alert'
import 'react-confirm-alert/src/react-confirm-alert.css'; // Import css

class ZoneBase extends Component {
  static displayName = ZoneBase.name

  constructor(props) {
    super(props)
    this.state = {
      listData: [],
      loading: true,
      alarmVisible: false,
      alertMsg: '',
      alertColor: 'success',
      curPage: 1,
      totalPage: 1,
    }
  }

  componentDidMount() {
    this.populateData(1)
  }

  gotoPrevPage() {
    this.populateData(this.state.curPage - 1)
  }

  gotoNextPage() {
    this.populateData(this.state.curPage + 1)
  }

  savePageState = () => {
    saveToLocalStorage({listData: this.state.listData, curPage: this.state.curPage, totalPage: this.state.totalPage}, 'zoneList')
  }

  deleteZoneConfirm = (zoneName) => {
    confirmAlert({
      title: 'Warnning',
      message: 'Are you sure to delete this zone.',
      buttons: [
        {
          label: 'Yes',
          onClick: () => this.deleteZone(zoneName)
        },
        {
          label: 'No',
          onClick: () => {return false;}
        }
      ]
    });
  };

  async deleteZone(zoneName) {
    const requestOptions = {
      method: 'GET'
    }
    fetch(`${process.env.REACT_APP_SERVER_URL}dns/deleteZone/${zoneName}`, requestOptions)
      .then((response) => {
        if (response.status === 200) {
          response.json().then(data => {
            if( data.result )
            {
              console.log('success:', data.result)
              toast.success('Zone \"' + zoneName + '\" was deleted successfully.', alertConfirmOption);
            }
            else
            {
              console.log('failed:', data.result)
              toast.error('Failed to delete this zone.', alertConfirmOption);
            }
          })
        }
      })
      .catch((err) => console.log(err))
  }

  renderProjectsTable(state) {
    let pageButtonCount = 3
    let pagination = <p></p>

    if (this.state.totalPage > 1) {
      let prevButton = (
        <CPaginationItem onClick={() => this.gotoPrevPage()}>Previous</CPaginationItem>
      )
      if (state.curPage <= 1) prevButton = <CPaginationItem disabled>Previous</CPaginationItem>

      let nextButton = <CPaginationItem onClick={() => this.gotoNextPage()}>Next</CPaginationItem>
      if (state.curPage >= state.totalPage)
        nextButton = <CPaginationItem disabled>Next</CPaginationItem>

      var pageNoAry = []
      var startNo = state.curPage - pageButtonCount
      var endNo = state.curPage + pageButtonCount
      if (startNo < 1) {
        startNo = 1
        endNo =
          pageButtonCount * 2 + 1 > state.totalPage ? state.totalPage : pageButtonCount * 2 + 1
      } else if (endNo > state.totalPage) {
        endNo = state.totalPage
        startNo = endNo - pageButtonCount * 2 > 1 ? endNo - pageButtonCount * 2 : 1
      }

      for (var i = startNo; i <= endNo; i++) {
        if (i < 1 || i > state.totalPage) continue
        pageNoAry.push(i)
      }

      const paginationItems = pageNoAry.map((number) => (
        <CPaginationItem
          key={number}
          onClick={() => this.populateData(number)}
          active={number == state.curPage}
        >
          {number}
        </CPaginationItem>
      ))

      pagination = (
        <CPagination align="center" aria-label="Page navigation example">
          {prevButton}
          {paginationItems}
          {nextButton}
        </CPagination>
      )
    }

    return (
      <>
        <CAlert
          color={state.alertColor}
          dismissible
          visible={state.alarmVisible}
          onClose={() => this.setState({ alarmVisible: false })}
        >
          {state.alertMsg}
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
        <table className="table">
          <thead>
            <tr>
              <th className='text-center'>Id</th>
              <th className='text-center'>Zone</th>
              <th className='text-center'>Status</th>
              <th className='text-center'>Name servers</th>
              <th className='text-center'>Action</th>
            </tr>
          </thead>
          <tbody>
            {state.listData.map((row) => (
              <tr key={row.id}>
                <td className='text-center'>{row.id}</td>
                <td>
                  <Link onClick={()=>this.savePageState()} to={`/cloudflare/dns`} state={{ zoneId: row.id, zoneName: row.name }}>
                    {row.name}
                  </Link>
                </td>
                <td className='text-center'>{row.status}</td>
                <td>
                  {row.name_servers.map((serv, index) => {
                    return serv + ', '
                  })}
                </td>
                <td className='text-center'>
                  <CButton type="button" onClick={() => this.deleteZoneConfirm(row.name)}>
                    Delete
                  </CButton>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {pagination}
      </>
    )
  }

  render() {
    let contents = this.state.loading ? (
      <p>
        <em>Loading...</em>
      </p>
    ) : (
      this.renderProjectsTable(this.state)
    )
    return (
      <>
        <CCard className="mb-4">
          <CCardHeader>
            <CContainer>
              <CRow>
                <CCol className="align-self-start">All Zones</CCol>
                <CCol className="align-self-end" xs="auto">
                  {/* <CButton
                    type="button"
                    onClick={() => this.props.navigate(-1)}
                  >
                    Back
                  </CButton> */}
                </CCol>
              </CRow>
            </CContainer>
          </CCardHeader>
          <CCardBody>{contents}</CCardBody>
        </CCard>
      </>
    )
  }

  async populateData(pageNo) {
    var zoneList = loadFromLocalStorage('zoneList')
    if(zoneList != null && zoneList != undefined)
    {
      //console.log(zoneList)
      this.setState({
        listData: zoneList.listData,
        curPage: zoneList.curPage,
        totalPage: zoneList.totalPage,
        loading: false,
      })
      clearLocalStorage('zoneList')
      return
    }

    const response = await fetch(`${process.env.REACT_APP_SERVER_URL}dns/` + pageNo + '/200')
    const data = await response.json()
    this.setState({
      listData: data.result,
      loading: false,
      alarmVisible: false,
      curPage: data.curPage,
      totalPage: data.total,
    })
  }
}

ZoneBase.propTypes = {
  navigate: PropTypes.any,
}

const Zone = (props) => {
  const navigate = useNavigate()

  // useEffect(() => {
  // }, [])
  return <ZoneBase navigate ={navigate} {...props} />
}
export default Zone
